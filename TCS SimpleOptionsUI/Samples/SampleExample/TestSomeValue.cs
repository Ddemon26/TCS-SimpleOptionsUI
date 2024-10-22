using UnityEngine;

public class TestSomeValue : MonoBehaviour {
    public SomeValue m_someValue;
    public bool useIntValues;
    void Update() {
        if (!useIntValues) {
            transform.position = new Vector3
            (
                m_someValue.m_floatValue1, // x
                m_someValue.m_floatValue2, // y
                m_someValue.m_floatValue3 // z
            );
        }
        else {
            transform.position = new Vector3
            (
                m_someValue.m_intValue1, // x
                m_someValue.m_intValue2, // y
                m_someValue.m_intValue3 // z
            );
        }
    }
}