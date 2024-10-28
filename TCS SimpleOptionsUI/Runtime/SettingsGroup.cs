using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace TCS.SimpleOptionsUI {
    [Serializable]
    public class SettingsGroup : IDisposable {
        public string m_groupName;
        public bool m_showLabel = true;
        [SerializeReference] public List<SettingBase> m_settings;

        TemplateFactory m_templateFactory;
        VisualElement m_settingsGroupTemplate;
        
        public void Init(TemplateFactory templateFactory) {
            m_templateFactory = templateFactory;
            m_settingsGroupTemplate = m_templateFactory.CreateGroupSettingsContainer();
        }
        
        public VisualElement CreateGroupSettingElement() {
            m_settingsGroupTemplate.Q<Label>().text = m_groupName;
            m_settingsGroupTemplate.Q<Label>().style.display = m_showLabel ? DisplayStyle.Flex : DisplayStyle.None;
            foreach (var setting in m_settings) {
                var uiElement = setting.CreateUIElement(GetTemplateForSetting(setting));
                if (uiElement != null) {
                    m_settingsGroupTemplate.Add(uiElement);
                }
            }
            return m_settingsGroupTemplate;
        }
        
        public void AddFloatSliderSetting() => AddSettingByType(TemplateType.FloatSliderSetting);
        public void AddIntSliderSetting() => AddSettingByType(TemplateType.IntSliderSetting);
        public void AddEnumSetting() => AddSettingByType(TemplateType.EnumSetting);
        public void AddToggleSetting() => AddSettingByType(TemplateType.ToggleSetting);
        public void AddButtonSetting() => AddSettingByType(TemplateType.ButtonSetting);
        
        public VisualTreeAsset GetTemplateForSetting(SettingBase setting) => m_templateFactory.GetTemplateForSetting(setting);
        public void AddSettingByType(TemplateType type) => m_settings.Add(m_templateFactory.GetTemplateForSetting(type));
        public void Dispose() {
            foreach (var setting in m_settings) {
                setting.Dispose();
            }
        }
    }
}