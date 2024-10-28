using System;
using UnityEditor;
namespace TCS.SimpleOptionsUI.Editor {
    [CustomPropertyDrawer(typeof(IntSliderSetting))]
    public class IntSettingDrawer : BaseSettingDrawer {
        protected override bool ShowMinMaxFields => true;
        protected override Type TargetType => typeof(int);
        protected override string TypeName => "int";
    }
}