using UnityEngine.UIElements;
namespace TCS.SimpleOptionsUI {
    public interface ISimpleSettingFactory {
        VisualTreeAsset GetTemplateForSetting(SettingBase setting);
    }
}