using UnityEngine;
public class TestSomeValue : MonoBehaviour {
    public SomeValue m_someValue;
    public bool m_useIntValues;
    MeshRenderer m_meshRenderer;
    void Awake() {
        m_meshRenderer = GetComponent<MeshRenderer>();
    }
    void Update() {
        if (!m_useIntValues) {
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

        m_meshRenderer.material.color = m_someValue.m_enumValue1 switch {
            SomeEnum.Value1 => Color.red,
            SomeEnum.Value2 => Color.green,
            SomeEnum.Value3 => Color.blue,
            _ => m_meshRenderer.material.color
        };
    }
    
    public void SomeMethod() {
        m_someValue.m_floatValue2 = Random.Range(0f, 6f);
    }
}