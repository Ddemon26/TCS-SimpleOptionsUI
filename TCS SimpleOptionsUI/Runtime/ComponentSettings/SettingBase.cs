using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using Component = UnityEngine.Component;
using Object = UnityEngine.Object;

namespace TCS.SimpleOptionsUI {
    public interface ISettingBase {
        VisualElement CreateUIElement(VisualTreeAsset template);
        void Dispose();
    }
    [Serializable] public abstract class SettingBase : IDisposable/*, ISettingBase*/ {
        public string m_label;
        public Object m_targetObject; // Reference to ScriptableObject, GameObject, or Component
        public string m_variableName;

        protected StyleSheet StyleSheet;
        protected void SetStyleSheet(StyleSheet styleSheet) => StyleSheet = styleSheet;
        protected PropertyChangedEventHandler PropertyChangedHandler;

        protected const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Reflection Cache: Thread-safe
        static readonly ConcurrentDictionary<string, MemberInfo> ReflectionCache = new();

        // Cached MemberInfo instance
        MemberInfo m_cachedMemberInfo;

        protected bool ValidateTargetAndVariableName(out string errorMessage) {
            errorMessage = string.Empty;
            if (!m_targetObject) {
                errorMessage = $"Target Object is not set for setting: {m_label}";
                return false;
            }

            if (string.IsNullOrEmpty(m_variableName)) {
                errorMessage = $"Variable Name is not set for setting: {m_label}";
                return false;
            }

            return true;
        }

        protected virtual object GetActualTargetObject() {
            if (m_targetObject is not GameObject go) return m_targetObject;
            // If the target is a GameObject, extract the component
            string[] splitName = m_variableName.Split('/');
            if (splitName.Length != 2) {
                Debug.LogError($"Variable name '{m_variableName}' is not in the format 'ComponentName/FieldName'.");
                return null;
            }

            string componentName = splitName[0];
            var component = go.GetComponents<Component>().FirstOrDefault(comp => comp.GetType().Name == componentName);
            if (component) return component;

            Debug.LogError($"Component '{componentName}' not found on GameObject '{go.name}'.");
            return null;
        }

        protected MemberInfo GetMemberInfo() {
            // Return cached member info if available
            if (m_cachedMemberInfo != null) return m_cachedMemberInfo;

            // Get the actual target object
            object actualTarget = GetActualTargetObject();
            if (actualTarget == null) return null;

            string memberName = m_variableName;

            // If the target is a GameObject, extract 'ComponentName/MemberName'
            if (m_targetObject is GameObject) {
                string[] splitName = m_variableName.Split('/');
                if (splitName.Length != 2) {
                    Debug.LogError($"Variable name '{m_variableName}' is not in the format 'ComponentName/MemberName'.");
                    return null;
                }

                memberName = splitName[1];
            }

            // Retrieve and cache the member info
            m_cachedMemberInfo = RetrieveMemberInfo(actualTarget, memberName);
            return m_cachedMemberInfo;
        }

        MemberInfo RetrieveMemberInfo(object actualTarget, string memberName) {
            var targetType = actualTarget.GetType();
            var cacheKey = $"{targetType.FullName}.{memberName}";

            // Check cache for the member info
            if (ReflectionCache.TryGetValue(cacheKey, out var cachedInfo)) {
                return cachedInfo;
            }

            // Attempt to get the field, property, or method
            var memberInfo = targetType.GetField(memberName, BINDING_FLAGS)
                             ?? targetType.GetProperty(memberName, BINDING_FLAGS)
                             ?? (MemberInfo)targetType.GetMethod(memberName, BINDING_FLAGS);

            // Cache the result if found
            if (memberInfo != null) {
                ReflectionCache.TryAdd(cacheKey, memberInfo);
                return memberInfo;
            }

            // Assert if no member was found
            Debug.Assert(false, $"Member '{memberName}' not found on type '{targetType.Name}'.");
            return null;
        }

        /// <summary>
        /// Template method to create the UI element with common steps handled.
        /// Derived classes implement SetupUIElement for specific UI configurations.
        /// </summary>
        public VisualElement CreateUIElement(VisualTreeAsset template) {
            if (!ValidateTargetAndVariableName(out string errorMessage)) {
                Debug.LogError(errorMessage);
                return null;
            }

            object actualTarget = GetActualTargetObject();
            if (actualTarget == null) return null;

            var memberInfo = GetMemberInfo();
            if (memberInfo == null) return null;

            VisualElement container = template.CloneTree();
            var label = container.Q<Label>();
            if (label != null) {
                label.text = m_label;
            }
            else {
                Debug.LogWarning($"No Label found in the template for setting '{m_label}'.");
            }

            SetupUIElement(container, actualTarget, memberInfo);
            return container;
        }

        /// <summary>
        /// Derived classes implement this to configure specific UI elements and bindings.
        /// </summary>
        protected abstract void SetupUIElement(VisualElement container, object actualTarget, MemberInfo memberInfo);

        public virtual void Dispose() {
            m_cachedMemberInfo = null;
            m_label = null;
            m_variableName = null;
            // Note: Do not nullify m_targetObject as it's managed by Unity

            if (PropertyChangedHandler != null && m_targetObject is INotifyPropertyChanged propertyChanged) {
                propertyChanged.PropertyChanged -= PropertyChangedHandler;
                PropertyChangedHandler = null;
            }
        }
    }
}