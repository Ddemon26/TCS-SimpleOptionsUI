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
    [Serializable] public abstract class SettingBase : IDisposable {
        public string m_label;
        public Object m_targetObject; // Reference to ScriptableObject, GameObject, or Component
        public string m_variableName;

        protected StyleSheet StyleSheet;
        protected void SetStyleSheet(StyleSheet styleSheet) => StyleSheet = styleSheet;
        protected private PropertyChangedEventHandler PropertyChangedHandler;

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
            if (m_cachedMemberInfo != null) return m_cachedMemberInfo;

            object actualTarget = GetActualTargetObject();
            if (actualTarget == null) return null;

            string memberName = m_variableName;

            if (m_targetObject is GameObject) {
                // Extract the field/property/method name from 'ComponentName/MemberName'
                string[] splitName = m_variableName.Split('/');
                if (splitName.Length != 2) {
                    Debug.LogError($"Variable name '{m_variableName}' is not in the format 'ComponentName/MemberName'.");
                    return null;
                }

                memberName = splitName[1];
            }

            var targetType = actualTarget.GetType();

            // Generate a unique cache key based on target type and member name
            var cacheKey = $"{targetType.FullName}.{memberName}";

            // Attempt to retrieve from cache
            if (ReflectionCache.TryGetValue(cacheKey, out var cachedInfo)) {
                m_cachedMemberInfo = cachedInfo;
                return m_cachedMemberInfo;
            }

            // First, try to get the field
            var fieldInfo = targetType.GetField(memberName, BINDING_FLAGS);
            if (fieldInfo != null) {
                ReflectionCache.TryAdd(cacheKey, fieldInfo);
                m_cachedMemberInfo = fieldInfo;
                return m_cachedMemberInfo;
            }

            // If no field found, try to get the property
            var propInfo = targetType.GetProperty(memberName, BINDING_FLAGS);
            if (propInfo != null) {
                ReflectionCache.TryAdd(cacheKey, propInfo);
                m_cachedMemberInfo = propInfo;
                return m_cachedMemberInfo;
            }

            // If no property found, try to get the method
            var methodInfo = targetType.GetMethod(memberName, BINDING_FLAGS);
            if (methodInfo != null) {
                ReflectionCache.TryAdd(cacheKey, methodInfo);
                m_cachedMemberInfo = methodInfo;
                return m_cachedMemberInfo;
            }

            Debug.LogError($"Member '{memberName}' not found on type '{targetType.Name}'.");
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

    [Serializable] public abstract class SliderSettingBase<T> : SettingBase where T : struct, IConvertible, IComparable<T> {
        public T m_minValue;
        public T m_maxValue;

        protected override void SetupUIElement(VisualElement container, object actualTarget, MemberInfo memberInfo) {
            BaseSlider<T> slider = CreateSlider(container);
            if (slider == null) {
                Debug.LogError($"Unsupported slider type for setting '{m_label}'.");
                return;
            }

            slider.lowValue = m_minValue;
            slider.highValue = m_maxValue;

            BindSlider(actualTarget, memberInfo, slider);
        }

        protected abstract BaseSlider<T> CreateSlider(VisualElement container);

        void BindSlider(object actualTarget, MemberInfo memberInfo, BaseSlider<T> slider) {
            switch (memberInfo) {
                case FieldInfo fieldInfo when fieldInfo.FieldType == typeof(T):
                    BindField(actualTarget, fieldInfo, slider);
                    break;
                case PropertyInfo propInfo when propInfo.PropertyType == typeof(T):
                    BindProperty(actualTarget, propInfo, slider);
                    break;
                default:
                    Debug.LogError($"Member '{memberInfo.Name}' on object '{actualTarget}' is not of type {typeof(T)}.");
                    break;
            }
        }

        void BindField(object actualTarget, FieldInfo fieldInfo, BaseSlider<T> slider) {
            if (fieldInfo.GetValue(actualTarget) is T currentValue) {
                slider.value = currentValue;
            }

            slider.RegisterValueChangedCallback
            (
                evt => {
                    fieldInfo.SetValue(actualTarget, evt.newValue);
                }
            );
        }

        void BindProperty(object actualTarget, PropertyInfo propInfo, BaseSlider<T> slider) {
            if (propInfo.GetValue(actualTarget) is T currentValue) {
                slider.value = currentValue;
            }

            slider.RegisterValueChangedCallback
            (
                evt => {
                    propInfo.SetValue(actualTarget, evt.newValue);
                }
            );

            if (actualTarget is INotifyPropertyChanged property) {
                PropertyChangedHandler = (_, args) => {
                    if (args.PropertyName != propInfo.Name) return;

                    if (propInfo.GetValue(actualTarget) is T newValue) {
                        slider.SetValueWithoutNotify(newValue);
                    }
                    
                };
                property.PropertyChanged += PropertyChangedHandler;
            }
        }

        public override void Dispose() {
            base.Dispose();
            m_minValue = default;
            m_maxValue = default;
        }
    }

    [Serializable] public class FloatSliderSetting : SliderSettingBase<float> {
        protected override BaseSlider<float> CreateSlider(VisualElement container) => container.Q<Slider>();
    }

    [Serializable] public class IntSliderSetting : SliderSettingBase<int> {
        protected override BaseSlider<int> CreateSlider(VisualElement container) => container.Q<SliderInt>();
    }

    [Serializable] public class EnumFieldSetting : SettingBase {
        protected override void SetupUIElement(VisualElement container, object actualTarget, MemberInfo memberInfo) {
            var enumField = container.Q<EnumField>();
            if (enumField == null) {
                Debug.LogError($"Unsupported EnumField type for setting '{m_label}'.");
                return;
            }

            BindEnumField(actualTarget, memberInfo, enumField);
        }

        void BindEnumField(object actualTarget, MemberInfo memberInfo, EnumField enumField) {
            switch (memberInfo) {
                case FieldInfo { FieldType: { IsEnum: true } } info:
                    BindEnumField(actualTarget, info, enumField);
                    break;
                case PropertyInfo { PropertyType: { IsEnum: true } } info:
                    BindEnumProperty(actualTarget, info, enumField);
                    break;
                default:
                    Debug.LogError($"Member '{memberInfo.Name}' on object '{actualTarget}' is not an enum.");
                    break;
            }
        }

        void BindEnumField(object actualTarget, FieldInfo fieldInfo, EnumField enumField) {
            if (fieldInfo.GetValue(actualTarget) is Enum currentValue) {
                enumField.Init(currentValue);
            }

            enumField.RegisterValueChangedCallback
            (
                evt => {
                    fieldInfo.SetValue(actualTarget, evt.newValue);
                }
            );
        }

        void BindEnumProperty(object actualTarget, PropertyInfo propInfo, EnumField enumField) {
            if (propInfo.GetValue(actualTarget) is Enum currentValue) {
                enumField.Init(currentValue);
            }

            enumField.RegisterValueChangedCallback
            (
                evt => {
                    propInfo.SetValue(actualTarget, evt.newValue);
                }
            );

            if (actualTarget is INotifyPropertyChanged property) {
                PropertyChangedHandler = (_, args) => {
                    if (args.PropertyName != propInfo.Name) return;
                    if (propInfo.GetValue(actualTarget) is Enum newValue) {
                        enumField.SetValueWithoutNotify(newValue);
                    }
                };
                property.PropertyChanged += PropertyChangedHandler;
            }
        }

        public override void Dispose() {
            base.Dispose();
        }
    }

    [Serializable] public class ToggleFieldSetting : SettingBase {
        protected override void SetupUIElement(VisualElement container, object actualTarget, MemberInfo memberInfo) {
            var toggle = container.Q<Toggle>();
            if (toggle == null) {
                Debug.LogError($"Unsupported Toggle type for setting '{m_label}'.");
                return;
            }

            BindToggle(actualTarget, memberInfo, toggle);
        }

        void BindToggle(object actualTarget, MemberInfo memberInfo, Toggle toggle) {
            switch (memberInfo) {
                case FieldInfo fieldInfo when fieldInfo.FieldType == typeof(bool):
                    BindBoolField(actualTarget, fieldInfo, toggle);
                    break;
                case PropertyInfo propInfo when propInfo.PropertyType == typeof(bool):
                    BindBoolProperty(actualTarget, propInfo, toggle);
                    break;
                default:
                    Debug.LogError($"Member '{memberInfo.Name}' on object '{actualTarget}' is not of type bool.");
                    break;
            }
        }

        void BindBoolField(object actualTarget, FieldInfo fieldInfo, Toggle toggle) {
            if (fieldInfo.GetValue(actualTarget) is bool currentValue) {
                toggle.value = currentValue;
            }

            toggle.RegisterValueChangedCallback
            (
                evt => {
                    fieldInfo.SetValue(actualTarget, evt.newValue);
                }
            );
        }

        void BindBoolProperty(object actualTarget, PropertyInfo propInfo, Toggle toggle) {
            if (propInfo.GetValue(actualTarget) is bool currentValue) {
                toggle.value = currentValue;
            }

            toggle.RegisterValueChangedCallback
            (
                evt => {
                    propInfo.SetValue(actualTarget, evt.newValue);
                }
            );

            if (actualTarget is INotifyPropertyChanged property) {
                PropertyChangedHandler = (_, args) => {
                    if (args.PropertyName != propInfo.Name) return;
                    if (propInfo.GetValue(actualTarget) is bool newValue) {
                        toggle.SetValueWithoutNotify(newValue);
                    }
                };
                property.PropertyChanged += PropertyChangedHandler;
            }
        }

        public override void Dispose() {
            base.Dispose();
        }
    }

    [Serializable] public class ButtonFieldSetting : SettingBase {
        Button m_button;
        Action m_buttonClickHandler;
        public string m_buttonText;
        protected override void SetupUIElement(VisualElement container, object actualTarget, MemberInfo memberInfo) {
            m_button = container.Q<Button>();
            if (m_button == null) {
                Debug.LogError($"Unsupported Button type for setting '{m_label}'.");
                return;
            }

            BindButton(actualTarget, memberInfo, m_button);
            
            m_button.text = string.IsNullOrEmpty(m_buttonText) ? "Button" : m_buttonText;
        }

        void BindButton(object actualTarget, MemberInfo memberInfo, Button button) {
            if (memberInfo is MethodInfo methodInfo) {
                m_buttonClickHandler = () => methodInfo.Invoke(actualTarget, null);
                button.clicked += m_buttonClickHandler;
            }
            else {
                Debug.LogError($"Member '{memberInfo.Name}' on object '{actualTarget}' is not a method.");
            }
        }

        public override void Dispose() {
            base.Dispose();

            if (m_buttonClickHandler != null) {
                m_button.clicked -= m_buttonClickHandler;
                m_buttonClickHandler = null;
                m_button = null;
            }
        }
    }
}