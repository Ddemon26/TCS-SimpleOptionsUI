using UnityEngine;
using UnityEngine.InputSystem;
namespace TCS {
    [System.Serializable]
    public class DefaultActions {
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
    }
}