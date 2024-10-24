using System;
using TCS.SimpleOptionsUI;
using UnityEngine;

namespace TCS {
    public class SomeTestingUI : UISettingBehaviour {
        [UISetting] public int _someInt;
        [UISetting] public bool m_someBool;
        [UISetting] public float s_someFloat;
        public string somePrefix_someString;
        public Vector3 _someVector3;
        public Color _someColor;

        // void OnValidate() {
        //     OnPropertyChanged(nameof(_someInt));
        //     OnPropertyChanged(nameof(m_someBool));
        //     OnPropertyChanged(nameof(s_someFloat));
        // }

        public int SomeInt {
            get => _someInt;
            //debug log the set value
            set {
                Debug.Log("SomeInt set to: " + value);
                SetField(ref _someInt, value);
            }
        }

        public bool SomeBool {
            get => m_someBool;
            set {
                Debug.Log("SomeBool set to: " + value);
                SetField(ref m_someBool, value);
            }
        }

        public float SomeFloat {
            get => s_someFloat;
            set => SetField(ref s_someFloat, value);
        }
    }
}