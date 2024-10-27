using System;
using UnityEngine.UIElements;

namespace TCS.SimpleOptionsUI
{
    [Serializable] public class FloatSliderSetting : SliderSettingBase<float> {
        protected override BaseSlider<float> CreateSlider(VisualElement container) => container.Q<Slider>();
    }
}