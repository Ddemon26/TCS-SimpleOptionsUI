using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace TCS {
    public static class InputBindInfo {
        // public static InputBinding GetBindingInfo(this InputAction action, int binding) {
        //     return action?.bindings[binding] ?? default;
        // }
    }

    public class TestActionChange : MonoBehaviour {
        [SerializeField] InputActionReference m_spaceAction;
        Action<InputAction.CallbackContext> m_spaceActionHandler;
        [Range(0, 5)] [SerializeField]
        int m_selectedBinding;
        [Header("Binding Info - DO NOT Edit")] [SerializeField]
        InputBinding m_inputBinding;

        int m_bindingIndex;
        int m_selectBinding;
        string m_actionName;

        void OnEnable() {
            m_spaceActionHandler = _ => OnSpace();
            m_spaceAction.action.performed += m_spaceActionHandler;
            m_spaceAction.action.Enable();

            m_inputBinding = m_spaceAction.action.GetBindingInfo(m_selectedBinding);
        }

        void OnDisable() {
            m_spaceAction.action.performed -= m_spaceActionHandler;
            m_spaceAction.action.Disable();
        }

        void OnSpace() {
            transform.position += new Vector3(0, Random.Range(-1f, 1f), 0);
        }
    }
}