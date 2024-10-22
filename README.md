# TCS SimpleOptionsUI

[![Join our Discord](https://img.shields.io/badge/Discord-Join%20Us-7289DA?logo=discord&logoColor=white)](https://discord.gg/knwtcq3N2a)
![Discord](https://img.shields.io/discord/1047781241010794506)
![GitHub Forks](https://img.shields.io/github/forks/Ddemon26/TCS-SimpleOptionsUI)
![GitHub Contributors](https://img.shields.io/github/contributors/Ddemon26/TCS-SimpleOptionsUI)
![GitHub Stars](https://img.shields.io/github/stars/Ddemon26/TCS-SimpleOptionsUI)
![GitHub Repo Size](https://img.shields.io/github/repo-size/Ddemon26/TCS-SimpleOptionsUI)

## Overview

**TCS SimpleOptionsUI** is a straightforward and efficient settings menu framework for Unity, designed to help developers quickly create intuitive and consistent user interfaces for settings in their games or applications. This system takes advantage of Unity's UI Toolkit (using `.uxml` and `.uss` files) to offer a modern and customizable settings interface. The main objective of TCS SimpleOptionsUI is to streamline the process of implementing common settings, such as toggles, sliders, buttons, and dropdowns, which makes it highly suitable for interactive projects that require user-adjustable options.

## Key Features

- **Modular UI Components**: Includes a variety of pre-built UI components like buttons, toggles, sliders, and dropdowns, allowing rapid development.
- **Unity Editor Integration**: Custom editor scripts enable seamless integration and editing of settings menus directly within Unity's editor environment.
- **Customizable Styles**: Uses `.uxml` and `.uss` files to allow easy customization of the appearance of settings menus, so they can fit well with the project's visual identity.
- **Easy to Extend**: The modular codebase allows developers to add new types of settings with minimal effort.
- **Example Implementations**: Provides sample settings menus to get developers up and running as quickly as possible.

## Getting Started

### Installation

1. **Clone the Repository**:
   ```sh
   git clone https://github.com/Ddemon26/TCS-SimpleOptionsUI.git
   ```
2. **Import into Unity**:
   - Open your Unity project.
   - Import the `TCS SimpleOptionsUI` folder into your project's `Assets` directory.

## Usage

### Creating a Settings Menu

1. **Create a New Settings Menu**
   - Navigate to the `Runtime` folder and locate the `ExampleSettingsMenu.cs` script. This can be used as a reference for creating your own settings menu.
   - Alternatively, you can use the templates provided in the `Templates` folder to create new UI components.

2. **Add Settings Components**
   - Use pre-built UI components like `SliderFloatSetting.uxml`, `ToggleSetting.uxml`, and `ButtonSetting.uxml` from the `Templates` folder.
   - Simply drag and drop these templates into your `.uxml` file to quickly create a functional settings interface.

3. **Customize Appearance**
   - Modify the `.uss` files (`MainSettingsMenu.uss`, `MenuButtonUSS.uss`) to change the visual style of the settings menu and buttons, aligning them with your project’s design.

### Example Code

Below is a basic example of how to create a settings menu using `TCS SimpleOptionsUI`. The code also includes detailed comments and examples drawn from the sample code to help you get started:

```csharp
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
```

The following code demonstrates how to utilize the settings system with more advanced control. This script shows how to use the `INotifyPropertyChanged` interface to bind `ScriptableObjects` to UI elements, allowing for more complex data structures and interactions.

```csharp
using UnityEngine;
/// <summary>
/// When using a simple monobehaviour class, we can directly access the values from the Menu UI System.
/// making mono behaviours more accessible and easier to use.
/// </summary>
public class SomeValue : MonoBehaviour {
    public float m_floatValue1;
    public float m_floatValue2;
    public float m_floatValue3;
    
    public int m_intValue1;
    public int m_intValue2;
    public int m_intValue3;
    
    public SomeEnum m_enumValue1;
}
```
```csharp
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// This side is more complex than using the monobehaviour, we need to use <see cref="INotifyPropertyChanged"/>  to bind the UI elements.
/// the system is looking for INotifyPropertyChanged interface to bind ScriptableObjects to the UI elements.
/// we also need to use the SetField method to set the values and call OnPropertyChanged to notify the UI elements.
/// unreachable values are added to show that the values are not accessible from the Menu UI System.
/// instead we need to use the INotifyPropertyChanged interface to bind the UI elements. and use get and set methods to access the values.
/// while invoking the OnPropertyChanged method to notify the UI elements.
/// at the end we are using UnityEditor.EditorUtility.SetDirty(this); to make the changes persistent in the editor.
/// keeping the data persistent is a bit tricky, but if its necessary to keep the data persistent, we can use this method.
/// <see cref="OnPropertyChanged"/>
/// </summary>
[CreateAssetMenu(menuName = "Create SomeValueSO", fileName = "SomeValueSO", order = 0)]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class SomeValueSo : ScriptableObject, INotifyPropertyChanged {
    public float m_unreachableFloatValue1;
    public float m_unreachableFloatValue2;
    public float m_unreachableFloatValue3;

    public int m_unreachableIntValue1;
    public int m_unreachableIntValue2;
    public int m_unreachableIntValue3;
    
    public SomeEnum m_unreachableEnumValue;
    public bool m_unreachableBoolValue;

    public event PropertyChangedEventHandler PropertyChanged;

    public float FloatValue1 {
        get => m_unreachableFloatValue1;
        set => SetField(ref m_unreachableFloatValue1, value);
    }
    public float FloatValue2 {
        get => m_unreachableFloatValue2;
        set => SetField(ref m_unreachableFloatValue2, value);
    }
    public float FloatValue3 {
        get => m_unreachableFloatValue3;
        set => SetField(ref m_unreachableFloatValue3, value);
    }

    public int IntValue1 {
        get => m_unreachableIntValue1;
        set => SetField(ref m_unreachableIntValue1, value);
    }
    public int IntValue2 {
        get => m_unreachableIntValue2;
        set => SetField(ref m_unreachableIntValue2, value);
    }
    public int IntValue3 {
        get => m_unreachableIntValue3;
        set => SetField(ref m_unreachableIntValue3, value);
    }
    
    public SomeEnum EnumValue {
        get => m_unreachableEnumValue;
        set => SetField(ref m_unreachableEnumValue, value);
    }
    
    public bool BoolValue {
        get => m_unreachableBoolValue;
        set => SetField(ref m_unreachableBoolValue, value);
    }

    void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this); // This is needed to make the changes persistent in the editor
#endif
        
        return true;
    }
}
```

### Sample Walkthrough

The `Samples` folder includes example scenes and implementations to help you get started quickly. Below is a summary of what you will find in the sample code:

1. **ExampleSettingsMenu.cs**: This script demonstrates how to create a settings menu and add different types of settings (e.g., toggle, slider). Each setting is thoroughly commented to explain its purpose and functionality.
2. **Sample Scene**: A Unity scene that showcases a fully functional settings menu using TCS SimpleOptionsUI, providing an example of how to integrate and visualize the settings in your own project.

By reviewing and running the sample scene, you can observe how to add settings to the menu and how the system allows users to adjust these settings at runtime.

### Adding Custom Settings

- **Extend `SettingBase.cs`**: To create a custom setting, extend the `SettingBase` class and implement your own logic. The sample scripts include comments that explain how to effectively extend this base class.
- **Create a New Template**: Use Unity's UI Toolkit to create a new `.uxml` file that defines the layout of your custom setting. You can refer to the provided templates in the `Templates` folder for guidance on structure and styling.

## Customization

The look and feel of the UI can be extensively customized using Unity's UI Toolkit. You can modify the provided `.uss` (Unity Style Sheet) files or create your own to ensure that the settings menu fits seamlessly with the overall aesthetic of your game or application.

For more advanced customization, you can extend existing UI components or create entirely new ones using the `.uxml` templates in the `Templates` folder.

## Contributing

We welcome contributions! If you have suggestions for improving the project, or if you would like to report an issue, please open an issue or submit a pull request. Make sure to follow the project's code of conduct.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Community & Support

Join our community on Discord for support, questions, and to share your projects that use **TCS SimpleOptionsUI**.

[![Join our Discord](https://img.shields.io/badge/Discord-Join%20Us-7289DA?logo=discord&logoColor=white)](https://discord.gg/knwtcq3N2a)

---

We hope **TCS SimpleOptionsUI** makes it easier for you to create excellent user experiences in your Unity projects! Feel free to reach out if you have any questions or need further assistance.

