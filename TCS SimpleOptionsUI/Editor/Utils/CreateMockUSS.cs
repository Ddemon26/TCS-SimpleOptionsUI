using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TCS.SimpleOptionsUI.Editor {
    public class CreateMockUSS : EditorWindow {
        [SerializeField] StyleSheet m_uss;

        string m_fileName;
        string m_savePath;

        const string K_DEFAULT_SAVE_PATH = "Assets";
        const string K_DEFAULT_FILE_NAME = "MockUSS";

        [MenuItem("Tools/Tent City Studio/Simple Settings/Create Mock USS")]
        public static void ShowWindow() {
            GetWindow<CreateMockUSS>("Create Mock USS");
        }

        void OnGUI() {
            m_uss = (StyleSheet)EditorGUILayout.ObjectField("USS File", m_uss, typeof(StyleSheet), false);
            m_fileName = EditorGUILayout.TextField("File Name", m_fileName);
            EditorGUILayout.BeginHorizontal();
            m_savePath = EditorGUILayout.TextField("Save Path", m_savePath);
            if (GUILayout.Button("Browse", GUILayout.Width(70))) {
                m_savePath = EditorUtility.SaveFolderPanel("Choose Save Path", "Assets", "");
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Create Mock USS")) {
                CreateMockUSSFile();
            }
        }

        void CreateMockUSSFile() {
            if (!m_uss) {
                Debug.LogError("No USS file selected.");
                return;
            }

            // Assign default values if necessary
            if (string.IsNullOrEmpty(m_savePath)) {
                m_savePath = K_DEFAULT_SAVE_PATH;
            }

            if (string.IsNullOrEmpty(m_fileName)) {
                m_fileName = K_DEFAULT_FILE_NAME;
            }

            string ussPath = AssetDatabase.GetAssetPath(m_uss);
            string ussContent = File.ReadAllText(ussPath);

            // Save the modified content to the specified savePath
            string fullPath = Path.Combine(m_savePath, m_fileName + ".uss");

            if (File.Exists(fullPath)) {
                bool overwrite = EditorUtility.DisplayDialog
                (
                    "File Exists",
                    "The file already exists at the specified path. Do you want to overwrite it?",
                    "Yes, overwrite",
                    "No, i'm scared"
                );

                if (!overwrite) {
                    Debug.Log("Operation cancelled by the user.");
                    return;
                }
            }

            File.WriteAllText(fullPath, ussContent);

            Debug.Log("Mock USS file created at: " + fullPath);
            AssetDatabase.Refresh();

            // Convert the absolute path to a relative path
            string relativePath = "Assets" + fullPath.Substring(Application.dataPath.Length);

            var asset = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
            EditorGUIUtility.PingObject(asset);
        }
    }
}