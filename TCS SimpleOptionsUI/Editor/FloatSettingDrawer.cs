using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace TCS.SimpleOptionsUI.Editor {
    [CustomPropertyDrawer(typeof(FloatSetting))]
    public class FloatSettingDrawer : PropertyDrawer {
        const int NUMBER_OF_FIELDS = 5;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = EditorGUIUtility.standardVerticalSpacing;

            var labelRect = GetNextRect(ref position, lineHeight, padding);
            var minRect = GetNextRect(ref position, lineHeight, padding);
            var maxRect = GetNextRect(ref position, lineHeight, padding);
            var targetRect = GetNextRect(ref position, lineHeight, padding);
            var variableRect = GetNextRect(ref position, lineHeight, padding);

            var labelProp = property.FindPropertyRelative("m_label");
            var minProp = property.FindPropertyRelative("m_minValue");
            var maxProp = property.FindPropertyRelative("m_maxValue");
            var targetProp = property.FindPropertyRelative("m_targetObject");
            var variableProp = property.FindPropertyRelative("m_variableName");

            EditorGUI.PropertyField(labelRect, labelProp);
            EditorGUI.PropertyField(minRect, minProp, new GUIContent("Min Value"));
            EditorGUI.PropertyField(maxRect, maxProp, new GUIContent("Max Value"));
            EditorGUI.PropertyField(targetRect, targetProp);

            var targetObj = targetProp.objectReferenceValue;
            if (targetObj) {
                List<string> floatFieldNames = GetFloatFieldNames(targetObj);

                if (floatFieldNames.Count > 0) {
                    int selectedIndex = floatFieldNames.IndexOf(variableProp.stringValue);
                    if (selectedIndex == -1) selectedIndex = 0;
                    selectedIndex = EditorGUI.Popup(variableRect, "Variable Name", selectedIndex, floatFieldNames.ToArray());
                    variableProp.stringValue = floatFieldNames[selectedIndex];
                }
                else {
                    EditorGUI.LabelField(variableRect, "No float variables found in target object");
                }
            }
            else {
                EditorGUI.LabelField(variableRect, "Select a target object first");
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        List<string> GetFloatFieldNames(Object targetObj) {
            List<string> floatFieldNames = new();
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            if (targetObj is GameObject go) {
                // Get components from the GameObject
                Component[] components = go.GetComponents<Component>();
                foreach (var comp in components) {
                    if (!comp) continue; // Handle missing scripts
                    var compType = comp.GetType();

                    // Get fields
                    var fields = compType.GetFields(bindingFlags)
                        .Where(field => field.FieldType == typeof(float))
                        .Select(field => compType.Name + "/" + field.Name);
                    floatFieldNames.AddRange(fields);

                    // Get properties
                    var properties = compType.GetProperties(bindingFlags)
                        .Where(prop => prop.PropertyType == typeof(float))
                        .Select(prop => compType.Name + "/" + prop.Name);
                    floatFieldNames.AddRange(properties);
                }
            }
            else {
                // Handle ScriptableObjects and Components
                var targetType = targetObj.GetType();

                // Get fields
                var fields = targetType.GetFields(bindingFlags)
                    .Where(field => field.FieldType == typeof(float))
                    .Select(field => field.Name);
                floatFieldNames.AddRange(fields);

                // Get properties
                var properties = targetType.GetProperties(bindingFlags)
                    .Where(prop => prop.PropertyType == typeof(float))
                    .Select(prop => prop.Name);
                floatFieldNames.AddRange(properties);
            }

            return floatFieldNames;
        }

        Rect GetNextRect(ref Rect position, float lineHeight, float padding) {
            var rect = new Rect(position.x, position.y, position.width, lineHeight);
            position.y += lineHeight + padding;
            return rect;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = EditorGUIUtility.standardVerticalSpacing;
            return NUMBER_OF_FIELDS * (lineHeight + padding);
        }
    }
}