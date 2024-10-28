using System;
using UnityEditor;
namespace TCS.SimpleOptionsUI.Editor {
    [CustomPropertyDrawer(typeof(ToggleFieldSetting))]
    public class ToggleFieldDrawer : BaseSettingDrawer {
        protected override bool ShowMinMaxFields => false;
        protected override Type TargetType => typeof(bool);
        protected override string TypeName => "bool";
    }
}