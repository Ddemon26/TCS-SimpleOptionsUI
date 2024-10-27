using UnityEngine;
using UnityEngine.EventSystems;

namespace TCS.SimpleOptionsUI {
    [RequireComponent(typeof(EventSystem))]
    [ExecuteInEditMode]
    public class HandleEventSystem : MonoBehaviour {
        void Start() {
            HandleEventSystem[] instances = FindObjectsByType<HandleEventSystem>(FindObjectsSortMode.None);
            foreach (var instance in instances) {
                if (instance != this) {
                    Debug.LogWarning("There is already a EventSystem in the scene. Destroying this EventSystem.");

                    var eventSystem = GetComponent<EventSystem>();
                    if (eventSystem) {
                        DestroyImmediate(eventSystem.gameObject);
                    }

                    return;
                }

                CheckForInputSystem();
            }
        }

        void CheckForInputSystem() {
#if !ENABLE_INPUT_SYSTEM
            var eventSystem = GetComponent<EventSystem>();
            if (eventSystem) {
                DestroyImmediate(eventSystem.gameObject);
            }
#endif
        }
    }
}