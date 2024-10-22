using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[Serializable]
public abstract class SettingBase {
    public string m_label;
    public Object m_targetObject; // if a scriptable object is used, this should be a reference to the scriptable object HARD REFERENCE.
    public string m_variableName;

    protected const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    protected void SetScriptableObjectTarget<T>(T target) where T : ScriptableObject => m_targetObject = target;

    protected bool ValidateTargetAndVariableName(out string errorMessage) {
        errorMessage = string.Empty;
        if (!m_targetObject) {
            errorMessage = $"Target Object is not set for setting: {m_label}";
            return false;
        }

        if (string.IsNullOrEmpty(m_variableName)) {
            errorMessage = $"Variable Name is not set for setting: {m_label}";
            return false;
        }

        return true;
    }

    protected FieldInfo GetFieldInfo() {
        string[] splitName = m_variableName.Split('/');
        if (splitName.Length != 2)
            return m_targetObject.GetType().GetField(m_variableName, BINDING_FLAGS);

        string componentName = splitName[0];
        string fieldName = splitName[1];

        Component[] components = m_targetObject switch {
            GameObject go => go.GetComponents<Component>(),
            Component comp => comp.gameObject.GetComponents<Component>(),
            _ => null
        };

        return components?
            .Where(comp => comp && comp.GetType().Name == componentName)
            .Select(comp => comp.GetType().GetField(fieldName, BINDING_FLAGS))
            .FirstOrDefault();
    }

    protected object GetActualTargetObject() {
        if (m_targetObject is not GameObject go) return m_targetObject;
        string[] splitName = m_variableName.Split('/');
        if (splitName.Length != 2) return m_targetObject;

        string componentName = splitName[0];
        return go.GetComponents<Component>().FirstOrDefault(comp => comp.GetType().Name == componentName) ?? m_targetObject;
    }

    // Abstract method for creating the UI element
    public abstract VisualElement CreateUIElement(VisualTreeAsset template);
}

[Serializable]
public class FloatSetting : SettingBase {
    public float m_minValue;
    public float m_maxValue;

    public override VisualElement CreateUIElement(VisualTreeAsset template) {
        if (!ValidateTargetAndVariableName(out string errorMessage)) {
            Debug.LogError(errorMessage);
            return null;
        }

        var fieldInfo = GetFieldInfo();
        if (!ValidateFieldType(fieldInfo)) return null;

        object actualTarget = GetActualTargetObject();
        return CreateSliderUI
        (
            template,
            (float)fieldInfo.GetValue(actualTarget),
            evt => fieldInfo.SetValue(actualTarget, evt.newValue)
        );
    }

    protected bool ValidateFieldType(FieldInfo fieldInfo) {
        if (fieldInfo != null && fieldInfo.FieldType == typeof(float)) return true;
        Debug.LogError($"Field '{m_variableName}' on object '{m_targetObject}' is not a float.");
        return false;
    }

    VisualElement CreateSliderUI(VisualTreeAsset template, float currentValue, EventCallback<ChangeEvent<float>> onValueChanged) {
        VisualElement container = template.CloneTree();
        container.Q<Label>().text = m_label;

        var slider = container.Q<Slider>();
        slider.lowValue = m_minValue;
        slider.highValue = m_maxValue;
        slider.value = currentValue;

        // Use UIElements binding system
        slider.bindingPath = m_variableName;
        slider.Bind(new SerializedObject((Object)GetActualTargetObject()));

        // Update the field value on change
        slider.RegisterValueChangedCallback(onValueChanged);
        return container;
    }
}


[RequireComponent(typeof(UIDocument))]
public class MainSettingsMenu : MonoBehaviour {
    [SerializeField] UIDocument m_uiDocument;
    [SerializeField] VisualTreeAsset m_floatSliderSetting;
    [SerializeField] VisualTreeAsset m_intSliderSetting;
    [SerializeField] VisualTreeAsset m_enumSetting;
    [SerializeReference] List<SettingBase> m_settings = new();

    void Start() {
        m_uiDocument ??= GetComponent<UIDocument>();
        var root = m_uiDocument.rootVisualElement;

        foreach (var setting in m_settings) {
            var uiElement = setting.CreateUIElement(GetTemplateForSetting(setting));
            if (uiElement != null) {
                root.Add(uiElement);
            }
        }
    }

    VisualTreeAsset GetTemplateForSetting(SettingBase setting) {
        return setting switch {
            FloatSetting => m_floatSliderSetting,
            _ => null
        };
    }
}