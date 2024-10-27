using UnityEngine;
using UnityEngine.UIElements;

namespace TCS.SimpleOptionsUI.Samples {
    public class InfoBox : MonoBehaviour {
        UIDocument m_uiDocument;
        VisualElement m_root;
        bool m_isVisible;

        void Awake() {
            m_uiDocument = GetComponent<UIDocument>();
            m_root = m_uiDocument.rootVisualElement;
            m_isVisible = true;
            Show();
        }

        void Update() {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            if (m_isVisible) {
                Hide();
            }
            else {
                Show();
            }
        }

        void Show() {
            if (m_isVisible) return;
            m_isVisible = true;
            m_root.style.display = DisplayStyle.Flex;
        }

        void Hide() {
            if (!m_isVisible) return;
            m_isVisible = false;
            m_root.style.display = DisplayStyle.None;
        }
    }
}