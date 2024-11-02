using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace TCS.SimpleOptionsUI {
    public class MenuButton {
        VisualElement m_container;
        Button m_button;
        bool m_isVisible;
    }
    public class SimpleSettingsMenu : MonoBehaviour {
        [SerializeField] UIDocument m_uiDocument;

        [SerializeField] public List<SettingsGroup> m_settingsGroups = new();
        readonly TemplateFactory m_templateFactory = new();

        VisualElement m_menuRoot;
        const string MENU_ROOT = "simple-settings";
        VisualElement m_settingContainer;
        VisualElement m_optionsContainer;
        ScrollView m_scrollView;
        bool m_menuVisible;

        VisualElement m_frontButtonContainer;
        const string FRONT_BUTTON_CONTAINER = "front-button-container";

        Button m_resumeButton;
        const string RESUME_BUTTON_TEXT = "resume-button";
        Button m_optionsButton;
        const string OPTIONS_BUTTON_TEXT = "options-button";
        Button m_quitButton;
        const string QUIT_BUTTON_TEXT = "quit-button";

        void Awake() {
            m_templateFactory.InitializeAll();
        }
        void Start() {
            InitializeElements();
            PopulateSettings();
            HideEntireMenu();
        }

        void InitializeElements() {
            m_uiDocument ??= GetComponent<UIDocument>();
            foreach (var group in m_settingsGroups) {
                group.Init(m_templateFactory);
            }

            var topRoot = m_uiDocument.rootVisualElement;
            m_menuRoot = topRoot.Q<VisualElement>(MENU_ROOT);
            m_resumeButton = topRoot.Q<Button>(RESUME_BUTTON_TEXT);
            m_optionsButton = topRoot.Q<Button>(OPTIONS_BUTTON_TEXT);
            m_quitButton = topRoot.Q<Button>(QUIT_BUTTON_TEXT);

            m_frontButtonContainer = topRoot.Q<VisualElement>(FRONT_BUTTON_CONTAINER);
            m_optionsContainer = m_templateFactory.CreateOptionsContainer();
            m_settingContainer = topRoot.Q<VisualElement>("menu-container");
            m_settingContainer.Add(m_optionsContainer);
            m_scrollView = m_optionsContainer.Q<ScrollView>();
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
            foreach (var group in m_settingsGroups) {
                m_scrollView.Add(group.CreateGroupSettingElement());
            }
        }

        void HideFrontButtonContainer() {
            m_frontButtonContainer.style.display = DisplayStyle.None;
        }

        void ShowFrontButtonContainer() {
            m_frontButtonContainer.style.display = DisplayStyle.Flex;
        }

        void HideSettingContainer() {
            m_settingContainer.style.display = DisplayStyle.None;

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
        }

        void PauseGame() {
            GameApplication.PauseGame();
        }

        void ResumeGame() {
            GameApplication.ResumeGame();
        }

        void OnDestroy() {
            HideEntireMenu();

            foreach (var group in m_settingsGroups) {
                group.Dispose();
            }
        }
    }
}