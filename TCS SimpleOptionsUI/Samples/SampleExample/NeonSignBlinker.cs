using System.Collections;
using UnityEngine;

namespace TCS {
    public class MaterialBlinker {
        readonly MeshRenderer m_meshRenderer;
        readonly Material m_materialA;
        readonly Material m_materialB;
        bool m_isMaterialAActive = true;

        public MaterialBlinker(MeshRenderer renderer, Material matA, Material matB) {
            m_meshRenderer = renderer ?? throw new System.ArgumentNullException(nameof(renderer));
            m_materialA = matA ?? throw new System.ArgumentNullException(nameof(matA));
            m_materialB = matB ?? throw new System.ArgumentNullException(nameof(matB));

            // Initialize with Material A
            m_meshRenderer.material = m_materialA;
        }

        public void ToggleMaterial() {
            if (!m_meshRenderer) return;

            m_meshRenderer.material = m_isMaterialAActive ? m_materialB : m_materialA;
            m_isMaterialAActive = !m_isMaterialAActive;
        }

        public void SetMaterialA() {
            if (!m_meshRenderer) return;

            m_meshRenderer.material = m_materialA;
            m_isMaterialAActive = true;
        }

        public void SetMaterialB() {
            if (!m_meshRenderer) return;

            m_meshRenderer.material = m_materialB;
            m_isMaterialAActive = false;
        }

        public bool IsMaterialAActive => m_isMaterialAActive;
    }

    [RequireComponent(typeof(MeshRenderer))]
    public class NeonSignBlinker : MonoBehaviour {
        [Header("Materials")]
        [SerializeField] Material m_materialA;
        [SerializeField] Material m_materialB;

        [Header("Blinking Settings")]
        [SerializeField] float m_minInterval = 0.1f;
        [SerializeField] float m_maxInterval = 0.5f;
        [SerializeField] float m_minOnPeriod = 1.0f; // Minimum duration for the "on" (stable) period
        [SerializeField] float m_maxOnPeriod = 3.0f; // Maximum duration for the "on" (stable) period
        [SerializeField] int m_minFlickers = 3; // Minimum number of flickers in unstable state
        [SerializeField] int m_maxFlickers = 8; // Maximum number of flickers in unstable state

        MeshRenderer m_meshRenderer;
        MaterialBlinker m_materialBlinker;
        Coroutine m_blinkCoroutine;

        void Awake() {
            m_meshRenderer = GetComponent<MeshRenderer>();

            if (!m_materialA) {
                Debug.LogError("Material A is not assigned.", this);
                enabled = false;
                return;
            }

            if (!m_materialB) {
                Debug.LogError("Material B is not assigned.", this);
                enabled = false;
                return;
            }

            // Initialize the MaterialBlinker
            m_materialBlinker = new MaterialBlinker(m_meshRenderer, m_materialA, m_materialB);
        }

        void Start() => m_blinkCoroutine = StartCoroutine(BlinkRoutine());

        IEnumerator BlinkRoutine() {
            while (true) {
                // Stable "on" period
                float onDuration = Random.Range(m_minOnPeriod, m_maxOnPeriod);
                m_materialBlinker.SetMaterialA();
                yield return new WaitForSeconds(onDuration);

                // Flickering phase
                int flickerCount = Random.Range(m_minFlickers, m_maxFlickers + 1);
                for (var i = 0; i < flickerCount; i++) {
                    float interval = Random.Range(m_minInterval, m_maxInterval);
                    m_materialBlinker.ToggleMaterial();
                    yield return new WaitForSeconds(interval);
                }

                // Ensure the material is set back to Material A after flickering
                if (!m_materialBlinker.IsMaterialAActive) {
                    m_materialBlinker.SetMaterialA();
                }
            }
        }

        void OnDestroy() {
            if (m_blinkCoroutine != null) {
                StopCoroutine(m_blinkCoroutine);
            }
        }

        public void StartBlinking() => m_blinkCoroutine ??= StartCoroutine(BlinkRoutine());

        public void StopBlinking() {
            if (m_blinkCoroutine == null) return;

            StopCoroutine(m_blinkCoroutine);
            m_blinkCoroutine = null;
            m_materialBlinker.SetMaterialA(); // Reset to Material A when stopping
        }
    }
}