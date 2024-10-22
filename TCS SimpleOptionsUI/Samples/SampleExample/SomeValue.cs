using UnityEngine;
public enum SomeEnum {
    Value1,
    Value2,
    Value3
}
/// <summary>
/// When using a simple monobehaviour class, we can directly access the values from the Menu UI System.
/// making mono behaviours more accessible and easier to use.
/// </summary>
public class SomeValue : MonoBehaviour {
    public float m_floatValue1;
    public float m_floatValue2;
    public float m_floatValue3;
    
    public int m_intValue1;
    public int m_intValue2;
    public int m_intValue3;
    
    public SomeEnum m_enumValue1;
}