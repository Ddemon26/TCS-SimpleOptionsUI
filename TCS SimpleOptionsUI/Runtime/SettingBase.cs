using System;
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

        protected StyleSheet StyleSheet;
        protected void SetStyleSheet(StyleSheet styleSheet) => StyleSheet = styleSheet;

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

            // First, try to get the field
            var fieldInfo = targetType.GetField(memberName, BINDING_FLAGS);
            if (fieldInfo != null) return fieldInfo;

            // If no field found, try to get the property
            var propInfo = targetType.GetProperty(memberName, BINDING_FLAGS);
            if (propInfo != null) return propInfo;

            // If no property found, try to get the method
            var methodInfo = targetType.GetMethod(memberName, BINDING_FLAGS);
            if (methodInfo != null) return methodInfo;

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
            container.Q<Label>().text = m_label;

            SetupUIElement(container, actualTarget, memberInfo);
            return container;
        }

        /// <summary>
        /// Derived classes implement this to configure specific UI elements and bindings.
        /// </summary>
        protected abstract void SetupUIElement(VisualElement container, object actualTarget, MemberInfo memberInfo);
    }

    [Serializable]
    public abstract class SliderSettingBase<T> : SettingBase where T : struct, IConvertible, IComparable<T> {
        public T m_minValue;
        public T m_maxValue;

        protected SliderSettingBase() {
            if (!typeof(IComparable<T>).IsAssignableFrom(typeof(T))) {
                throw new InvalidOperationException($"Type {typeof(T)} must implement IComparable<{typeof(T)}>.");
            }
        }

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
                property.PropertyChanged += (_, args) => {
                    if (args.PropertyName != propInfo.Name) return;
                    
                    if (propInfo.GetValue(actualTarget) is T newValue) {
                        slider.SetValueWithoutNotify(newValue);
                    }
                };
            }
        }
    }

    [Serializable]
    public class FloatSliderSetting : SliderSettingBase<float> {
        protected override BaseSlider<float> CreateSlider(VisualElement container) => container.Q<Slider>();
    }

    [Serializable]
    public class IntSliderSetting : SliderSettingBase<int> {
        protected override BaseSlider<int> CreateSlider(VisualElement container) => container.Q<SliderInt>();
    }

    [Serializable]
    public class EnumFieldSetting : SettingBase {
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
                property.PropertyChanged += (_, args) => {
                    if (args.PropertyName != propInfo.Name) return;
                    if (propInfo.GetValue(actualTarget) is Enum newValue) {
                        enumField.SetValueWithoutNotify(newValue);
                    }
                };
            }
        }
    }

    [Serializable]
    public class ToggleFieldSetting : SettingBase {
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
                property.PropertyChanged += (_, args) => {
                    if (args.PropertyName != propInfo.Name) return;
                    if (propInfo.GetValue(actualTarget) is bool newValue) {
                        toggle.SetValueWithoutNotify(newValue);
                    }
                };
            }
        }
    }

    [Serializable]
    public class ButtonFieldSetting : SettingBase {
        protected override void SetupUIElement(VisualElement container, object actualTarget, MemberInfo memberInfo) {
            var button = container.Q<Button>();
            if (button == null) {
                Debug.LogError($"Unsupported Button type for setting '{m_label}'.");
                return;
            }

            BindButton(actualTarget, memberInfo, button);
        }

        void BindButton(object actualTarget, MemberInfo memberInfo, Button button) {
            if (memberInfo is MethodInfo methodInfo) {
                button.clicked += () => methodInfo.Invoke(actualTarget, null);
            }
            else {
                Debug.LogError($"Member '{memberInfo.Name}' on object '{actualTarget}' is not a method.");
            }
        }
    }
}