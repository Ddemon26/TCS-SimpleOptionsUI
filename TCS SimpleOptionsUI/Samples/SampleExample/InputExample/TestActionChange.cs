using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace TCS {
    public class TestActionChange : MonoBehaviour {
        [SerializeField] InputActionReference m_spaceAction;
        Action<InputAction.CallbackContext> m_spaceActionHandler;

        void OnEnable() {
            m_spaceActionHandler = _ => OnSpace();
            m_spaceAction.action.performed += m_spaceActionHandler;
            m_spaceAction.action.Enable();
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