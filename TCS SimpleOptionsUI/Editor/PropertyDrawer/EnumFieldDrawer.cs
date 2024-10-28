using System;
using UnityEditor;
namespace TCS.SimpleOptionsUI.Editor {
    [CustomPropertyDrawer(typeof(EnumFieldSetting))]
    public class EnumFieldDrawer : BaseSettingDrawer {
        protected override bool ShowMinMaxFields => false;
        protected override Type TargetType => typeof(Enum);
        protected override string TypeName => "enum";
    }
}