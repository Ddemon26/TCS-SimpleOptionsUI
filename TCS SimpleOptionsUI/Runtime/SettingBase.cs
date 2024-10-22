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
            if (m_targetObject is not GameObject go) return m_targetObject;
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

            // For ScriptableObjects or Components, return the object itself
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
        
        public abstract VisualElement CreateUIElement(VisualTreeAsset template);
    }

    [Serializable]
    public abstract class SliderSettingBase<T> : SettingBase where T : struct, IConvertible, IComparable<T> {
        public T m_minValue;
        public T m_maxValue;

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

            BaseSlider<T> slider = CreateSlider(container);
            if (slider == null) {
                Debug.LogError($"Unsupported slider type for setting '{m_label}'.");
                return null;
            }

            slider.lowValue = m_minValue;
            slider.highValue = m_maxValue;

            BindSlider(actualTarget, memberInfo, slider);

            return container;
        }

        protected abstract BaseSlider<T> CreateSlider(VisualElement container);

        void BindSlider(object actualTarget, MemberInfo memberInfo, BaseSlider<T> slider) {
            switch (memberInfo) {
                case FieldInfo fieldInfo when fieldInfo.FieldType == typeof(T):
                {
                    var currentValue = (T)fieldInfo.GetValue(actualTarget);
                    slider.value = currentValue;

                    slider.RegisterValueChangedCallback
                    (
                        evt => {
                            fieldInfo.SetValue(actualTarget, evt.newValue);
                        }
                    );
                    break;
                }
                case PropertyInfo propInfo when propInfo.PropertyType == typeof(T):
                {
                    var currentValue = (T)propInfo.GetValue(actualTarget);
                    slider.value = currentValue;

                    slider.RegisterValueChangedCallback
                    (
                        evt => {
                            propInfo.SetValue(actualTarget, evt.newValue);
                        }
                    );

                    if (actualTarget is INotifyPropertyChanged property) {
                        property.PropertyChanged += (_, args) => {
                            if (args.PropertyName == propInfo.Name) {
                                slider.SetValueWithoutNotify((T)propInfo.GetValue(actualTarget));
                            }
                        };
                    }

                    break;
                }
                default:
                    Debug.LogError($"Member '{memberInfo.Name}' on object '{actualTarget}' is not of type {typeof(T)}.");
                    break;
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
}