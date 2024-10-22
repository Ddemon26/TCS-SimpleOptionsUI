using UnityEngine;

public class TestSomeValueSO : MonoBehaviour {
    public SomeValueSo m_someValueSo;
    public bool useIntValues;
    void Update() {
        if (!useIntValues) {
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
    }
}