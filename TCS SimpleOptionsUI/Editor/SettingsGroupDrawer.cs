#if !ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TCS.SimpleOptionsUI.Editor {
    [CustomPropertyDrawer(typeof(SettingsGroup))]
    public class SettingsGroupDrawer : PropertyDrawer {
        // Constants for layout configuration
        private const float Padding = 4f; // Spacing between elements
        private const float BoxPadding = 10f; // Padding inside each box
        private const float RemoveButtonSize = 20f; // Size for the remove button
        private const float HiddenVariableHeight = 20f; // Height reserved for the hidden variable

        // Define the list of available SettingBase types
        private static readonly Dictionary<string, Type> SettingTypes = new Dictionary<string, Type> {
            { "Float", typeof(FloatSliderSetting) },
            { "Int", typeof(IntSliderSetting) },
            { "Enum", typeof(EnumFieldSetting) },
            { "Toggle", typeof(ToggleFieldSetting) },
            { "Button", typeof(ButtonFieldSetting) }
        };

        // Define GUIStyles for better visual appearance
        private GUIStyle boxStyle;
        private GUIStyle headerStyle;
        private GUIStyle removeButtonStyle;

        /// <summary>
        /// Initializes the GUIStyles if they are not already initialized.
        /// </summary>
        private void InitializeStyles() {
            if (boxStyle == null) {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.padding = new RectOffset(10, 10, 10, 10);
                boxStyle.margin = new RectOffset(0, 0, 5, 5);
                boxStyle.border = new RectOffset(4, 4, 4, 4);
            }

            if (headerStyle == null) {
                headerStyle = new GUIStyle(GUI.skin.label);
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.fontSize = 12;
            }

            if (removeButtonStyle == null) {
                removeButtonStyle = new GUIStyle(GUI.skin.button);
                removeButtonStyle.normal.textColor = Color.red;
                removeButtonStyle.alignment = TextAnchor.MiddleCenter;
                removeButtonStyle.fixedWidth = RemoveButtonSize;
                removeButtonStyle.fixedHeight = RemoveButtonSize;
                removeButtonStyle.margin = new RectOffset(0, 0, 0, 0);
                removeButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Calculates the total height required for the property drawer.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            InitializeStyles(); // Ensure styles are initialized

            float totalHeight = 0f;

            // Height for Group Name
            totalHeight += EditorGUIUtility.singleLineHeight + Padding;

            // Height for Foldout and Add button
            totalHeight += EditorGUIUtility.singleLineHeight + Padding;

            // Height for Settings List
            SerializedProperty settings = property.FindPropertyRelative("m_settings");
            if (settings.isExpanded) {
                // Indent the settings list
                EditorGUI.indentLevel++;
                totalHeight += EditorGUIUtility.singleLineHeight + Padding; // Foldout

                for (int i = 0; i < settings.arraySize; i++) {
                    SerializedProperty setting = settings.GetArrayElementAtIndex(i);
                    // Add height for each setting with possible foldouts and hidden variable space
                    totalHeight += GetSettingPropertyHeight(setting) + Padding;
                }

                EditorGUI.indentLevel--;
            }
            else {
                // Just the foldout line
                totalHeight += EditorGUIUtility.singleLineHeight + Padding;
            }

            return totalHeight;
        }

        /// <summary>
        /// Calculates the height required for each setting, including its box, contents, and hidden variable.
        /// </summary>
        private float GetSettingPropertyHeight(SerializedProperty setting) {
            // Calculate the height of the property field with indentation
            float propertyHeight = EditorGUI.GetPropertyHeight(setting, true);
            // Total height includes property height, box padding, and hidden variable space
            float height = propertyHeight + BoxPadding * 2 + HiddenVariableHeight;
            return height;
        }

        /// <summary>
        /// Draws the property drawer GUI.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            InitializeStyles(); // Ensure styles are initialized

            // Begin property
            EditorGUI.BeginProperty(position, label, property);

            // Save the current indent level and set to 0
            int originalIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Group name field
            Rect groupNameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            SerializedProperty groupNameProp = property.FindPropertyRelative("m_groupName");
            EditorGUI.PropertyField(groupNameRect, groupNameProp, new GUIContent("Group Name"));

            // Settings list
            SerializedProperty settings = property.FindPropertyRelative("m_settings");

            // Foldout and Add button on the same row
            float buttonWidth = 120f; // Increased width for better visibility
            float foldoutWidth = position.width - buttonWidth - Padding;

            // Create a rect for the foldout
            Rect foldoutRect = new Rect(position.x, groupNameRect.yMax + Padding, foldoutWidth, EditorGUIUtility.singleLineHeight);

            // Create a rect for the "Add Setting" button beside the foldout
            Rect addButtonRect = new Rect(foldoutRect.xMax + Padding, groupNameRect.yMax + Padding, buttonWidth, EditorGUIUtility.singleLineHeight);

            // Foldout for settings list
            settings.isExpanded = EditorGUI.Foldout(foldoutRect, settings.isExpanded, "Settings", true);

            // Add button beside the foldout
            if (GUI.Button(addButtonRect, "Add Setting")) {
                // Show the popup window to choose a setting type
                ShowAddSettingPopup(property);
            }

            // List to hold indices of elements to remove after iteration
            List<int> indicesToRemove = new List<int>();

            if (settings.isExpanded) {
                float currentY = foldoutRect.yMax + Padding;

                EditorGUI.indentLevel++;

                for (int i = 0; i < settings.arraySize; i++) {
                    SerializedProperty setting = settings.GetArrayElementAtIndex(i);

                    // Calculate height for the current setting
                    float settingHeight = GetSettingPropertyHeight(setting);

                    // Define the rect for the setting's box
                    Rect boxRect = new Rect(position.x, currentY, position.width, settingHeight);
                    GUI.Box(boxRect, GUIContent.none, boxStyle);

                    // Define the rect for the setting's label
                    Rect labelRect = new Rect(position.x + BoxPadding, currentY + BoxPadding, position.width - 40, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(labelRect, $"Setting {i + 1}", headerStyle);

                    // Define the rect for the remove button
                    Rect removeButtonRect = new Rect(position.x + position.width - BoxPadding - RemoveButtonSize, currentY + BoxPadding, RemoveButtonSize, RemoveButtonSize);
                    if (GUI.Button(removeButtonRect, "X", removeButtonStyle)) {
                        // Collect the index to remove later
                        indicesToRemove.Add(i);
                    }

                    // Define the rect for the setting's property field
                    // Reserve extra space for the hidden variable by increasing the height
                    Rect settingFieldRect = new Rect(position.x + BoxPadding, currentY + BoxPadding + EditorGUIUtility.singleLineHeight + Padding,
                                                     position.width - BoxPadding * 2 - RemoveButtonSize, settingHeight - EditorGUIUtility.singleLineHeight - BoxPadding * 2 - Padding - HiddenVariableHeight);

                    // Ensure the settingFieldRect has a positive height
                    if (settingHeight - EditorGUIUtility.singleLineHeight - BoxPadding * 2 - Padding - HiddenVariableHeight > 0) {
                        EditorGUI.PropertyField(settingFieldRect, setting, GUIContent.none, true);
                    }

                    // Optional: Reserve space for the hidden variable (not drawn)
                    // This space can be used for future elements or left empty
                    // If needed, you can manipulate the hidden variable here programmatically

                    currentY += settingHeight + Padding;
                }

                // Remove elements after iteration to prevent index issues
                if (indicesToRemove.Count > 0) {
                    // Sort indices in descending order to avoid shifting issues
                    foreach (int i in indicesToRemove.OrderByDescending(index => index)) {
                        RemoveElementAtIndex(settings, i);
                    }

                    // Apply changes after all deletions
                    settings.serializedObject.ApplyModifiedProperties();
                }

                EditorGUI.indentLevel--;
            }

            // Restore indent level
            EditorGUI.indentLevel = originalIndent;

            // End property
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Safely removes an element from the settings array at the specified index.
        /// Handles managed references appropriately.
        /// </summary>
        /// <param name="settings">The SerializedProperty representing the settings array.</param>
        /// <param name="index">The index of the element to remove.</param>
        private void RemoveElementAtIndex(SerializedProperty settings, int index) {
            if (settings != null && index >= 0 && index < settings.arraySize) {
                SerializedProperty element = settings.GetArrayElementAtIndex(index);

                // If the element is a managed reference, set it to null first
                if (element.propertyType == SerializedPropertyType.ManagedReference) {
                    element.managedReferenceValue = null;
                }

                // Delete the array element
                settings.DeleteArrayElementAtIndex(index);
            }
        }

        /// <summary>
        /// Displays a popup menu to select the type of setting to add.
        /// </summary>
        /// <param name="settingsGroupProperty">The SerializedProperty representing the settings group.</param>
        private void ShowAddSettingPopup(SerializedProperty settingsGroupProperty) {
            // Create the popup window
            GenericMenu menu = new GenericMenu();

            // Add options for each type of setting
            foreach (var kvp in SettingTypes) {
                menu.AddItem(new GUIContent(kvp.Key), false, () => AddSetting(settingsGroupProperty, kvp.Value));
            }

            // Display the popup
            menu.ShowAsContext();
        }

        /// <summary>
        /// Adds a new setting of the specified type to the settings group.
        /// </summary>
        /// <param name="settingsGroupProperty">The SerializedProperty representing the settings group.</param>
        /// <param name="settingType">The Type of setting to add.</param>
        private void AddSetting(SerializedProperty settingsGroupProperty, Type settingType) {
            if (!typeof(SettingBase).IsAssignableFrom(settingType)) {
                Debug.LogError($"Type {settingType} does not inherit from SettingBase.");
                return;
            }

            SerializedProperty settings = settingsGroupProperty.FindPropertyRelative("m_settings");
            settings.arraySize++;
            SerializedProperty newSetting = settings.GetArrayElementAtIndex(settings.arraySize - 1);
            newSetting.managedReferenceValue = Activator.CreateInstance(settingType);

            // Apply the changes after setting all new values
            settings.serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
