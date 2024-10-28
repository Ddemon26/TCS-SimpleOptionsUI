using System;
using System.Collections.Generic;

namespace TCS.SimpleOptionsUI
{
    public static class SettingFactory {
        static ISettingBase floatSliderSetting;
        static ISettingBase intSliderSetting;
        static ISettingBase enumFieldSetting;
        static ISettingBase toggleFieldSetting;
        static ISettingBase buttonFieldSetting;
        
        static readonly Dictionary<string, Func<ISettingBase>> SettingCreators = new() {
            // { "Float", () => floatSliderSetting ??= new FloatSliderSetting() },
            // { "Int", () => intSliderSetting ??= new IntSliderSetting() },
            // { "Enum", () => enumFieldSetting ??= new EnumFieldSetting() },
            // { "Toggle", () => toggleFieldSetting ??= new ToggleFieldSetting() },
            // { "Button", () => buttonFieldSetting ??= new ButtonFieldSetting() }
        };

        public static ISettingBase CreateSetting(string key) {
            if (SettingCreators.TryGetValue(key, out Func<ISettingBase> creator)) {
                return creator();
            }
            throw new ArgumentException($"No setting found for key: {key}");
        }
    }
}
