using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace TCS.SimpleOptionsUI {
    [RequireComponent(typeof(UIDocument))]
    public class ExampleSettingsMenu : MonoBehaviour {
        [SerializeField] UIDocument m_uiDocument;
        [SerializeField] VisualTreeAsset m_floatSliderSetting;
        [SerializeField] VisualTreeAsset m_intSliderSetting;
        [SerializeField] VisualTreeAsset m_enumSetting;
        [SerializeField] VisualTreeAsset m_toggleSetting;
        [SerializeField] VisualTreeAsset m_buttonSetting;
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

        /// <summary>
        /// Retrieves the appropriate VisualTreeAsset template for a given setting.
        /// </summary>
        /// <param name="setting">The setting for which to retrieve the template.</param>
        /// <returns>
        /// The VisualTreeAsset template corresponding to the type of the provided setting.
        /// Returns null if the setting type is not recognized.
        /// </returns>
        VisualTreeAsset GetTemplateForSetting(SettingBase setting) {
            return setting switch {
                FloatSliderSetting => m_floatSliderSetting,
                IntSliderSetting => m_intSliderSetting,
                EnumFieldSetting => m_enumSetting,
                ToggleFieldSetting => m_toggleSetting,
                ButtonFieldSetting => m_buttonSetting,
                _ => null
            };
        }
    }
}