# TCS SimpleOptionsUI

![Status - Pre-Release](https://img.shields.io/badge/Status-Pre--Release-FFFF00)

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
    ```csharp
   https://github.com/Ddemon26/TCS-SimpleOptionsUI.git
   ```
    or
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

## Example Code
<details>
<summary>Click to expand To Show Examples of Binding Variables</summary>
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
</details>

# Settings Base Explanation

<details>
<summary>Click to expand the detailed explanation of the Settings Base implementation</summary>

## Using Statements

```csharp
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using Component = UnityEngine.Component;
using Object = UnityEngine.Object;
```

These statements import core .NET functionalities, reflection, Unity components, and UI elements. Aliases are used to avoid conflicts between Unity's `Component` and `Object` classes and the generic .NET counterparts.

## Namespace and Base Class Definition

```csharp
namespace TCS.SimpleOptionsUI {
    [Serializable] public abstract class SettingBase : IDisposable {
        public string m_label;
        public Object m_targetObject; // Reference to ScriptableObject, GameObject, or Component
        public string m_variableName;

        protected StyleSheet StyleSheet;
        protected void SetStyleSheet(StyleSheet styleSheet) => StyleSheet = styleSheet;
        protected private PropertyChangedEventHandler PropertyChangedHandler;

        protected const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Reflection Cache: Thread-safe
        static readonly ConcurrentDictionary<string, MemberInfo> ReflectionCache = new();

        // Cached MemberInfo instance
        MemberInfo m_cachedMemberInfo;
```

The `SettingBase` class is an abstract base class that serves as the foundation for representing a general setting in the UI. It implements `IDisposable` to facilitate resource management and proper cleanup.

## Public Fields

```csharp
        public string m_label;
        public Object m_targetObject; // Reference to ScriptableObject, GameObject, or Component
        public string m_variableName;
```

- `m_label`: Represents the UI label.
- `m_targetObject`: Refers to the target object, which can be a ScriptableObject, GameObject, or Component.
- `m_variableName`: Holds the name of the variable that needs to be bound.

## Validation Method

```csharp
        protected bool ValidateTargetAndVariableName(out string errorMessage) {
            errorMessage = string.Empty;
            if (!m_targetObject) {
                errorMessage = $"Target Object is not set for setting: {m_label}";
                return false;
            }

            if (string.IsNullOrEmpty(m_variableName)) {
                errorMessage = $"Variable Name is not set for setting: {m_label}";
                return false;
            }

            return true;
        }
```

This method checks if `m_targetObject` and `m_variableName` are correctly set. It returns an error message if either of these values is invalid, ensuring that both the target object and variable name are provided before further operations are attempted.

## Retrieving Target Object

```csharp
        protected virtual object GetActualTargetObject() {
            if (m_targetObject is not GameObject go) return m_targetObject;
            // If the target is a GameObject, extract the component
            string[] splitName = m_variableName.Split('/');
            if (splitName.Length != 2) {
                Debug.LogError($"Variable name '{m_variableName}' is not in the format 'ComponentName/FieldName'.");
                return null;
            }

            string componentName = splitName[0];
            var component = go.GetComponents<Component>().FirstOrDefault(comp => comp.GetType().Name == componentName);
            if (component) return component;

            Debug.LogError($"Component '{componentName}' not found on GameObject '{go.name}'.");
            return null;
        }
```

This method retrieves the actual target object. If `m_targetObject` is a GameObject, it attempts to extract the appropriate component using the `ComponentName/FieldName` format. If the component cannot be found, an error is logged.

## MemberInfo Retrieval with Caching

```csharp
        protected MemberInfo GetMemberInfo() {
            if (m_cachedMemberInfo != null) return m_cachedMemberInfo;

            object actualTarget = GetActualTargetObject();
            if (actualTarget == null) return null;

            string memberName = m_variableName;

            if (m_targetObject is GameObject) {
                // Extract the field/property/method name from 'ComponentName/MemberName'
                string[] splitName = m_variableName.Split('/');
                if (splitName.Length != 2) {
                    Debug.LogError($"Variable name '{m_variableName}' is not in the format 'ComponentName/MemberName'.");
                    return null;
                }

                memberName = splitName[1];
            }

            var targetType = actualTarget.GetType();

            // Generate a unique cache key based on target type and member name
            var cacheKey = $"{targetType.FullName}.{memberName}";

            // Attempt to retrieve from cache
            if (ReflectionCache.TryGetValue(cacheKey, out var cachedInfo)) {
                m_cachedMemberInfo = cachedInfo;
                return m_cachedMemberInfo;
            }

            // Attempt to retrieve the member using reflection
            var memberInfo = targetType.GetField(memberName, BINDING_FLAGS) ??
                             (MemberInfo)targetType.GetProperty(memberName, BINDING_FLAGS) ??
                             targetType.GetMethod(memberName, BINDING_FLAGS);

            if (memberInfo != null) {
                ReflectionCache.TryAdd(cacheKey, memberInfo);
                m_cachedMemberInfo = memberInfo;
                return m_cachedMemberInfo;
            }

            Debug.LogError($"Member '{memberName}' not found on type '{targetType.Name}'.");
            return null;
        }
```

This method uses reflection to obtain the `MemberInfo` of the target object based on `m_variableName`. It leverages a cache to improve performance and reduce repeated reflection lookups. The method attempts to retrieve fields, properties, or methods, depending on what's available.

## UI Element Creation

```csharp
        public VisualElement CreateUIElement(VisualTreeAsset template) {
            if (!ValidateTargetAndVariableName(out string errorMessage)) {
                Debug.LogError(errorMessage);
                return null;
            }

            object actualTarget = GetActualTargetObject();
            if (actualTarget == null) return null;

            var memberInfo = GetMemberInfo();
            if (memberInfo == null) return null;

            VisualElement container = template.CloneTree();
            var label = container.Q<Label>();
            if (label != null) {
                label.text = m_label;
            } else {
                Debug.LogWarning($"No Label found in the template for setting '{m_label}'.");
            }

            SetupUIElement(container, actualTarget, memberInfo);
            return container;
        }
```

This method generates a UI element from the provided template and configures it using the relevant member information. It validates the target and member information before setting up the UI, providing robust error handling.

## Abstract Method for UI Setup

```csharp
        protected abstract void SetupUIElement(VisualElement container, object actualTarget, MemberInfo memberInfo);
```

This abstract method must be implemented by derived classes to provide specific UI setup logic for different settings, enabling custom behavior for each setting type.

## SliderSettingBase<T> Class

```csharp
    [Serializable] public abstract class SliderSettingBase<T> : SettingBase where T : struct, IConvertible, IComparable<T> {
        public T m_minValue;
        public T m_maxValue;

        protected override void SetupUIElement(VisualElement container, object actualTarget, MemberInfo memberInfo) {
            BaseSlider<T> slider = CreateSlider(container);
            if (slider == null) {
                Debug.LogError($"Unsupported slider type for setting '{m_label}'.");
                return;
            }

            slider.lowValue = m_minValue;
            slider.highValue = m_maxValue;

            BindSlider(actualTarget, memberInfo, slider);
        }

        protected abstract BaseSlider<T> CreateSlider(VisualElement container);
```

The `SliderSettingBase<T>` class is a generic base class for slider UI elements, which can be specialized for specific types like `int` or `float`. It provides methods for setting up and binding sliders to member variables.

## Slider Binding Methods

```csharp
        void BindSlider(object actualTarget, MemberInfo memberInfo, BaseSlider<T> slider) {
            switch (memberInfo) {
                case FieldInfo fieldInfo when fieldInfo.FieldType == typeof(T):
                    BindField(actualTarget, fieldInfo, slider);
                    break;
                case PropertyInfo propInfo when propInfo.PropertyType == typeof(T):
                    BindProperty(actualTarget, propInfo, slider);
                    break;
                default:
                    Debug.LogError($"Member '{memberInfo.Name}' on object '{actualTarget}' is not of type {typeof(T)}.");
                    break;
            }
        }

        void BindField(object actualTarget, FieldInfo fieldInfo, BaseSlider<T> slider) {
            if (fieldInfo.GetValue(actualTarget) is T currentValue) {
                slider.value = currentValue;
            }

            slider.RegisterValueChangedCallback(evt => {
                fieldInfo.SetValue(actualTarget, evt.newValue);
            });
        }
```

These methods bind a slider to either a field or a property of the target object, ensuring that changes in the UI are reflected in the underlying data and vice versa.

## Concrete Slider Classes

```csharp
    [Serializable] public class FloatSliderSetting : SliderSettingBase<float> {
        protected override BaseSlider<float> CreateSlider(VisualElement container) => container.Q<Slider>();
    }

    [Serializable] public class IntSliderSetting : SliderSettingBase<int> {
        protected override BaseSlider<int> CreateSlider(VisualElement container) => container.Q<SliderInt>();
    }
```

`FloatSliderSetting` and `IntSliderSetting` are concrete implementations of `SliderSettingBase` for `float` and `int` types, respectively. They specify which slider control should be used.

## EnumFieldSetting Class

```csharp
    [Serializable] public class EnumFieldSetting : SettingBase {
        protected override void SetupUIElement(VisualElement container, object actualTarget, MemberInfo memberInfo) {
            var enumField = container.Q<EnumField>();
            if (enumField == null) {
                Debug.LogError($"Unsupported EnumField type for setting '{m_label}'.");
                return;
            }

            BindEnumField(actualTarget, memberInfo, enumField);
        }
```

The `EnumFieldSetting` class is a specific implementation of `SettingBase` for Enum types. It binds an `EnumField` UI element to a field or property, allowing the user to select from an enumeration of values.

## ToggleFieldSetting Class

```csharp
    [Serializable] public class ToggleFieldSetting : SettingBase {
        protected override void SetupUIElement(VisualElement container, object actualTarget, MemberInfo memberInfo) {
            var toggle = container.Q<Toggle>();
            if (toggle == null) {
                Debug.LogError($"Unsupported Toggle type for setting '{m_label}'.");
                return;
            }

            BindToggle(actualTarget, memberInfo, toggle);
        }
```

The `ToggleFieldSetting` class is a specific implementation of `SettingBase` for boolean fields or properties. It uses Unity's `Toggle` component to represent a boolean state.

## ButtonFieldSetting Class

```csharp
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
```

The `ButtonFieldSetting` class represents a button in the UI that binds to a method on the target object. When the button is clicked, the bound method is executed.

## Disposal Methods

```csharp
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
```

The `Dispose` method is used to clean up resources, specifically event handlers, to prevent memory leaks. This is particularly important for UI elements that respond to user interaction.

</details>

# Base Setting Drawer Documentation

<details>
<summary>Click to expand the detailed explanation of the Base Setting Drawer implementation</summary>

## Using Statements

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
```

These are the necessary imports for core .NET functionalities, Unity editor tools, Unity components, and reflection. The alias for `Object` is used to distinguish Unity's `Object` from the .NET `Object` class.

## Namespace and Class Definition

```csharp
namespace TCS.SimpleOptionsUI.Editor {
    public abstract class BaseSettingDrawer : PropertyDrawer {
        protected const string LABEL_PROPERTY_PATH = "m_label";
        protected const string TARGET_PROPERTY_PATH = "m_targetObject";
        protected const string VARIABLE_PROPERTY_PATH = "m_variableName";
        const string MIN_VALUE_PROPERTY_PATH = "m_minValue";
        const string MAX_VALUE_PROPERTY_PATH = "m_maxValue";
        const string MIN_VALUE_PROPERTY_FIELD = "Min Value";
        const string MAX_VALUE_PROPERTY_FIELD = "Max Value";
        // Number of fields varies; derived classes can override if needed
        protected virtual int NumberOfFields => ShowMinMaxFields ? 5 : 3;
```

The `BaseSettingDrawer` class extends `PropertyDrawer` to create custom property drawers in the Unity Editor. This class serves as the foundation for different types of property drawers that share a consistent UI layout.

- `LABEL_PROPERTY_PATH`, `TARGET_PROPERTY_PATH`, and `VARIABLE_PROPERTY_PATH` are constants representing the paths to serialized properties.
- `MIN_VALUE_PROPERTY_PATH` and `MAX_VALUE_PROPERTY_PATH` define paths for min and max values, typically used in slider fields.
- `NumberOfFields` determines how many fields are displayed in the UI. It can be overridden by derived classes to customize the layout.

## OnGUI Method

```csharp
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            // Preserve original indent level
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Define layout parameters
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = EditorGUIUtility.standardVerticalSpacing;

            // Calculate rects for each field
            var labelRect = GetNextRect(ref position, lineHeight, padding);
            var minRect = ShowMinMaxFields ? GetNextRect(ref position, lineHeight, padding) : default;
            var maxRect = ShowMinMaxFields ? GetNextRect(ref position, lineHeight, padding) : default;
            var targetRect = GetNextRect(ref position, lineHeight, padding);
            var variableRect = GetNextRect(ref position, lineHeight, padding);

            // Fetch serialized properties
            var labelProp = property.FindPropertyRelative(LABEL_PROPERTY_PATH);
            var targetProp = property.FindPropertyRelative(TARGET_PROPERTY_PATH);
            var variableProp = property.FindPropertyRelative(VARIABLE_PROPERTY_PATH);

            // Render fields
            EditorGUI.PropertyField(labelRect, labelProp);

            // Render Min and Max fields if applicable
            if (ShowMinMaxFields) {
                var minProp = property.FindPropertyRelative(MIN_VALUE_PROPERTY_PATH);
                var maxProp = property.FindPropertyRelative(MAX_VALUE_PROPERTY_PATH);
                EditorGUI.PropertyField(minRect, minProp, new GUIContent(MIN_VALUE_PROPERTY_FIELD));
                EditorGUI.PropertyField(maxRect, maxProp, new GUIContent(MAX_VALUE_PROPERTY_FIELD));
            }

            EditorGUI.PropertyField(targetRect, targetProp);

            // Render the Variable Name Dropdown
            RenderVariableDropdown(targetProp, variableProp, variableRect);

            // Restore original indent level
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
```

The `OnGUI` method is responsible for rendering the custom UI elements for the property drawer. It is called whenever Unity needs to display these properties in the inspector.

- `EditorGUI.BeginProperty` and `EditorGUI.EndProperty` are used to manage property drawing, ensuring proper handling of changes.
- Layout positions for individual UI elements (`labelRect`, `minRect`, `maxRect`, `targetRect`, `variableRect`) are calculated to ensure they are correctly placed and spaced.
- `RenderVariableDropdown` manages rendering the dropdown list for selecting the appropriate variable.

## Helper Methods

### GetNextRect Method

```csharp
        protected Rect GetNextRect(ref Rect position, float lineHeight, float padding) {
            var rect = new Rect(position.x, position.y, position.width, lineHeight);
            position.y += lineHeight + padding;
            return rect;
        }
```

This method calculates the next position for drawing a UI element. It updates `position` for the following element, ensuring consistent spacing between the different UI elements.

### RenderVariableDropdown Method

```csharp
        protected virtual void RenderVariableDropdown(SerializedProperty targetProp, SerializedProperty variableProp, Rect variableRect) {
            var targetObj = targetProp.objectReferenceValue;
            if (targetObj) {
                List<string> memberNames = GetMemberNames(targetObj);

                if (memberNames.Count > 0) {
                    // Determine the currently selected index
                    int selectedIndex = memberNames.IndexOf(variableProp.stringValue);
                    if (selectedIndex == -1) selectedIndex = 0;

                    // Render the dropdown
                    selectedIndex = EditorGUI.Popup(variableRect, "Variable Name", selectedIndex, memberNames.ToArray());
                    variableProp.stringValue = memberNames[selectedIndex];
                }
                else {
                    EditorGUI.LabelField(variableRect, $"No {TypeName} members found in target object");
                }
            }
            else {
                EditorGUI.LabelField(variableRect, "Select a target object first");
            }
        }
```

This method handles rendering the variable dropdown. It displays all available members (fields or properties) of the target object, allowing the user to select the appropriate one.

- If `targetObj` is set, it retrieves a list of member names and displays them in a dropdown menu.
- If no members are available, an appropriate message is displayed to inform the user.

### GetMemberNames Method

```csharp
        List<string> GetMemberNames(Object targetObj) {
            List<string> memberNames = new();
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            switch (targetObj) {
                case GameObject go:
                {
                    // Get components from the GameObject
                    Component[] components = go.GetComponents<Component>();
                    foreach (var comp in components) {
                        if (!comp) continue; // Handle missing scripts
                        var compType = comp.GetType();

                        // Get fields
                        IEnumerable<string> fields = compType.GetFields(bindingFlags)
                            .Where(field => field.FieldType == TargetType || (TargetType == typeof(Enum) && field.FieldType.IsEnum))
                            .Select(field => $"{compType.Name}/{field.Name}");
                        memberNames.AddRange(fields);

                        // Get properties
                        IEnumerable<string> properties = compType.GetProperties(bindingFlags)
                            .Where(prop => prop.PropertyType == TargetType || (TargetType == typeof(Enum) && prop.PropertyType.IsEnum))
                            .Select(prop => $"{compType.Name}/{prop.Name}");
                        memberNames.AddRange(properties);
                    }
                    break;
                }
                case ScriptableObject:
                {
                    var targetType = targetObj.GetType();

                    // Get public fields
                    IEnumerable<string> fields = targetType.GetFields(bindingFlags)
                        .Where(field => field.IsPublic)
                        .Where(field => field.FieldType == TargetType || (TargetType == typeof(Enum) && field.FieldType.IsEnum))
                        .Select(field => field.Name);
                    memberNames.AddRange(fields);

                    // Get public properties
                    IEnumerable<string> properties = targetType.GetProperties(bindingFlags)
                        .Where(prop => prop.PropertyType == TargetType || (TargetType == typeof(Enum) && prop.PropertyType.IsEnum))
                        .Select(prop => prop.Name);
                    memberNames.AddRange(properties);
                    break;
                }
                default:
                    Debug.LogWarning("Target object is not a GameObject or ScriptableObject");
                    break;
            }

            return memberNames;
        }
```

This method retrieves a list of member names (fields or properties) that match the expected `TargetType`. It uses reflection to gather this information for both `GameObject` and `ScriptableObject` instances.

- For `GameObject`, it iterates through all components and collects relevant fields and properties.
- For `ScriptableObject`, it collects public fields and properties that match the target type.

## Derived Property Drawers

### FloatSettingDrawer Class

```csharp
    [CustomPropertyDrawer(typeof(FloatSliderSetting))]
    public class FloatSettingDrawer : BaseSettingDrawer {
        protected override bool ShowMinMaxFields => true;
        protected override Type TargetType => typeof(float);
        protected override string TypeName => "float";
    }
```

The `FloatSettingDrawer` class is a custom property drawer for `FloatSliderSetting`. It specifies that min and max fields should be displayed and sets the target type to `float`.

### IntSettingDrawer Class

```csharp
    [CustomPropertyDrawer(typeof(IntSliderSetting))]
    public class IntSettingDrawer : BaseSettingDrawer {
        protected override bool ShowMinMaxFields => true;
        protected override Type TargetType => typeof(int);
        protected override string TypeName => "int";
    }
```

The `IntSettingDrawer` class is similar to `FloatSettingDrawer`, but it is specific to integer values. It also displays min and max fields.

### EnumFieldDrawer Class

```csharp
    [CustomPropertyDrawer(typeof(EnumFieldSetting))]
    public class EnumFieldDrawer : BaseSettingDrawer {
        protected override bool ShowMinMaxFields => false;
        protected override Type TargetType => typeof(Enum);
        protected override string TypeName => "enum";
    }
```

The `EnumFieldDrawer` class is a property drawer for `EnumFieldSetting`. Since enumerations do not need min and max fields, those fields are not displayed.

### ToggleFieldDrawer Class

```csharp
    [CustomPropertyDrawer(typeof(ToggleFieldSetting))]
    public class ToggleFieldDrawer : BaseSettingDrawer {
        protected override bool ShowMinMaxFields => false;
        protected override Type TargetType => typeof(bool);
        protected override string TypeName => "bool";
    }
```

The `ToggleFieldDrawer` class is a property drawer for `ToggleFieldSetting`. It targets boolean fields and does not display min and max values.

### ButtonFieldSettingDrawer Class

```csharp
    [CustomPropertyDrawer(typeof(ButtonFieldSetting))]
    public class ButtonFieldSettingDrawer : BaseSettingDrawer {
        protected override bool ShowMinMaxFields => false;
        protected override Type TargetType => typeof(void); // No specific type for methods
        protected override string TypeName => "method";
        protected override int NumberOfFields => 4;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            // Preserve original indent level
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Define layout parameters
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = EditorGUIUtility.standardVerticalSpacing;

            // Calculate rects for each field
            var labelRect = GetNextRect(ref position, lineHeight, padding);
            var buttonTextRect = GetNextRect(ref position, lineHeight, padding);
            var targetRect = GetNextRect(ref position, lineHeight, padding);
            var variableRect = GetNextRect(ref position, lineHeight, padding);

            // Fetch serialized properties
            var labelProp = property.FindPropertyRelative(LABEL_PROPERTY_PATH);
            var targetProp = property.FindPropertyRelative(TARGET_PROPERTY_PATH);
            var variableProp = property.FindPropertyRelative(VARIABLE_PROPERTY_PATH);
            var buttonTextProp = property.FindPropertyRelative("m_buttonText");

            // Render fields
            EditorGUI.PropertyField(labelRect, labelProp);
            EditorGUI.PropertyField(buttonTextRect, buttonTextProp, new GUIContent("Button Text"));
            EditorGUI.PropertyField(targetRect, targetProp);
            RenderVariableDropdown(targetProp, variableProp, variableRect);

            // Restore original indent level
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
```

The `ButtonFieldSettingDrawer` class is a custom property drawer for `ButtonFieldSetting`. It defines a unique UI layout for button fields, including a text field for specifying the button label and a dropdown to select the associated method.

</details>

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

