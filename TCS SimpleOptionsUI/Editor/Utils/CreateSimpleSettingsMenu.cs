using UnityEditor;
using UnityEngine;

namespace TCS.SimpleOptionsUI.Editor {
    public static class CreateSimpleSettingsMenu {
        [MenuItem("GameObject/Tent City Studio/Create Simple Settings Menu")]
        public static void CreateSimpleSettingsMenuCommand() {
            if (!Object.FindFirstObjectByType<SimpleSettingsMenu>(FindObjectsInactive.Include)) {
                var settingsMenu = Resources.Load<GameObject>("Prefabs/SimpleSettings");
                if (settingsMenu) {
                    var instance = PrefabUtility.InstantiatePrefab(settingsMenu) as GameObject;
                    if (!instance) return;
                    
                    instance.name = "[SimpleSettings]";
                    Selection.activeGameObject = instance;
                }
                else {
                    Debug.LogError("SimpleSettings prefab not found in Resources/Prefabs folder.");
                }
            }
            else {
                Debug.Log("A SimpleSettingsMenu already exists in the scene.");
            }
        }
    }
}