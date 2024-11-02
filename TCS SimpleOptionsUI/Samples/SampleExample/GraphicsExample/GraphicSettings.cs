using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace TCS {
    public enum QualityLevel {
        Low = 0,
        Medium = 1,
        High = 2
    }
    
    public enum Resolution {
        _800X600 = 0,
        _1024X768 = 1,
        _1280X720 = 2,
        _1366X768 = 3,
        _1600X900 = 4,
        _1920X1080 = 5
    }
    
    [Serializable] public class GraphicsSettingsData {
        public int m_qualityLevel;
        public int m_resolutionWidth;
        public int m_resolutionHeight;
        public bool m_fullscreen;
        public int m_vSyncCount;
        public int m_antiAliasing;
        public int m_textureQuality;
        public ShadowQuality m_shadowQuality;
    }

    public class GraphicSettings {
        readonly RenderPipelineAsset[] m_qualityLevels;
        
        GraphicsSettingsData m_currentSettings;
        
        public GraphicSettings() {
            LoadSettings();
        }
        
        public GraphicSettings(RenderPipelineAsset[] qualityLevels, GraphicsSettingsData settings) {
            m_qualityLevels = qualityLevels;
            m_currentSettings = settings;
        }

        #region Properties
        public int QualityLevel {
            get => m_currentSettings.m_qualityLevel;
            set => SetQualityLevel(value);
        }

        public int ResolutionWidth {
            get => m_currentSettings.m_resolutionWidth;
            set => SetResolution(value, ResolutionHeight, Fullscreen);
        }

        public int ResolutionHeight {
            get => m_currentSettings.m_resolutionHeight;
            set => SetResolution(ResolutionWidth, value, Fullscreen);
        }

        public bool Fullscreen {
            get => m_currentSettings.m_fullscreen;
            set => SetFullscreen(value);
        }

        public int VSyncCount {
            get => m_currentSettings.m_vSyncCount;
            set => SetVSyncCount(value);
        }

        public int AntiAliasing {
            get => m_currentSettings.m_antiAliasing;
            set => SetAntiAliasing(value);
        }

        public int TextureQuality {
            get => m_currentSettings.m_textureQuality;
            set => SetTextureQuality(value);
        }

        public ShadowQuality ShadowQuality {
            get => m_currentSettings.m_shadowQuality;
            set => SetShadowQuality(value);
        }
        #endregion

        #region Methods
        public void SetQualityLevel(int level) {
            m_currentSettings.m_qualityLevel = level;
            QualitySettings.SetQualityLevel(level);
            if (m_qualityLevels != null && level >= 0 && level < m_qualityLevels.Length) {
                QualitySettings.renderPipeline = m_qualityLevels[level];
            }

            SaveSettings();
        }

        public void SetResolution(int width, int height, bool fullscreen) {
            m_currentSettings.m_resolutionWidth = width;
            m_currentSettings.m_resolutionHeight = height;
            m_currentSettings.m_fullscreen = fullscreen;
            Screen.SetResolution(width, height, fullscreen);
            SaveSettings();
        }

        public void SetFullscreen(bool fullscreen) {
            m_currentSettings.m_fullscreen = fullscreen;
            Screen.fullScreen = fullscreen;
            SaveSettings();
        }

        public void SetVSyncCount(int count) {
            m_currentSettings.m_vSyncCount = count;
            QualitySettings.vSyncCount = count;
            SaveSettings();
        }

        public void SetAntiAliasing(int level) {
            m_currentSettings.m_antiAliasing = level;
            QualitySettings.antiAliasing = level;
            SaveSettings();
        }

        public void SetTextureQuality(int level) {
            m_currentSettings.m_textureQuality = level;
            QualitySettings.globalTextureMipmapLimit = level;
            SaveSettings();
        }

        public void SetShadowQuality(ShadowQuality quality) {
            m_currentSettings.m_shadowQuality = quality;
            QualitySettings.shadows = quality;
            SaveSettings();
        }

        public void ApplySettings() {
            // Apply all settings without recursion
            QualitySettings.SetQualityLevel(m_currentSettings.m_qualityLevel);
            if (m_qualityLevels != null && m_currentSettings.m_qualityLevel >= 0 && m_currentSettings.m_qualityLevel < m_qualityLevels.Length) {
                QualitySettings.renderPipeline = m_qualityLevels[m_currentSettings.m_qualityLevel];
            }

            Screen.SetResolution(m_currentSettings.m_resolutionWidth, m_currentSettings.m_resolutionHeight, m_currentSettings.m_fullscreen);
            QualitySettings.vSyncCount = m_currentSettings.m_vSyncCount;
            QualitySettings.antiAliasing = m_currentSettings.m_antiAliasing;
            QualitySettings.globalTextureMipmapLimit = m_currentSettings.m_textureQuality;
            QualitySettings.shadows = m_currentSettings.m_shadowQuality;
        }

        public void SaveSettings() {
            // Save current settings to PlayerPrefs
            PlayerPrefs.SetInt("QualityLevel", m_currentSettings.m_qualityLevel);
            PlayerPrefs.SetInt("ResolutionWidth", m_currentSettings.m_resolutionWidth);
            PlayerPrefs.SetInt("ResolutionHeight", m_currentSettings.m_resolutionHeight);
            PlayerPrefs.SetInt("Fullscreen", m_currentSettings.m_fullscreen ? 1 : 0);
            PlayerPrefs.SetInt("VSyncCount", m_currentSettings.m_vSyncCount);
            PlayerPrefs.SetInt("AntiAliasing", m_currentSettings.m_antiAliasing);
            PlayerPrefs.SetInt("TextureQuality", m_currentSettings.m_textureQuality);
            PlayerPrefs.SetInt("ShadowQuality", (int)m_currentSettings.m_shadowQuality);
            PlayerPrefs.Save();
        }

        public void LoadSettings() {
            m_currentSettings = new GraphicsSettingsData {
                m_qualityLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel()),
                m_resolutionWidth = PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width),
                m_resolutionHeight = PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height),
                m_fullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1,
                m_vSyncCount = PlayerPrefs.GetInt("VSyncCount", QualitySettings.vSyncCount),
                m_antiAliasing = PlayerPrefs.GetInt("AntiAliasing", QualitySettings.antiAliasing),
                m_textureQuality = PlayerPrefs.GetInt("TextureQuality", QualitySettings.globalTextureMipmapLimit),
                m_shadowQuality = (ShadowQuality)PlayerPrefs.GetInt("ShadowQuality", (int)QualitySettings.shadows)
            };
        }

        public void ResetToDefaults() {
            PlayerPrefs.DeleteKey("QualityLevel");
            PlayerPrefs.DeleteKey("ResolutionWidth");
            PlayerPrefs.DeleteKey("ResolutionHeight");
            PlayerPrefs.DeleteKey("Fullscreen");
            PlayerPrefs.DeleteKey("VSyncCount");
            PlayerPrefs.DeleteKey("AntiAliasing");
            PlayerPrefs.DeleteKey("TextureQuality");
            PlayerPrefs.DeleteKey("ShadowQuality");
            LoadSettings();
            ApplySettings();
        }

        public GraphicsSettingsData GetCurrentSettings() => m_currentSettings;
        #endregion
    }
}