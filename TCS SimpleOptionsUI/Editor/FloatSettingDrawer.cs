using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TCS.SimpleOptionsUI.Editor {
    public abstract class BaseSettingDrawer : PropertyDrawer {
        // Number of fields varies; derived classes can override if needed
        protected virtual int NumberOfFields => ShowMinMaxFields ? 5 : 3;

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
            var labelProp = property.FindPropertyRelative("m_label");
            var minProp = property.FindPropertyRelative("m_minValue");
            var maxProp = property.FindPropertyRelative("m_maxValue");
            var targetProp = property.FindPropertyRelative("m_targetObject");
            var variableProp = property.FindPropertyRelative("m_variableName");

            // Render fields
            EditorGUI.PropertyField(labelRect, labelProp);

            // Render Min and Max only if applicable
            if (ShowMinMaxFields) {
                EditorGUI.PropertyField(minRect, minProp, new GUIContent("Min Value"));
                EditorGUI.PropertyField(maxRect, maxProp, new GUIContent("Max Value"));
            }

            EditorGUI.PropertyField(targetRect, targetProp);

            // Handle Variable Name Dropdown
            RenderVariableDropdown(targetProp, variableProp, variableRect);

            // Restore original indent level
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        Rect GetNextRect(ref Rect position, float lineHeight, float padding) {
            var rect = new Rect(position.x, position.y, position.width, lineHeight);
            position.y += lineHeight + padding;
            return rect;
        }

        protected virtual bool ShowMinMaxFields => true;
        protected abstract Type TargetType { get; }
        protected abstract string TypeName { get; }

        List<string> GetMemberNames(Object targetObj) {
            List<string> memberNames = new();
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            if (targetObj is GameObject go) {
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
            }
            else if (targetObj is ScriptableObject) {
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
            }
            else {
                Debug.LogWarning("Target object is not a GameObject or ScriptableObject");
            }

            return memberNames;
        }

        protected virtual void RenderVariableDropdown(SerializedProperty targetProp, SerializedProperty variableProp, Rect variableRect) {
            var targetObj = targetProp.objectReferenceValue;
            if (targetObj) {
                List<string> memberNames = GetMemberNames(targetObj);

                if (memberNames.Count > 0) {
                    // Determine the currently selected index
                    int selectedIndex = memberNames.IndexOf(variableProp.stringValue);
                    if (selectedIndex == -1) selectedIndex = 0;

                    // Render the popup
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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = EditorGUIUtility.standardVerticalSpacing;
            return NumberOfFields * (lineHeight + padding);
        }
    }

    [CustomPropertyDrawer(typeof(FloatSliderSetting))]
    public class FloatSettingDrawer : BaseSettingDrawer {
        protected override bool ShowMinMaxFields => true;
        protected override Type TargetType => typeof(float);
        protected override string TypeName => "float";
    }

    [CustomPropertyDrawer(typeof(IntSliderSetting))]
    public class IntSettingDrawer : BaseSettingDrawer {
        protected override bool ShowMinMaxFields => true;
        protected override Type TargetType => typeof(int);
        protected override string TypeName => "int";
    }

    [CustomPropertyDrawer(typeof(EnumFieldSetting))]
    public class EnumFieldDrawer : BaseSettingDrawer {
        protected override bool ShowMinMaxFields => false;
        protected override Type TargetType => typeof(Enum);
        protected override string TypeName => "enum";
    }

    [CustomPropertyDrawer(typeof(ToggleFieldSetting))]
    public class ToggleFieldDrawer : BaseSettingDrawer {
        protected override bool ShowMinMaxFields => false;
        protected override Type TargetType => typeof(bool);
        protected override string TypeName => "bool";
    }

    [CustomPropertyDrawer(typeof(ButtonFieldSetting))]
    public class ButtonFieldSettingDrawer : BaseSettingDrawer {
        protected override bool ShowMinMaxFields => false;
        protected override Type TargetType => typeof(void); // No specific type for methods
        protected override string TypeName => "method";

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
                Component[] components = go.GetComponents<Component>();
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