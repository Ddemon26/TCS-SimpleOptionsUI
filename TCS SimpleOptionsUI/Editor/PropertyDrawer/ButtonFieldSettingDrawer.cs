using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
namespace TCS.SimpleOptionsUI.Editor {
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

        protected override void RenderVariableDropdown(SerializedProperty targetProp, SerializedProperty variableProp, Rect variableRect) {
            var targetObj = targetProp.objectReferenceValue;
            if (targetObj) {
                List<string> methodNames = GetMethodNames(targetObj);

                if (methodNames.Count > 0) {
                    // Determine the currently selected index
                    int selectedIndex = methodNames.IndexOf(variableProp.stringValue);
                    if (selectedIndex == -1) selectedIndex = 0;

                    // Render the popup
                    selectedIndex = EditorGUI.Popup(variableRect, "Method Name", selectedIndex, methodNames.ToArray());
                    variableProp.stringValue = methodNames[selectedIndex];
                }
                else {
                    EditorGUI.LabelField(variableRect, "No public methods found in target object");
                }
            }
            else {
                EditorGUI.LabelField(variableRect, "Select a target object first");
            }
        }

        List<string> GetMethodNames(Object targetObj) {
            List<string> methodNames = new();
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            if (targetObj is GameObject go) {
                // Get components from the GameObject
                Component[] components = go.GetComponents<Component>(); //TODO: Find a better way to handle getting components
                foreach (var comp in components) {
                    if (!comp) continue; // Handle missing scripts
                    var compType = comp.GetType();

                    // Get methods
                    IEnumerable<string> methods = compType.GetMethods(bindingFlags)
                        .Where(method => method.GetParameters().Length == 0 && method.ReturnType == typeof(void)) // Only methods with no parameters and return type void
                        .Select(method => $"{compType.Name}/{method.Name}");
                    methodNames.AddRange(methods);
                }
            }
            else {
                // Handle ScriptableObjects and Components
                var targetType = targetObj.GetType();

                // Get methods
                IEnumerable<string> methods = targetType.GetMethods(bindingFlags)
                    .Where(method => method.GetParameters().Length == 0 && method.ReturnType == typeof(void)) // Only methods with no parameters and return type void
                    .Select(method => method.Name);
                methodNames.AddRange(methods);
            }

            return methodNames;
        }
    }
}