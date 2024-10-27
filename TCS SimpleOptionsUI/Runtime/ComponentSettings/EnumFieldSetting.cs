using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace TCS.SimpleOptionsUI
{
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
    }
}