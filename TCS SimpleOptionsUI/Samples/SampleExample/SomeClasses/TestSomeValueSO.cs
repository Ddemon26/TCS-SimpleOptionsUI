﻿using UnityEngine;
namespace TCS.Examples {
    public class TestSomeValueSO : MonoBehaviour {
        public SomeValueSo m_someValueSo;
        MeshRenderer m_meshRenderer;
        void Awake() {
            m_meshRenderer = GetComponent<MeshRenderer>();
        }
        void Update() {
            if (!m_someValueSo.BoolValue) {
                transform.position = new Vector3
                (
                    m_someValueSo.FloatValue1, // x
                    m_someValueSo.FloatValue2, // y
                    m_someValueSo.FloatValue3 // z
                );
            }
            else {
                transform.position = new Vector3
                (
                    m_someValueSo.IntValue1, // x
                    m_someValueSo.IntValue2, // y
                    m_someValueSo.IntValue3 // z
                );
            }
        
            m_meshRenderer.material.color = m_someValueSo.EnumValue switch {
                SomeEnum.Value1 => Color.red,
                SomeEnum.Value2 => Color.green,
                SomeEnum.Value3 => Color.blue,
                _ => m_meshRenderer.material.color,
            };
        }
    }
}