using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace TCS.SimpleOptionsUI.Editor {
    public class UISettingGenerator : EditorWindow {
        MonoScript m_targetScript;
        string m_className = "";
        string m_namespaceName = "";
        string m_scriptPath = "";
        string m_scriptText = "";
        bool m_scriptParsed = false;
        string m_baseClassName = "";

        [MenuItem("Tools/Tent City Studio/UI Setting Generator")]
        public static void ShowWindow() {
            GetWindow<UISettingGenerator>("UI Setting Generator");
        }

        void OnGUI() {
            GUILayout.Label("Generate Getters and Setters for [UISetting] Fields", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Drag-and-Drop ObjectField for MonoScript
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Target Script:", GUILayout.Width(100));
            m_targetScript = (MonoScript)EditorGUILayout.ObjectField(m_targetScript, typeof(MonoScript), false);
            EditorGUILayout.EndHorizontal();

            if (m_targetScript) {
                // Extract class information when a script is selected
                if (!m_scriptParsed) {
                    ExtractClassInfo();
                }

                if (m_scriptParsed) {
                    GUILayout.Space(10);
                    GUILayout.Label($"Class: {m_className}");
                    if (!string.IsNullOrEmpty(m_namespaceName)) {
                        GUILayout.Label($"Namespace: {m_namespaceName}");
                    }

                    GUILayout.Label($"Base Class: {m_baseClassName}");

                    GUILayout.Space(10);

                    if (GUILayout.Button("Generate Getters and Setters")) {
                        GenerateCode();
                    }
                }
            }
            else {
                GUILayout.Space(10);
                GUILayout.Label("Drag and drop a Class that inherits from UISettingBehaviour or UISettingScriptableObject here to generate getters and setters for fields marked with [UISetting].", EditorStyles.wordWrappedLabel);
            }
        }

        void ExtractClassInfo() {
            if (!m_targetScript)
                return;

            // Get the path of the script asset
            m_scriptPath = AssetDatabase.GetAssetPath(m_targetScript);
            if (string.IsNullOrEmpty(m_scriptPath)) {
                EditorUtility.DisplayDialog("Error", "Failed to get the script path.", "OK");
                return;
            }

            // Read the script text
            m_scriptText = File.ReadAllText(m_scriptPath);

            // Extract namespace and class name
            var namespaceRegex = new Regex(@"namespace\s+([\w\.]+)");
            var classRegex = new Regex(@"public\s+class\s+(\w+)(?:\s*:\s*([\w\s,<>]+))?");

            var namespaceMatch = namespaceRegex.Match(m_scriptText);
            var classMatch = classRegex.Match(m_scriptText);

            m_namespaceName = namespaceMatch.Success ? namespaceMatch.Groups[1].Value : "";

            if (classMatch.Success) {
                m_className = classMatch.Groups[1].Value;
                string inheritance = classMatch.Groups[2].Value.Trim();

                if (!string.IsNullOrEmpty(inheritance)) {
                    // Determine the base class (first item before any comma)
                    string[] inheritanceParts = inheritance.Split(',');
                    m_baseClassName = inheritanceParts[0].Trim();
                }
                else {
                    m_baseClassName = "";
                }
            }
            else {
                EditorUtility.DisplayDialog("Error", "Failed to extract the class name from the script.", "OK");
                ResetSelection();
                return;
            }

            // Validate that the class inherits from UISettingBehaviour or UISettingScriptableObject
            if (!string.IsNullOrEmpty(m_baseClassName)) {
                if (!(m_baseClassName == "UISettingBehaviour" || m_baseClassName == "UISettingScriptableObject")) {
                    EditorUtility.DisplayDialog("Error", "The selected class must inherit from UISettingBehaviour or UISettingScriptableObject.", "OK");
                    ResetSelection();
                    return;
                }
            }
            else {
                EditorUtility.DisplayDialog("Error", "The selected class must inherit from UISettingBehaviour or UISettingScriptableObject.", "OK");
                ResetSelection();
                return;
            }

            m_scriptParsed = true;
        }

        void GenerateCode() {
            if (string.IsNullOrEmpty(m_scriptPath) || string.IsNullOrEmpty(m_scriptText)) {
                EditorUtility.DisplayDialog("Error", "No valid script selected.", "OK");
                return;
            }

            // Extract fields with [UISetting]
            List<FieldInfo> uiFields = GetUISettingFields(m_scriptText);

            if (uiFields.Count == 0) {
                EditorUtility.DisplayDialog("Info", "No fields with [UISetting] attribute found.", "OK");
                return;
            }

            var sb = new StringBuilder();

            // Collect necessary using directives
            List<string> requiredUsings = new() {
                "TCS.SimpleOptionsUI"
            };

            // Determine existing usings
            List<string> existingUsings = new();
            var usingRegex = new Regex(@"using\s+([\w\.]+);");
            var usingMatches = usingRegex.Matches(m_scriptText);
            foreach (Match match in usingMatches) {
                existingUsings.Add(match.Groups[1].Value);
            }

            // Prepare using directives to add
            var usingsToAdd = new StringBuilder();
            foreach
            (
                string requiredUsing in requiredUsings
                    .Where
                    (
                        requiredUsing => !existingUsings
                            .Contains(requiredUsing)
                    )) {
                usingsToAdd.AppendLine($"using {requiredUsing};");
            }

            if (usingsToAdd.Length > 0) {
                // Insert missing usings at the top of the file, after existing usings
                if (usingMatches.Count > 0) {
                    int lastUsingIndex = usingMatches[^1].Index + usingMatches[^1].Length;
                    m_scriptText = m_scriptText.Insert(lastUsingIndex, "\n" + usingsToAdd);
                }
                else {
                    // If no existing usings, insert at the very top
                    m_scriptText = usingsToAdd + "\n" + m_scriptText;
                }
            }

            // Determine indentation
            string indentation = DetectIndentation();

            // Insert a blank line before the first property for readability
            sb.AppendLine();

            // Generate properties with appropriate indentation
            foreach (var field in uiFields) {
                // Check if property already exists
                string propertyName = GetPropertyName(field.Name);
                var propertyPattern = $@"public\s+{Regex.Escape(field.Type)}\s+{Regex.Escape(propertyName)}\s*{{";
                if (Regex.IsMatch(m_scriptText, propertyPattern)) {
                    Debug.Log($"Property '{propertyName}' already exists in '{m_className}'. Skipping.");
                    continue;
                }

                sb.AppendLine($"{indentation}public {field.Type} {propertyName} {{");
                sb.AppendLine($"{indentation}    get => {field.Name};");
                sb.AppendLine($"{indentation}    set => SetField(ref {field.Name}, value);");
                sb.AppendLine($"{indentation}}}");
                sb.AppendLine();
            }

            if (sb.Length == 0) {
                EditorUtility.DisplayDialog("Info", "All properties already exist. No changes were made.", "OK");
                return;
            }

            // Insert the generated code into the class
            string newScript;
            if (!string.IsNullOrEmpty(m_namespaceName)) {
                // Pattern to find the closing brace of the class within the namespace
                var classEndRegex = new Regex(@"(public\s+class\s+" + Regex.Escape(m_className) + @"\s*:[^{]+?\{)([\s\S]*?)(\n\s*\})");
                var classEndMatch = classEndRegex.Match(m_scriptText);
                if (classEndMatch.Success) {
                    int insertionPoint = classEndMatch.Groups[3].Index;
                    newScript = m_scriptText.Insert(insertionPoint, sb.ToString());
                }
                else {
                    EditorUtility.DisplayDialog("Error", "Failed to locate the class closing bracket.", "OK");
                    return;
                }
            }
            else {
                // Pattern to find the closing brace of the class without a namespace
                var classEndRegex = new Regex(@"(public\s+class\s+" + Regex.Escape(m_className) + @"\s*:[^{]+?\{)([\s\S]*?)(\n\s*\})");
                var classEndMatch = classEndRegex.Match(m_scriptText);
                if (classEndMatch.Success) {
                    int insertionPoint = classEndMatch.Groups[3].Index;
                    newScript = m_scriptText.Insert(insertionPoint, sb.ToString());
                }
                else {
                    EditorUtility.DisplayDialog("Error", "Failed to locate the class closing bracket.", "OK");
                    return;
                }
            }

            // Write back to the script file
            File.WriteAllText(m_scriptPath, newScript);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", "Getters and Setters generated successfully.", "OK");

            // Reset fields after successful generation
            ResetSelection();
        }

        static List<FieldInfo> GetUISettingFields(string scriptText) {
            List<FieldInfo> fields = new();
            // Regex to find [UISetting] fields
            // This regex assumes fields are in the format: [UISetting] public type name;
            var fieldRegex = new Regex(@"\[UISetting\]\s+public\s+([\w\<\>\[\]]+)\s+(\w+);");
            var matches = fieldRegex.Matches(scriptText);
            foreach (Match match in matches) {
                if (match.Groups.Count != 3) continue;
                string type = match.Groups[1].Value;
                string value = match.Groups[2].Value;
                fields.Add(new FieldInfo { Type = type, Name = value });
            }

            return fields;
        }

        /// <summary>
        /// Converts a field name to a proper property name by removing prefixes and underscores,
        /// then converting to PascalCase.
        /// </summary>
        /// <param name="fieldName">Original field name (e.g., "_someInt", "m_someBool").</param>
        /// <returns>Property name in PascalCase (e.g., "SomeInt", "SomeBool").</returns>
        static string GetPropertyName(string fieldName) {
            // Remove prefix and underscore if present
            int underscoreIndex = fieldName.IndexOf('_');
            string cleanedName = underscoreIndex >= 0 && underscoreIndex < fieldName.Length - 1
                ? fieldName[(underscoreIndex + 1)..]
                : fieldName;

            return ToPascalCase(cleanedName);
        }

        /// <summary>
        /// Converts a string to PascalCase.
        /// </summary>
        /// <param name="str">Input string.</param>
        /// <returns>PascalCase version of the string.</returns>
        static string ToPascalCase(string str) {
            if (string.IsNullOrEmpty(str))
                return str;
            if (str.Length == 1)
                return str.ToUpper();
            return char.ToUpper(str[0]) + str[1..];
        }

        void ResetSelection() {
            m_className = "";
            m_namespaceName = "";
            m_scriptPath = "";
            m_scriptText = "";
            m_baseClassName = "";
            m_targetScript = null;
            m_scriptParsed = false;
        }

        class FieldInfo {
            public string Type;
            public string Name;
        }

        /// <summary>
        /// Detects the indentation level based on the class's position in the script.
        /// Adds an extra indentation if the class is within a namespace.
        /// </summary>
        /// <returns>Indentation string (e.g., "    " for four spaces or "\t" for a tab).</returns>
        string DetectIndentation() {
            // Find the line where the class is declared
            var classDeclarationRegex = new Regex(@"public\s+class\s+" + Regex.Escape(m_className) + @"\s*:[^{]+?\{");
            var match = classDeclarationRegex.Match(m_scriptText);
            if (!match.Success) return "\t";
            // Get the line-up to the class declaration
            int lineStart = m_scriptText.LastIndexOf('\n', match.Index) + 1;
            int classLineLength = match.Length;
            string classLine = m_scriptText.Substring(lineStart, classLineLength);

            // Extract leading whitespace
            var leadingWhitespaceMatch = Regex.Match(classLine, @"^\s+");
            string classIndent = leadingWhitespaceMatch.Success ? leadingWhitespaceMatch.Value : "";

            // If within a namespace, add additional indentation
            if (string.IsNullOrEmpty(m_namespaceName)) return classIndent;
            // Determine the indentation style (tabs or spaces)
            var indentationUnit = "\t"; // Default to tab

            // Check if spaces are used in the class declaration
            var spaceMatch = Regex.Match(classLine, @"^(\s+)");
            if (!spaceMatch.Success) return classIndent + indentationUnit;
            string whitespace = spaceMatch.Groups[1].Value;
            // If spaces are used, determine the number
            if (whitespace.Contains("    ")) {
                // 4 spaces
                indentationUnit = "    ";
            }
            else if (whitespace.Contains("  ")) {
                // 2 spaces
                indentationUnit = "  ";
            }
            // Add more conditions if different indentation sizes are used

            return classIndent + indentationUnit;

            // Default indentation if class declaration not found
        }
    }
}