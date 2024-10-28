using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace TCS.SimpleOptionsUI {
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