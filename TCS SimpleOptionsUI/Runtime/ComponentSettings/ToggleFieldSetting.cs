using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace TCS.SimpleOptionsUI
{
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
    }
}