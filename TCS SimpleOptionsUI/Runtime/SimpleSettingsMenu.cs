using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace TCS.SimpleOptionsUI {
    public class SimpleSettingsMenu : MonoBehaviour {
        [SerializeField] UIDocument m_uiDocument;
        [SerializeField] VisualTreeAsset m_optionsTemplate;

        [SerializeField] VisualTreeAsset m_floatSliderSetting;
        [SerializeField] VisualTreeAsset m_intSliderSetting;
        [SerializeField] VisualTreeAsset m_enumSetting;
        [SerializeField] VisualTreeAsset m_toggleSetting;
        [SerializeField] VisualTreeAsset m_buttonSetting;

        [SerializeReference] List<SettingBase> m_settings = new();

        VisualElement m_menuRoot;
        const string MENU_ROOT = "simple-settings";
        VisualElement m_settingContainer;
        VisualElement m_optionsContainer;
        ScrollView m_scrollView;
        bool m_settingsPopulated;
        bool m_menuVisible;

        VisualElement m_frontButtonContainer;
        const string FRONT_BUTTON_CONTAINER = "front-button-container";

        Button m_resumeButton;
        const string RESUME_BUTTON_TEXT = "resume-button";
        Button m_optionsButton;
        const string OPTIONS_BUTTON_TEXT = "options-button";
        Button m_quitButton;
        const string QUIT_BUTTON_TEXT = "quit-button";

        void Start() {
            Init();
            HideEntireMenu();
        }

        void Init() {
            m_uiDocument ??= GetComponent<UIDocument>();
            var topRoot = m_uiDocument.rootVisualElement;
            m_menuRoot = topRoot.Q<VisualElement>(MENU_ROOT);
            m_frontButtonContainer = topRoot.Q<VisualElement>(FRONT_BUTTON_CONTAINER);
            m_optionsContainer = m_optionsTemplate.CloneTree();
            m_settingContainer = topRoot.Q<VisualElement>("menu-container");
            m_settingContainer.Add(m_optionsContainer);
            m_scrollView = topRoot.Q<ScrollView>();
            m_resumeButton = topRoot.Q<Button>(RESUME_BUTTON_TEXT);
            m_optionsButton = topRoot.Q<Button>(OPTIONS_BUTTON_TEXT);
            m_quitButton = topRoot.Q<Button>(QUIT_BUTTON_TEXT);
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Escape) && !m_menuVisible) {
                ShowMenu();
                //PauseGame();
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && m_menuVisible) {
                HideEntireMenu();
                //ResumeGame();
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

        void HideFrontButtonContainer() {
            m_frontButtonContainer.style.display = DisplayStyle.None;
        }

        void ShowFrontButtonContainer() {
            m_frontButtonContainer.style.display = DisplayStyle.Flex;
        }

        void HideSettingContainer() {
            m_scrollView.Clear();
            m_settingContainer.style.display = DisplayStyle.None;
            m_settingsPopulated = false;

            ShowFrontButtonContainer();
        }

        void HideEntireMenu() {
            HideSettingContainer();
            m_optionsButton.clicked -= ShowSettingContainer;
            m_resumeButton.clicked -= HideEntireMenu;
            m_quitButton.clicked -= ReturnToMainMenu;
            m_menuRoot.style.display = DisplayStyle.None;

            m_menuVisible = false;
        }

        void ShowSettingContainer() {
            PopulateSettings();
            HideFrontButtonContainer();
            m_settingContainer.style.display = DisplayStyle.Flex;
        }

        void ShowMenu() {
            m_optionsButton.clicked += ShowSettingContainer;
            m_resumeButton.clicked += HideEntireMenu;
            m_quitButton.clicked += ReturnToMainMenu;
            m_menuRoot.style.display = DisplayStyle.Flex;

            m_menuVisible = true;
        }
        void ReturnToMainMenu() {
            GameApplication.QuitGameCompletely();
            //GameApplication.LoadSceneByIndex(0);
        }
        
        void PauseGame() {
            GameApplication.PauseGame();
        }
        
        void ResumeGame() {
            GameApplication.ResumeGame();
        }

        void OnDestroy() {
            foreach (var setting in m_settings) {
                setting.Dispose();
            }
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
    }
}