﻿using UnityEngine;
namespace TCS.Examples {
    public class SomeTestingClass : MonoBehaviour {
        [SerializeField] float _someFloat;
        SomeTestingUI _someTestingUI;

        void Awake() {
            _someTestingUI = GetComponent<SomeTestingUI>();
        }

        void Update() {
            _someTestingUI.SomeFloat = _someFloat;
        }
    }
}