using System;
using UnityEngine.UIElements;

namespace TCS.SimpleOptionsUI
{
    [Serializable] public class IntSliderSetting : SliderSettingBase<int> {
        protected override BaseSlider<int> CreateSlider(VisualElement container) => container.Q<SliderInt>();
    }
}