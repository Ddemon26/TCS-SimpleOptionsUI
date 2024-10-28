using System;
using UnityEditor;

namespace TCS.SimpleOptionsUI.Editor {
    [CustomPropertyDrawer(typeof(FloatSliderSetting))]
    public class FloatSettingDrawer : BaseSettingDrawer {
        protected override bool ShowMinMaxFields => true;
        protected override Type TargetType => typeof(float);
        protected override string TypeName => "float";
    }
}