using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;
/// <summary>
/// This side is more complex than using the monobehaviour, we need to use <see cref="INotifyPropertyChanged"/>  to bind the UI elements.
/// the system is looking for INotifyPropertyChanged interface to bind ScriptableObjects to the UI elements.
/// we also need to use the SetField method to set the values and call OnPropertyChanged to notify the UI elements.
/// unreachable values are added to show that the values are not accessible from the Menu UI System.
/// instead we need to use the INotifyPropertyChanged interface to bind the UI elements. and use get and set methods to access the values.
/// while invoking the OnPropertyChanged method to notify the UI elements.
/// at the end we are using UnityEditor.EditorUtility.SetDirty(this); to make the changes persistent in the editor.
/// keeping the data persistent is a bit tricky, but if its necessary to keep the data persistent, we can use this method.
/// <see cref="OnPropertyChanged"/>
/// </summary>
[CreateAssetMenu(menuName = "Create SomeValueSO", fileName = "SomeValueSO", order = 0)]
public class SomeValueSo : ScriptableObject, INotifyPropertyChanged {
    [SerializeField] float m_unreachableFloatValue1;
    [SerializeField] float m_unreachableFloatValue2;
    [SerializeField] float m_unreachableFloatValue3;

    [SerializeField] int m_unreachableIntValue1;
    [SerializeField] int m_unreachableIntValue2;
    [SerializeField] int m_unreachableIntValue3;
    
    [SerializeField] SomeEnum m_unreachableEnumValue1;

    public event PropertyChangedEventHandler PropertyChanged;

    public float FloatValue1 {
        get => m_unreachableFloatValue1;
        set => SetField(ref m_unreachableFloatValue1, value);
    }
    public float FloatValue2 {
        get => m_unreachableFloatValue2;
        set => SetField(ref m_unreachableFloatValue2, value);
    }
    public float FloatValue3 {
        get => m_unreachableFloatValue3;
        set => SetField(ref m_unreachableFloatValue3, value);
    }

    public int IntValue1 {
        get => m_unreachableIntValue1;
        set => SetField(ref m_unreachableIntValue1, value);
    }
    public int IntValue2 {
        get => m_unreachableIntValue2;
        set => SetField(ref m_unreachableIntValue2, value);
    }
    public int IntValue3 {
        get => m_unreachableIntValue3;
        set => SetField(ref m_unreachableIntValue3, value);
    }
    
    public SomeEnum EnumValue1 {
        get => m_unreachableEnumValue1;
        set => SetField(ref m_unreachableEnumValue1, value);
    }

    void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this); // This is needed to make the changes persistent in the editor
#endif
        
        return true;
    }
}