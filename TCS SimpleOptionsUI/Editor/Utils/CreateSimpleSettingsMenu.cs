using UnityEditor;
using UnityEngine;

namespace TCS.SimpleOptionsUI.Editor {
    public static class CreateSimpleSettingsMenu {
        [MenuItem("GameObject/Tent City Studio/Create Simple Settings Menu")]
        public static void CreateSimpleSettingsMenuCommand() {
            var settingsMenu = Resources.Load<GameObject>("Prefabs/SimpleSettings");
            if (settingsMenu) {
                var instance = PrefabUtility.InstantiatePrefab(settingsMenu) as GameObject;
                if (!instance) return;

                instance.name = "[SimpleSettings]";
                Selection.activeGameObject = instance;
            }
        }
    }
}