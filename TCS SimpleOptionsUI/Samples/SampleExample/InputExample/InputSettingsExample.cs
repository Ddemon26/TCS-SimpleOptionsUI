using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace TCS {
    public enum InputType { }

    public class InputSettingsExample : MonoBehaviour {
        public InputActionReference m_exampleAction;
        public InputBinding m_binding;
        public List<InputBinding> m_bindings = new();

        void Start() {

            if (m_exampleAction == null) {
                Debug.LogError("InputActionReference is null.");
                return;
            }

            m_exampleAction.action.Disable();

            // m_exampleAction.action.StartRebinding("Move", OnRebindingComplete);
        }

        void OnRebindingComplete() {
            Debug.Log("Rebinding complete!");
        }

        public void MethodBindTest() {
            m_binding = m_exampleAction.action.GetBindingInfo(0);
            m_bindings = m_exampleAction.action.bindings.ToList();
        }
    }

    public static class InputRebinder {
        //InputActionAsset m_inputActions;  
        // static InputActionRebindingExtensions.RebindingOperation activeRebindingOperation;

        // public InputRebinder(InputActionAsset inputActions) {
        //     m_inputActions = inputActions;
        // }

        /*public static void StartRebinding(this InputAction action, string actionName, Action onComplete = null) {
            //var action = asset.FindAction(actionName, true);
            if (action == null) {
                throw new ArgumentException($"Action '{actionName}' not found in the InputActionAsset.");
            }

            action.Disable();

            activeRebindingOperation = action.PerformInteractiveRebinding()
                .WithControlsExcluding("<Mouse>/position")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnComplete
                (
                    operation => {
                        operation.Dispose();
                        action.Enable();
                        onComplete?.Invoke();
                        activeRebindingOperation = null;
                    }
                )
                .OnCancel
                (
                    operation => {
                        operation.Dispose();
                        action.Enable();
                        activeRebindingOperation = null;
                    }
                )
                .Setup();
        }

        public static void StartRebinding(this InputActionAsset asset, string actionName, int bindingIndex, Action onComplete = null) {
            var action = asset.FindAction(actionName, true);
            if (action == null) {
                throw new ArgumentException($"Action '{actionName}' not found in the InputActionAsset.");
            }

            if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) {
                throw new ArgumentOutOfRangeException(nameof(bindingIndex), "Invalid binding index.");
            }

            action.Disable();

            activeRebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("<Mouse>/position")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnComplete
                (
                    operation => {
                        operation.Dispose();
                        action.Enable();
                        onComplete?.Invoke();
                        activeRebindingOperation = null;
                    }
                )
                .OnCancel
                (
                    operation => {
                        operation.Dispose();
                        action.Enable();
                        activeRebindingOperation = null;
                    }
                )
                .Setup();
        }*/
        
        public static InputBinding GetBindingInfo(this InputAction action, int binding) {
            return action?.bindings[binding] ?? default;
        }

        public static void RebindAction(this InputActionReference asset, string actionName, int bindingIndex, string newBinding) {
            var action = asset.action;
            if (action == null) {
                throw new ArgumentException($"Action '{actionName}' not found in the InputActionAsset.");
            }

            if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) {
                throw new ArgumentOutOfRangeException(nameof(bindingIndex), "Invalid binding index.");
            }

            action.ApplyBindingOverride(bindingIndex, newBinding);
        }

        public static void RemoveBindingOverride(this InputActionReference reference, string actionName, int bindingIndex) {
            var action = reference.action;
            if (action == null) {
                throw new ArgumentException($"Action '{actionName}' not found in the InputActionAsset.");
            }

            if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) {
                throw new ArgumentOutOfRangeException(nameof(bindingIndex), "Invalid binding index.");
            }

            action.RemoveBindingOverride(bindingIndex);
        }

        public static void SaveRebindings(this InputActionAsset asset) {
            string rebinds = asset.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("rebinds", rebinds);
            PlayerPrefs.Save();
        }

        public static void LoadRebindings(this InputActionAsset asset) {
            if (PlayerPrefs.HasKey("rebinds")) {
                string rebinds = PlayerPrefs.GetString("rebinds");
                asset.LoadBindingOverridesFromJson(rebinds);
            }
        }

        public static void ResetRebindings(this InputActionAsset asset) {
            foreach (var actionMap in asset.actionMaps) {
                actionMap.RemoveAllBindingOverrides();
            }
        }
    }
}