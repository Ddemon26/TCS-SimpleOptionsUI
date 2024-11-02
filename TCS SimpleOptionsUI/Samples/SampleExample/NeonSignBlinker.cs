using System;
using UnityEngine;

namespace TCS {
    public class NeonSignBlinker : MonoBehaviour,  IDisposable {
        MeshRenderer m_meshRenderer;
        Material m_materialA;
        Material m_materialB;
        readonly float m_minInterval;
        readonly float m_maxInterval;
        float m_timer;
        bool m_isBlinking;
        bool m_isMaterialAActive;
        float m_nextInterval;
        readonly System.Random m_random;

        public NeonSignBlinker(
            MeshRenderer meshRenderer,
            Material materialA,
            Material materialB,
            float minInterval = 0.1f,
            float maxInterval = 0.5f
        ) {
            m_meshRenderer = meshRenderer ?? throw new ArgumentNullException(nameof(meshRenderer));
            m_materialA = materialA ?? throw new ArgumentNullException(nameof(materialA));
            m_materialB = materialB ?? throw new ArgumentNullException(nameof(materialB));

            if (minInterval <= 0) {
                throw new ArgumentException("Min interval must be greater than zero.", nameof(minInterval));
            }

            if (maxInterval < minInterval) {
                throw new ArgumentException("Max interval must be greater than or equal to min interval.", nameof(maxInterval));
            }

            m_minInterval = minInterval;
            m_maxInterval = maxInterval;
            m_isMaterialAActive = true;
            m_meshRenderer.material = m_materialA;
            m_random = new System.Random();
            SetNextInterval();
        }

        public void StartFlickering() {
            if (m_isBlinking) return;
            m_isBlinking = true;
            m_timer = 0f;
        }

        public void StopFlickering() {
            if (!m_isBlinking) return;
            m_isBlinking = false;
            m_meshRenderer.material = m_materialA;
            m_isMaterialAActive = true;
        }

        public void Update() {
            if (!m_isBlinking) return;

            m_timer += Time.deltaTime;
            if (!(m_timer >= m_nextInterval)) return;
            m_timer -= m_nextInterval;
            ToggleMaterial();
            SetNextInterval();
        }

        void ToggleMaterial() {
            m_meshRenderer.material = m_isMaterialAActive ? m_materialB : m_materialA;
            m_isMaterialAActive = !m_isMaterialAActive;
        }

        void SetNextInterval() {
            m_nextInterval = (float)(m_minInterval + m_random.NextDouble() * (m_maxInterval - m_minInterval));
        }

        public void Dispose() {
            StopFlickering();
            m_meshRenderer = null;
            m_materialA = null;
            m_materialB = null;
            GC.SuppressFinalize(this);
        }

        // Destructor   
        ~NeonSignBlinker() {
            Dispose();
        }
    }
}