using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using Component = UnityEngine.Component;
using Object = UnityEngine.Object;
namespace TCS.SimpleOptionsUI {
    [Serializable]
    public abstract class SettingBase {
        public string m_label;
        public Object m_targetObject; // Reference to ScriptableObject, GameObject, or Component
        public string m_variableName;

        protected const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

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

        protected object GetActualTargetObject() {
            if (m_targetObject is GameObject go) {
                // If the target is a GameObject, extract the component
                string[] splitName = m_variableName.Split('/');
                if (splitName.Length != 2) {
                    Debug.LogError($"Variable name '{m_variableName}' is not in the format 'ComponentName/FieldName'.");
                    return null;
                }

                string componentName = splitName[0];
                var component = go.GetComponents<Component>().FirstOrDefault(comp => comp.GetType().Name == componentName);
                if (!component) {
                    Debug.LogError($"Component '{componentName}' not found on GameObject '{go.name}'.");
                    return null;
                }

                return component;
            }

            // For ScriptableObjects or Components, return the object itself
            return m_targetObject;
        }

        protected MemberInfo GetMemberInfo() {
            object actualTarget = GetActualTargetObject();
            if (actualTarget == null) return null;

            string memberName = m_variableName;

            if (m_targetObject is GameObject) {
                // Extract the field/property name from 'ComponentName/FieldName'
                string[] splitName = m_variableName.Split('/');
                if (splitName.Length != 2) {
                    Debug.LogError($"Variable name '{m_variableName}' is not in the format 'ComponentName/FieldName'.");
                    return null;
                }

                memberName = splitName[1];
            }

            var targetType = actualTarget.GetType();

            // First, try to get the field
            var fieldInfo = targetType.GetField(memberName, BINDING_FLAGS);
            if (fieldInfo != null) return fieldInfo;

            // If no field found, try to get the property
            var propInfo = targetType.GetProperty(memberName, BINDING_FLAGS);
            if (propInfo != null) return propInfo;

            Debug.LogError($"Member '{memberName}' not found on type '{targetType.Name}'.");
            return null;
        }

        // Abstract method for creating the UI element
        public abstract VisualElement CreateUIElement(VisualTreeAsset template);
    }

    [Serializable]
    public class FloatSetting : SettingBase {
        public float m_minValue;
        public float m_maxValue;

        public override VisualElement CreateUIElement(VisualTreeAsset template) {
            if (!ValidateTargetAndVariableName(out string errorMessage)) {
                Debug.LogError(errorMessage);
                return null;
            }

            object actualTarget = GetActualTargetObject();
            if (actualTarget == null) return null;

            var memberInfo = GetMemberInfo();
            if (memberInfo == null) return null;

            VisualElement container = template.CloneTree();
            container.Q<Label>().text = m_label;

            var slider = container.Q<Slider>();
            slider.lowValue = m_minValue;
            slider.highValue = m_maxValue;

            if (memberInfo is FieldInfo fieldInfo && fieldInfo.FieldType == typeof(float)) {
                var currentValue = (float)fieldInfo.GetValue(actualTarget);
                slider.value = currentValue;

                slider.RegisterValueChangedCallback
                (
                    evt => {
                        fieldInfo.SetValue(actualTarget, evt.newValue);
                    }
                );
            }
            else if (memberInfo is PropertyInfo propInfo && propInfo.PropertyType == typeof(float)) {
                var currentValue = (float)propInfo.GetValue(actualTarget);
                slider.value = currentValue;

                slider.RegisterValueChangedCallback
                (
                    evt => {
                        propInfo.SetValue(actualTarget, evt.newValue);
                    }
                );

                if (actualTarget is INotifyPropertyChanged property) {
                    // Update the slider when the property changes
                    property.PropertyChanged += (_, args) => {
                        if (args.PropertyName == propInfo.Name) {
                            slider.SetValueWithoutNotify((float)propInfo.GetValue(actualTarget));
                        }
                    };
                }
            }
            else {
                Debug.LogError($"Member '{memberInfo.Name}' on object '{actualTarget}' is not a float.");
                return null;
            }

            return container;
        }
    }

    [RequireComponent(typeof(UIDocument))]
    public class MainSettingsMenu : MonoBehaviour {
        [SerializeField] UIDocument m_uiDocument;
        [SerializeField] VisualTreeAsset m_floatSliderSetting;
        [SerializeField] VisualTreeAsset m_intSliderSetting;
        [SerializeField] VisualTreeAsset m_enumSetting;
        [SerializeReference] List<SettingBase> m_settings = new();

        void Start() {
            m_uiDocument ??= GetComponent<UIDocument>();
            var root = m_uiDocument.rootVisualElement;

            foreach (var setting in m_settings) {
                var uiElement = setting.CreateUIElement(GetTemplateForSetting(setting));
                if (uiElement != null) {
                    root.Add(uiElement);
                }
            }
        }

        VisualTreeAsset GetTemplateForSetting(SettingBase setting) {
            return setting switch {
                FloatSetting => m_floatSliderSetting,
                _ => null
            };
        }
    }
}