using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace TCS {
    public enum DeviceType {
        Keyboard,
        Gamepad,
        Mouse,
        Touchscreen,
        XRController,
        Joystick,
    }

    public enum GamepadButton {
        ButtonSouth,
        ButtonNorth,
        ButtonEast,
        ButtonWest,
        LeftTrigger,
        RightTrigger,
        LeftBumper,
        RightBumper,
        DpadUp,
        DpadDown,
        DpadLeft,
        DpadRight,
        LeftStick,
        RightStick,
    }

    public class InputSettingsExample : MonoBehaviour {
        [Header("Input Actions")]
        [SerializeField] InputActionReference m_attackAction;
        [SerializeField] InputActionReference m_crouchAction;
        [SerializeField] InputActionReference m_interactAction;
        [SerializeField] InputActionReference m_jumpAction;
        [SerializeField] InputActionReference m_lookAction;
        [SerializeField] InputActionReference m_moveAction;
        [SerializeField] InputActionReference m_nextAction;
        [SerializeField] InputActionReference m_previousAction;
        [SerializeField] InputActionReference m_sprintAction;

        [Header("UI Actions")]
        [SerializeField] InputActionReference m_cancelAction;
        [SerializeField] InputActionReference m_clickAction;
        [SerializeField] InputActionReference m_middleClickAction;
        [SerializeField] InputActionReference m_navigateAction;
        [SerializeField] InputActionReference m_pointAction;
        [SerializeField] InputActionReference m_rightClickAction;
        [SerializeField] InputActionReference m_scrollAction;
        [SerializeField] InputActionReference m_submitAction;
        [SerializeField] InputActionReference m_trackedDeviceOrientationAction;
        [SerializeField] InputActionReference m_trackedDevicePositionAction;

        void Start() {
            // Log the current bindings
            m_jumpAction.action.LogDeviceTypesAndIndices();
            m_attackAction.action.LogDeviceTypesAndIndices();

            // Change the bindings
            MethodBindTest();

            // Log the new bindings
            Debug.Log($"New Keyboard binding: {m_jumpAction.action.GetControlFromAction(DeviceType.Keyboard)}");
            Debug.Log($"New Gamepad binding: {m_jumpAction.action.GetControlFromAction(DeviceType.Gamepad)}");
        }


        public void MethodBindTest() {
            // Change keyboard and gamepad binding independently
            m_jumpAction.action.RebindAction(DeviceType.Keyboard, Key.J);
            m_jumpAction.action.RebindAction(DeviceType.Gamepad, GamepadButton.ButtonSouth);
        }
    }

    public static class InputActionBinder {
        public static void RebindKeyboardAction(this InputAction action, string control) {
            int bindingIndex = action.GetBindingIndexForDevice(DeviceType.Keyboard);
            if (bindingIndex != -1) {
                string deviceString = BuildDeviceString(DeviceType.Keyboard, control);
                action.ApplyBindingOverride(bindingIndex, deviceString);
            }
            else {
                Debug.LogWarning($"Keyboard binding not found for action {action.name}");
            }
        }

        public static void RebindGamepadAction(this InputAction action, GamepadButton button) {
            int bindingIndex = action.GetBindingIndexForDevice(DeviceType.Gamepad);
            if (bindingIndex != -1) {
                var control = button.ToString();
                string deviceString = BuildDeviceString(DeviceType.Gamepad, control);
                action.ApplyBindingOverride(bindingIndex, deviceString);
            }
            else {
                Debug.LogWarning($"Gamepad binding not found for action {action.name}");
            }
        }

        public static void RebindMouseAction(this InputAction action, string control) {
            int bindingIndex = action.GetBindingIndexForDevice(DeviceType.Mouse);
            if (bindingIndex != -1) {
                string deviceString = BuildDeviceString(DeviceType.Mouse, control);
                action.ApplyBindingOverride(bindingIndex, deviceString);
            }
            else {
                Debug.LogWarning($"Mouse binding not found for action {action.name}");
            }
        }

        public static void RebindAction(this InputAction action, DeviceType deviceType, GamepadButton button) {
            if (deviceType != DeviceType.Gamepad) {
                Debug.LogError("Invalid device type for GamepadButton input. Expected Gamepad.");
                return;
            }

            int bindingIndex = action.GetBindingIndexForDevice(deviceType);
            if (bindingIndex != -1) {   
                string deviceString = BuildDeviceString(deviceType, button.ToString());
                action.ApplyBindingOverride(bindingIndex, deviceString);
            } else {
                Debug.LogWarning($"{deviceType} binding not found for action {action.name}");
            }
        }

        public static void RebindAction(this InputAction action, DeviceType deviceType, Key control) {
            int bindingIndex = action.GetBindingIndexForDevice(deviceType);
            if (bindingIndex != -1) {
                string deviceString = BuildDeviceString(deviceType, control.ToString());
                action.ApplyBindingOverride(bindingIndex, deviceString);
            } else {
                Debug.LogWarning($"{deviceType} binding not found for action {action.name}");
            }
        }


        public static string GetControlFromAction(this InputAction action, DeviceType deviceType) {
            int bindingIndex = action.GetBindingIndexForDevice(deviceType);
            return bindingIndex != -1 ? GetControlFromBinding(action.bindings[bindingIndex].effectivePath) : null;
        }

        public static void LogDeviceTypesAndIndices(this InputAction action) {
            ReadOnlyArray<InputBinding> bindings = action.bindings;

            for (var i = 0; i < bindings.Count; i++) {
                var binding = bindings[i];
                string deviceLayout = InputControlPath.TryGetDeviceLayout(binding.effectivePath);
                if (!string.IsNullOrEmpty(deviceLayout)) {
                    Debug.Log($"Index: {i}, Device type: {deviceLayout}");
                }
            }
        }

        static int GetBindingIndexForDevice(this InputAction action, DeviceType deviceType) {
            ReadOnlyArray<InputBinding> bindings = action.bindings;
            for (var i = 0; i < bindings.Count; i++) {
                var binding = bindings[i];
                if (binding.isComposite || binding.isPartOfComposite) continue;
                string deviceLayout = InputControlPath.TryGetDeviceLayout(binding.effectivePath);

                if (string.IsNullOrEmpty(deviceLayout)) continue;
                if (deviceLayout == deviceType.ToString())
                    return i;
            }

            return -1;
        }
    
        static string BuildDeviceString(DeviceType device, string control) => $"<{device}>/{control.ToLower()}";

        static string GetControlFromBinding(string binding) {
            int index = binding.IndexOf('/');
            return index >= 0 ? binding.Substring(index + 1) : binding;
        }
    }
}