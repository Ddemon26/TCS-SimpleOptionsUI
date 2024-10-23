using UnityEngine;
using UnityEngine.UIElements;
namespace TCS.SimpleOptionsUI {
    public interface ITemplateFactory {
        VisualTreeAsset GetTemplateForSetting(SettingBase setting);
    }
    public class TemplateFactory : ITemplateFactory {
        VisualTreeAsset m_floatSliderSetting;
        VisualTreeAsset m_intSliderSetting;
        VisualTreeAsset m_enumSetting;
        VisualTreeAsset m_toggleSetting;
        VisualTreeAsset m_buttonSetting;
        
        public TemplateFactory() => Init();

        void Init() {
            m_floatSliderSetting = Resources.Load<VisualTreeAsset>("Templates/FloatSliderSetting");
            m_intSliderSetting = Resources.Load<VisualTreeAsset>("Templates/IntSliderSetting");
            m_enumSetting = Resources.Load<VisualTreeAsset>("Templates/EnumSetting");
            m_toggleSetting = Resources.Load<VisualTreeAsset>("Templates/ToggleSetting");
            m_buttonSetting = Resources.Load<VisualTreeAsset>("Templates/ButtonSetting");
        }

        public VisualTreeAsset GetTemplateForSetting(SettingBase setting) {
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