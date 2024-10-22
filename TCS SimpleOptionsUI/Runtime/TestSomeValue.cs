using UnityEngine;

public class TestSomeValue : MonoBehaviour {
    public SomeValue m_someValue;
    void Update() {
        transform.position = new Vector3
        (
            m_someValue.m_value1, // x
            m_someValue.m_value2, // y
            m_someValue.m_value3 // z
        );
    }
}