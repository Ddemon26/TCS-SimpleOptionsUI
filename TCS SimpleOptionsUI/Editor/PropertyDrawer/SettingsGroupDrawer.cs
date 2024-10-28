#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TCS.SimpleOptionsUI.Editor {
    [CustomPropertyDrawer(typeof(SettingsGroup))]
    public class SettingsGroupDrawer : PropertyDrawer {
        // Constants for layout configuration
        const float PADDING = 4f; // Spacing between elements
        const float BOX_PADDING = 10f; // Padding inside each box
        const float BUTTON_SIZE = 20f; // Size for the remove and move buttons
        const float HIDDEN_VARIABLE_HEIGHT = 20f; // Height reserved for the hidden variable

        // Define the list of available SettingBase types
        static readonly Dictionary<string, Type> SettingTypes = new() {
            { "Float", typeof(FloatSliderSetting) },
            { "Int", typeof(IntSliderSetting) },
            { "Enum", typeof(EnumFieldSetting) },
            { "Toggle", typeof(ToggleFieldSetting) },
            { "Button", typeof(ButtonFieldSetting) }
        };

        // Define GUIStyles for better visual appearance
        GUIStyle m_boxStyle;
        GUIStyle m_headerStyle;
        GUIStyle m_buttonCloseStyle;
        GUIStyle m_borderStyle;
        GUIStyle m_buttonArrowStyle;

        /// <summary>
        /// Initializes the GUIStyles if they are not already initialized.
        /// </summary>
        void InitializeStyles() {
            m_boxStyle ??= new GUIStyle(GUI.skin.box) {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 5, 5),
                border = new RectOffset(4, 4, 4, 4)
            };

            m_headerStyle ??= new GUIStyle(GUI.skin.label) {
                fontStyle = FontStyle.Bold,
                fontSize = 12
            };

            m_buttonCloseStyle ??= new GUIStyle(GUI.skin.button) {
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = BUTTON_SIZE,
                fixedHeight = BUTTON_SIZE,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                normal = { textColor = Color.red } // Set the text color to red
            };

            m_buttonArrowStyle ??= new GUIStyle(GUI.skin.button) {
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = BUTTON_SIZE,
                fixedHeight = BUTTON_SIZE,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                normal = { textColor = Color.green } // Set the text color to black
            };

            // New border style for each setting element
            m_borderStyle ??= new GUIStyle(GUI.skin.box) {
                border = new RectOffset(2, 2, 2, 2),
                normal = {
                    background = MakeTex(2, 2, new Color(0.5f, 0.5f, 0.5f, 0.5f))
                }
            };
        }

        /// <summary>
        /// Creates a Texture2D of a single color for use in GUIStyles.
        /// </summary>
        static Texture2D MakeTex(int width, int height, Color col) {
            Color[] pix = new Color[width * height];

            for (var i = 0; i < pix.Length; i++)
                pix[i] = col;

            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        /// <summary>
        /// Calculates the total height required for the property drawer.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            InitializeStyles(); // Ensure styles are initialized

            var totalHeight = 0f;

            // Height for Group Name
            totalHeight += EditorGUIUtility.singleLineHeight + PADDING;

            // Height for Foldout and Add button
            totalHeight += EditorGUIUtility.singleLineHeight + PADDING;

            // Height for Settings List
            var settings = property.FindPropertyRelative("m_settings");
            if (settings.isExpanded) {
                // Indent the settings list
                EditorGUI.indentLevel++;
                totalHeight += EditorGUIUtility.singleLineHeight + PADDING; // Foldout

                for (var i = 0; i < settings.arraySize; i++) {
                    var setting = settings.GetArrayElementAtIndex(i);
                    // Add height for each setting with possible foldouts and hidden variable space
                    totalHeight += GetSettingPropertyHeight(setting) + PADDING;
                }

                EditorGUI.indentLevel--;
            }
            else {
                // Just the foldout line
                totalHeight += EditorGUIUtility.singleLineHeight + PADDING;
            }

            return totalHeight;
        }

        /// <summary>
        /// Calculates the height required for each setting, including its box, contents, and hidden variable.
        /// </summary>
        float GetSettingPropertyHeight(SerializedProperty setting) {
            // Calculate the height of the property field with indentation
            float propertyHeight = EditorGUI.GetPropertyHeight(setting, true);
            // Total height includes property height, box padding, and hidden variable space
            float height = propertyHeight + BOX_PADDING * 2 + HIDDEN_VARIABLE_HEIGHT;
            return height;
        }

        /// <summary>
        /// Draws the property drawer GUI.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            InitializeStyles(); // Ensure styles are initialized
            EditorGUI.BeginProperty(position, label, property);

            int originalIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            DrawGroupNameField(position, property);

            var settings = property.FindPropertyRelative("m_settings");
            float nextYPosition = DrawFoldoutAndAddButton(position, settings, property);

            if (settings.isExpanded) {
                DrawSettingsList(position, settings, nextYPosition);
            }

            EditorGUI.indentLevel = originalIndent;
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Draws the group name field.
        /// </summary>
        void DrawGroupNameField(Rect position, SerializedProperty property) {
            var groupNameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var groupNameProp = property.FindPropertyRelative("m_groupName");
            EditorGUI.PropertyField(groupNameRect, groupNameProp, new GUIContent("Group Name"));
        }

        /// <summary>
        /// Draws the foldout and Add Setting button, returning the next y-position.
        /// </summary>
        float DrawFoldoutAndAddButton(Rect position, SerializedProperty settings, SerializedProperty property) {
            const float buttonWidth = 120f;
            float foldoutWidth = position.width - buttonWidth - PADDING;

            var foldoutRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + PADDING, foldoutWidth, EditorGUIUtility.singleLineHeight);
            var addButtonRect = new Rect(foldoutRect.xMax + PADDING, foldoutRect.y, buttonWidth, EditorGUIUtility.singleLineHeight);

            settings.isExpanded = EditorGUI.Foldout(foldoutRect, settings.isExpanded, "Settings", true);

            if (GUI.Button(addButtonRect, "Add Setting")) {
                ShowAddSettingPopup(property);
            }

            return foldoutRect.yMax + PADDING;
        }

        /// <summary>
        /// Draws the settings list, returning the next y-position.
        /// </summary>
        float DrawSettingsList(Rect position, SerializedProperty settings, float startY) {
            float currentY = startY;
            EditorGUI.indentLevel++;

            List<int> indicesToRemove = new();
            List<(int from, int to)> indicesToMove = new();

            for (var i = 0; i < settings.arraySize; i++) {
                var setting = settings.GetArrayElementAtIndex(i);
                currentY = DrawSettingItem(position, setting, i, currentY, indicesToRemove, indicesToMove, settings.arraySize);
            }

            ApplyRemovalsAndMoves(settings, indicesToRemove, indicesToMove);
            EditorGUI.indentLevel--;

            return currentY;
        }

        /// <summary>
        /// Draws a single setting item, updating the current y-position.
        /// </summary>
        float DrawSettingItem(Rect position, SerializedProperty setting, int index, float currentY, List<int> indicesToRemove, List<(int from, int to)> indicesToMove, int settingsCount) {
            float settingHeight = GetSettingPropertyHeight(setting);
            var boxRect = new Rect(position.x, currentY, position.width, settingHeight);
            GUI.Box(boxRect, GUIContent.none, m_borderStyle);

            var contentRect = new Rect(boxRect.x + 1, boxRect.y + 1, boxRect.width - 2, boxRect.height - 2);
            GUI.Box(contentRect, GUIContent.none, m_boxStyle);

            var labelRect = new Rect(position.x + BOX_PADDING + 1, currentY + BOX_PADDING + 1, position.width - 40, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, $"Setting {index + 1}", m_headerStyle);

            DrawSettingButtons(position, index, currentY, indicesToRemove, indicesToMove, settingsCount);

            var settingFieldRect = new Rect
            (
                position.x + BOX_PADDING + 1, currentY + BOX_PADDING + EditorGUIUtility.singleLineHeight + PADDING + 1,
                position.width - BOX_PADDING * 2 - BUTTON_SIZE * 3 - PADDING * 2 - 2,
                settingHeight - EditorGUIUtility.singleLineHeight - BOX_PADDING * 2 - PADDING - HIDDEN_VARIABLE_HEIGHT - 2
            );

            if (settingHeight - EditorGUIUtility.singleLineHeight - BOX_PADDING * 2 - PADDING - HIDDEN_VARIABLE_HEIGHT > 0) {
                EditorGUI.PropertyField(settingFieldRect, setting, GUIContent.none, true);
            }

            return currentY + settingHeight + PADDING;
        }

        /// <summary>
        /// Draws setting item buttons for remove move up, and move down.
        /// </summary>
        void DrawSettingButtons(Rect position, int index, float currentY, List<int> indicesToRemove, List<(int from, int to)> indicesToMove, int settingsCount) {
            float buttonsX = position.x + position.width - BOX_PADDING - BUTTON_SIZE - 1;
            float buttonsY = currentY + BOX_PADDING + 1;

            var removeButtonRect = new Rect(buttonsX, buttonsY, BUTTON_SIZE, BUTTON_SIZE);
            if (GUI.Button(removeButtonRect, "X", m_buttonCloseStyle)) {
                indicesToRemove.Add(index);
            }

            if (index > 0) {
                var moveUpButtonRect = new Rect(buttonsX, buttonsY + BUTTON_SIZE + PADDING, BUTTON_SIZE, BUTTON_SIZE);
                if (GUI.Button(moveUpButtonRect, "↑", m_buttonArrowStyle)) {
                    indicesToMove.Add((index, index - 1));
                }
            }

            if (index < settingsCount - 1) {
                var moveDownButtonRect = new Rect(buttonsX, buttonsY + (BUTTON_SIZE + PADDING) * 2, BUTTON_SIZE, BUTTON_SIZE);
                if (GUI.Button(moveDownButtonRect, "↓", m_buttonArrowStyle)) {
                    indicesToMove.Add((index, index + 1));
                }
            }
        }

        /// <summary>
        /// Applies element removals and moves after all drawing is complete.
        /// </summary>
        void ApplyRemovalsAndMoves(SerializedProperty settings, List<int> indicesToRemove, List<(int from, int to)> indicesToMove) {
            if (indicesToRemove.Count > 0) {
                foreach (int i in indicesToRemove.OrderByDescending(index => index)) {
                    RemoveElementAtIndex(settings, i);
                }

                settings.serializedObject.ApplyModifiedProperties();
            }

            if (indicesToMove.Count > 0) {
                foreach ((int from, int to) in indicesToMove) {
                    MoveElement(settings, from, to);
                }

                settings.serializedObject.ApplyModifiedProperties();
            }
        }


        /// <summary>
        /// Safely removes an element from the settings array at the specified index.
        /// Handles managed references appropriately.
        /// </summary>
        /// <param name="settings">The SerializedProperty representing the settings array.</param>
        /// <param name="index">The index of the element to remove.</param>
        static void RemoveElementAtIndex(SerializedProperty settings, int index) {
            if (settings != null && index >= 0 && index < settings.arraySize) {
                var element = settings.GetArrayElementAtIndex(index);

                // If the element is a managed reference, set it to null first
                if (element.propertyType == SerializedPropertyType.ManagedReference) {
                    element.managedReferenceValue = null;
                }

                // Delete the array element
                settings.DeleteArrayElementAtIndex(index);
            }
        }

        /// <summary>
        /// Moves an element from one index to another within the settings array.
        /// </summary>
        /// <param name="settings">The SerializedProperty representing the settings array.</param>
        /// <param name="fromIndex">The current index of the element.</param>
        /// <param name="toIndex">The index to move the element to.</param>
        static void MoveElement(SerializedProperty settings, int fromIndex, int toIndex) {
            if (settings != null &&
                fromIndex >= 0 && fromIndex < settings.arraySize &&
                toIndex >= 0 && toIndex < settings.arraySize) {
                settings.MoveArrayElement(fromIndex, toIndex);
            }
        }

        /// <summary>
        /// Displays a popup menu to select the type of setting to add.
        /// </summary>
        /// <param name="settingsGroupProperty">The SerializedProperty representing the settings group.</param>
        void ShowAddSettingPopup(SerializedProperty settingsGroupProperty) {
            // Create the popup window
            var menu = new GenericMenu();

            // Add options for each type of setting
            foreach (KeyValuePair<string, Type> kvp in SettingTypes) {
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
        void AddSetting(SerializedProperty settingsGroupProperty, Type settingType) {
            if (!typeof(SettingBase).IsAssignableFrom(settingType)) {
                Debug.LogError($"Type {settingType} does not inherit from SettingBase.");
                return;
            }

            var settings = settingsGroupProperty.FindPropertyRelative("m_settings");
            settings.arraySize++;
            var newSetting = settings.GetArrayElementAtIndex(settings.arraySize - 1);
            newSetting.managedReferenceValue = Activator.CreateInstance(settingType);

            // Apply the changes after setting all new values
            settings.serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif