using UnityEngine;
using UnityEngine.UIElements;
namespace TCS.SimpleOptionsUI {
    public class TemplateFactory : ISimpleSettingFactory {
        VisualTreeAsset m_optionsContainer;
        VisualTreeAsset m_groupSettingsContainer;
        
        VisualTreeAsset m_floatSliderSetting;
        VisualTreeAsset m_intSliderSetting;
        VisualTreeAsset m_enumSetting;
        VisualTreeAsset m_toggleSetting;
        VisualTreeAsset m_buttonSetting;

        public TemplateFactory() { }
        
        public void InitializeAll() {
            InitializeTemplates();
            InitializeContainers();
        }

        public void InitializeTemplates() {
            if (!m_floatSliderSetting) {
                m_floatSliderSetting = Resources.Load<VisualTreeAsset>("Templates/FloatSliderSetting");
            }
            if (!m_intSliderSetting) {
                m_intSliderSetting = Resources.Load<VisualTreeAsset>("Templates/IntSliderSetting");
            }
            if (!m_enumSetting) {
                m_enumSetting = Resources.Load<VisualTreeAsset>("Templates/EnumSetting");
            }
            if (!m_toggleSetting) {
                m_toggleSetting = Resources.Load<VisualTreeAsset>("Templates/ToggleSetting");
            }
            if (!m_buttonSetting) {
                m_buttonSetting = Resources.Load<VisualTreeAsset>("Templates/ButtonSetting");
            }
        }
        
        public void InitializeContainers() {
            if (!m_optionsContainer) {
                m_optionsContainer = Resources.Load<VisualTreeAsset>("Templates/OptionsContainer");
            }
            if (!m_groupSettingsContainer) {
                m_groupSettingsContainer = Resources.Load<VisualTreeAsset>("Templates/GroupSettingsContainer");
            }
        }
        
        public VisualElement CreateOptionsContainer() => m_optionsContainer.CloneTree();
        public VisualElement CreateGroupSettingsContainer() => m_groupSettingsContainer.CloneTree();

        public VisualTreeAsset GetTemplateForSetting(SettingBase setting) {
            return setting switch {
                FloatSliderSetting => m_floatSliderSetting,
                IntSliderSetting => m_intSliderSetting,
                EnumFieldSetting => m_enumSetting,
                ToggleFieldSetting => m_toggleSetting,
                ButtonFieldSetting => m_buttonSetting,
                _ => null,
            };
        }

        public SettingBase GetTemplateForSetting(TemplateType type) {
            return type switch {
                TemplateType.FloatSliderSetting => new FloatSliderSetting(),
                TemplateType.IntSliderSetting => new IntSliderSetting(),
                TemplateType.EnumSetting => new EnumFieldSetting(),
                TemplateType.ToggleSetting => new ToggleFieldSetting(),
                TemplateType.ButtonSetting => new ButtonFieldSetting(),
                _ => null,
            };
        }
    }
}