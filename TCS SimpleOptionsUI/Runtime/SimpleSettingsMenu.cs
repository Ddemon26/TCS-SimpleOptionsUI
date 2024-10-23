using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace TCS.SimpleOptionsUI {
    [RequireComponent(typeof(UIDocument))]
    public class SimpleSettingsMenu : MonoBehaviour {
        [SerializeField] UIDocument m_uiDocument;
        [SerializeField] VisualTreeAsset m_optionsTemplate;

        [SerializeField] VisualTreeAsset m_floatSliderSetting;
        [SerializeField] VisualTreeAsset m_intSliderSetting;
        [SerializeField] VisualTreeAsset m_enumSetting;
        [SerializeField] VisualTreeAsset m_toggleSetting;
        [SerializeField] VisualTreeAsset m_buttonSetting;

        [SerializeReference] List<SettingBase> m_settings = new();

        VisualElement m_settingContainer;
        VisualElement m_optionsContainer;
        ScrollView m_scrollView;
        bool m_settingsPopulated;

        Button m_resumeButton;
        const string RESUME_BUTTON_TEXT = "resume-button";
        Button m_optionsButton;
        const string OPTIONS_BUTTON_TEXT = "options-button";
        Button m_quitButton;
        const string QUIT_BUTTON_TEXT = "quit-button";

        void Start() {
            m_uiDocument ??= GetComponent<UIDocument>();
            var root = m_uiDocument.rootVisualElement;
            m_optionsContainer = m_optionsTemplate.CloneTree();
            m_settingContainer = root.Q<VisualElement>("menu-container");
            m_settingContainer.Add(m_optionsContainer);
            m_scrollView = root.Q<ScrollView>();

            //PopulateSettings();
            m_resumeButton = root.Q<Button>(RESUME_BUTTON_TEXT);
            m_optionsButton = root.Q<Button>(OPTIONS_BUTTON_TEXT);
            m_quitButton = root.Q<Button>(QUIT_BUTTON_TEXT);

            HideEntireMenu();
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                ShowMenu();
            }
        }

        void PopulateSettings() {
            if (m_settingsPopulated) return;

            foreach (var setting in m_settings) {
                var uiElement = setting.CreateUIElement(GetTemplateForSetting(setting));
                if (uiElement != null) {
                    m_scrollView.Add(uiElement);
                }
            }

            m_settingsPopulated = true;
        }

        void HideSettingContainer() {
            m_scrollView.Clear();
            m_settingContainer.style.display = DisplayStyle.None;
            m_settingsPopulated = false;
        }

        void HideEntireMenu() {
            HideSettingContainer();
            m_optionsButton.clicked -= ShowSettingContainer;
            m_resumeButton.clicked -= HideEntireMenu;
            m_quitButton.clicked -= ReturnToMainMenu;
            m_uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        }

        void ShowSettingContainer() {
            PopulateSettings();
            m_settingContainer.style.display = DisplayStyle.Flex;
        }

        void ShowMenu() {
            m_optionsButton.clicked += ShowSettingContainer;
            m_resumeButton.clicked += HideEntireMenu;
            m_quitButton.clicked += ReturnToMainMenu;
            m_uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        }
        void ReturnToMainMenu() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
        }

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

        public void RemoveAllStyleSheets() {
            List<VisualElement> allElements = m_uiDocument.rootVisualElement.Query().ToList();
            foreach (var element in allElements) {
                element.styleSheets.Clear();
            }
        }

        public void AddStyleSheet(StyleSheet styleSheet) {
            List<VisualElement> allElements = m_uiDocument.rootVisualElement.Query().ToList();
            foreach (var element in allElements) {
                element.styleSheets.Add(styleSheet);
            }
        }
    }
}