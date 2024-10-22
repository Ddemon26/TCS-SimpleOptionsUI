using UnityEngine;

public class TestSomeValue : MonoBehaviour {
    public SomeValue m_someValue;
    public bool m_switch;
    void Update() {
        transform.position = m_switch 
            ? new Vector3(m_someValue.m_value1, m_someValue.m_value2, m_someValue.m_value3)
            : new Vector3(m_someValue.m_value4, m_someValue.m_value5, m_someValue.m_value6);
    }
}