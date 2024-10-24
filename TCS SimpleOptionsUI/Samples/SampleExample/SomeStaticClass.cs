using System;
using System.Reflection;
using UnityEngine;

namespace TCS {
    public static class StaticClassReflectionExample {
        public static void Main() {
            Type staticClassType = typeof(SomeStaticClass);

            Debug.Log($"Class: {staticClassType.Name}");

            // Use BindingFlags.Static to get static methods
            MethodInfo[] methods = staticClassType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods) {
                Debug.Log($"Method: {method.Name}");
            }
        }
    }

    public static class SomeStaticClass {
        public static void StaticMethod1() {
            Debug.Log("StaticMethod1");
        }

        private static void StaticMethod2() {
            Debug.Log("StaticMethod2");
        }
    }
}