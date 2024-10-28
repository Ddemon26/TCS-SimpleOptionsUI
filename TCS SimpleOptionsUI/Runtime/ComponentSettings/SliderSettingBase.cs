using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace TCS.SimpleOptionsUI {
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
}