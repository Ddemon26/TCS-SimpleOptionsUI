using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
[CustomPropertyDrawer(typeof(FloatSetting))]
public class FloatSettingDrawer : PropertyDrawer {
    const int NUMBER_OF_FIELDS = 5; // Total number of fields to be drawn

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // Begin property drawing
        EditorGUI.BeginProperty(position, label, property);

        // Set indent level
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate heights and positions
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float padding = EditorGUIUtility.standardVerticalSpacing;

        // Define rects for each property field
        var labelRect = GetNextRect(ref position, lineHeight, padding);
        var minRect = GetNextRect(ref position, lineHeight, padding);
        var maxRect = GetNextRect(ref position, lineHeight, padding);
        var targetRect = GetNextRect(ref position, lineHeight, padding);
        var variableRect = GetNextRect(ref position, lineHeight, padding);

        // Get properties
        var labelProp = property.FindPropertyRelative("m_label");
        var minProp = property.FindPropertyRelative("m_minValue");
        var maxProp = property.FindPropertyRelative("m_maxValue");
        var targetProp = property.FindPropertyRelative("m_targetObject");
        var variableProp = property.FindPropertyRelative("m_variableName");

        // Draw the fields
        EditorGUI.PropertyField(labelRect, labelProp);
        EditorGUI.PropertyField(minRect, minProp, new GUIContent("Min Value"));
        EditorGUI.PropertyField(maxRect, maxProp, new GUIContent("Max Value"));
        EditorGUI.PropertyField(targetRect, targetProp);

        // If targetObject is set, get its float fields
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

        // Restore indent level
        EditorGUI.indentLevel = indent;

        // End property drawing
        EditorGUI.EndProperty();
    }

    List<string> GetFloatFieldNames(Object targetObj) {
        List<string> floatFieldNames = new();
        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        switch (targetObj) {
            case ScriptableObject:
            {
                // Use SerializedObject to get properties
                var serializedObject = new SerializedObject(targetObj);
                var property = serializedObject.GetIterator();

                if (!property.NextVisible(true)) return floatFieldNames;
                do {
                    if (property.propertyType == SerializedPropertyType.Float) {
                        floatFieldNames.Add(property.name);
                    }
                } while (property.NextVisible(false));

                break;
            }
            case GameObject go:
            {
                // Get components from the GameObject
                Component[] components = go.GetComponents<Component>();
                foreach (var comp in components) {
                    if (!comp) continue; // Handle missing scripts
                    var compType = comp.GetType();
                    FieldInfo[] fields = compType.GetFields(bindingFlags);
                    floatFieldNames.AddRange(from field in fields where field.FieldType == typeof(float) select compType.Name + "/" + field.Name);
                }

                break;
            }
            case Component comp:
            {
                // Handle MonoBehaviour components
                var compType = comp.GetType();
                FieldInfo[] fields = compType.GetFields(bindingFlags);
                floatFieldNames.AddRange(from field in fields where field.FieldType == typeof(float) select compType.Name + "/" + field.Name);
                break;
            }
            default:
            {
                // Handle other Objects
                var targetType = targetObj.GetType();
                FieldInfo[] fields = targetType.GetFields(bindingFlags);
                floatFieldNames.AddRange(from field in fields where field.FieldType == typeof(float) select field.Name);
                break;
            }
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