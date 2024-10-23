using System.Collections.Generic;
using System.IO;
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
                GUILayout.Label("Drag and drop a MonoScript here to generate getters and setters for fields marked with [UISetting].", EditorStyles.wordWrappedLabel);
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

            // Validate that the class inherits from MonoBehaviour or ScriptableObject
            if (!string.IsNullOrEmpty(m_baseClassName)) {
                if (!(m_baseClassName == "MonoBehaviour" || m_baseClassName == "ScriptableObject")) {
                    EditorUtility.DisplayDialog("Error", "The selected class must inherit from MonoBehaviour or ScriptableObject.", "OK");
                    ResetSelection();
                    return;
                }
            }
            else {
                EditorUtility.DisplayDialog("Error", "The selected class must inherit from MonoBehaviour or ScriptableObject.", "OK");
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

            // Check if INotifyPropertyChanged is already implemented
            var implementsINotify = false;
            var interfaceRegex = new Regex(@"public\s+class\s+" + Regex.Escape(m_className) + @"\s*:\s*([\w\s,<>]+)");
            var interfaceMatch = interfaceRegex.Match(m_scriptText);
            if (interfaceMatch.Success) {
                string interfaces = interfaceMatch.Groups[1].Value;
                implementsINotify = interfaces.Contains("INotifyPropertyChanged");
            }

            // Extract fields with [UISetting]
            List<FieldInfo> uiFields = GetUISettingFields(m_scriptText);

            if (uiFields.Count == 0) {
                EditorUtility.DisplayDialog("Info", "No fields with [UISetting] attribute found.", "OK");
                return;
            }

            var sb = new StringBuilder();

            // Collect necessary using directives
            List<string> requiredUsings = new List<string> {
                "System.Collections.Generic",
                "System.ComponentModel",
                "System.Runtime.CompilerServices"
            };

            // If ScriptableObject, we need to use UnityEditor.EditorUtility.SetDirty(this);
            bool isScriptableObject = m_baseClassName == "ScriptableObject";

            // Determine existing usings
            List<string> existingUsings = new List<string>();
            var usingRegex = new Regex(@"using\s+([\w\.]+);");
            var usingMatches = usingRegex.Matches(m_scriptText);
            foreach (Match match in usingMatches) {
                existingUsings.Add(match.Groups[1].Value);
            }

            // Prepare using directives to add
            var usingsToAdd = new StringBuilder();
            foreach (string requiredUsing in requiredUsings) {
                if (!existingUsings.Contains(requiredUsing)) {
                    usingsToAdd.AppendLine($"using {requiredUsing};");
                }
            }

            if (usingsToAdd.Length > 0) {
                // Insert missing usings at the top of the file, after existing usings
                var allUsings = usingRegex.Matches(m_scriptText);
                if (allUsings.Count > 0) {
                    int lastUsingIndex = allUsings[^1].Index + allUsings[^1].Length;
                    m_scriptText = m_scriptText.Insert(lastUsingIndex, "\n" + usingsToAdd.ToString());
                }
                else {
                    // If no existing usings, insert at the very top
                    m_scriptText = usingsToAdd.ToString() + "\n" + m_scriptText;
                }
            }

            // Re-check if INotifyPropertyChanged is implemented after possible using insertions
            implementsINotify = m_scriptText.Contains("INotifyPropertyChanged");

            // If INotifyPropertyChanged is not implemented, add it to the class definition and implement the interface
            if (!implementsINotify) {
                // Add INotifyPropertyChanged to class inheritance
                string classPattern = @"(public\s+class\s+" + Regex.Escape(m_className) + @"\s*)(?:\:\s*([\w\s,<>]+))?(\s*\{)";
                if (Regex.IsMatch(m_scriptText, classPattern)) {
                    m_scriptText = Regex.Replace
                    (
                        m_scriptText, classPattern, (match) => {
                            string existingInheritance = match.Groups[2].Value;
                            string beforeBrace = match.Groups[1].Value;

                            return string.IsNullOrEmpty(existingInheritance) ? $"{beforeBrace}: INotifyPropertyChanged{match.Groups[3].Value}" : $"{beforeBrace}: {existingInheritance.Trim()}, INotifyPropertyChanged{match.Groups[3].Value}";
                        }
                    );
                }
                else {
                    EditorUtility.DisplayDialog("Error", "Failed to modify the class definition to implement INotifyPropertyChanged.", "OK");
                    return;
                }

                // Prepare INotifyPropertyChanged implementation methods
                sb.AppendLine("    public event PropertyChangedEventHandler PropertyChanged;");
                sb.AppendLine("    void OnPropertyChanged([CallerMemberName] string propertyName = null) {");
                sb.AppendLine("        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));");
                sb.AppendLine("    }");
                sb.AppendLine("    bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {");
                sb.AppendLine("        if (EqualityComparer<T>.Default.Equals(field, value)) return false;");
                sb.AppendLine("        field = value;");
                sb.AppendLine("        OnPropertyChanged(propertyName);");
                if (isScriptableObject) {
                    sb.AppendLine();
                    sb.AppendLine("#if UNITY_EDITOR");
                    sb.AppendLine("        UnityEditor.EditorUtility.SetDirty(this); // This is needed to make the changes persistent in the editor");
                    sb.AppendLine("#endif");
                    sb.AppendLine();
                }

                sb.AppendLine("        return true;");
                sb.AppendLine("    }");
                sb.AppendLine();
            }
            else {
                // If INotifyPropertyChanged is already implemented, ensure SetField includes the UnityEditor logic if ScriptableObject
                if (isScriptableObject) {
                    // Check if SetField already includes the UnityEditor logic
                    var setFieldRegex = new Regex(@"bool\s+SetField<[^>]+>\s*\(\s*ref\s+\w+\s+\w+,\s*\w+\s+\w+,\s*\[CallerMemberName\]\s+string\s+\w+\s*=\s*null\s*\)\s*\{[\s\S]*?#if\s+UNITY_EDITOR");
                    if (!setFieldRegex.IsMatch(m_scriptText)) {
                        // Find the SetField method and insert the UNITY_EDITOR block before return
                        var existingSetFieldRegex = new Regex(@"bool\s+SetField<[^>]+>\s*\(\s*ref\s+\w+\s+\w+,\s*\w+\s+\w+,\s*\[CallerMemberName\]\s+string\s+\w+\s*=\s*null\s*\)\s*\{([\s\S]*?)\}");
                        var setFieldMatch = existingSetFieldRegex.Match(m_scriptText);
                        if (setFieldMatch.Success) {
                            string existingMethodBody = setFieldMatch.Groups[1].Value;
                            // Insert UNITY_EDITOR block before the closing brace
                            string newMethodBody = existingMethodBody.TrimEnd() + "\n\n#if UNITY_EDITOR\n        UnityEditor.EditorUtility.SetDirty(this); // This is needed to make the changes persistent in the editor\n#endif\n\n        return true;";
                            m_scriptText = m_scriptText.Replace(setFieldMatch.Value, $"bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {{\n{newMethodBody}\n}}");
                        }
                        else {
                            Debug.LogWarning("SetField method not found or does not match the expected pattern. Skipping insertion of UNITY_EDITOR code.");
                        }
                    }
                }
            }

            // Generate properties
            foreach (var field in uiFields) {
                // Check if property already exists
                string propertyName = ToPascalCase(field.Name);
                var propertyPattern = $"public\\s+{Regex.Escape(field.Type)}\\s+{Regex.Escape(propertyName)}\\s*{{";
                if (Regex.IsMatch(m_scriptText, propertyPattern)) {
                    Debug.Log($"Property '{propertyName}' already exists in '{m_className}'. Skipping.");
                    continue;
                }

                sb.AppendLine($"    public {field.Type} {propertyName} {{");
                sb.AppendLine($"        get => {field.Name};");
                sb.AppendLine($"        set => SetField(ref {field.Name}, value);");
                sb.AppendLine("    }");
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

        List<FieldInfo> GetUISettingFields(string scriptText) {
            List<FieldInfo> fields = new List<FieldInfo>();
            // Regex to find [UISetting] fields
            // This regex assumes fields are in the format: [UISetting] public type name;
            var fieldRegex = new Regex(@"\[UISetting\]\s+public\s+([\w\<\>\[\]]+)\s+(\w+);");
            var matches = fieldRegex.Matches(scriptText);
            foreach (Match match in matches) {
                if (match.Groups.Count == 3) {
                    string type = match.Groups[1].Value;
                    string value = match.Groups[2].Value;
                    fields.Add(new FieldInfo { Type = type, Name = value });
                }
            }

            return fields;
        }

        string ToPascalCase(string str) {
            if (string.IsNullOrEmpty(str))
                return str;
            if (str.Length == 1)
                return str.ToUpper();
            return char.ToUpper(str[0]) + str.Substring(1);
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
    }
}