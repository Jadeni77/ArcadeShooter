using UnityEngine;

namespace ArcadeShooter.UI
{
    // Local presentation preferences. No setting here changes network or mission state.
    public static class LocalGameSettings
    {
        private const string SensitivityKey = "ArcadeShooter.Input.MouseSensitivity";
        private const string FullscreenKey = "ArcadeShooter.Display.Fullscreen";
        private const string WidthKey = "ArcadeShooter.Display.Width";
        private const string HeightKey = "ArcadeShooter.Display.Height";
        private static float? cachedSensitivity;

        public static float MouseSensitivity
        {
            get => cachedSensitivity ??= Mathf.Clamp(PlayerPrefs.GetFloat(SensitivityKey, 0.12f), 0.04f, 0.4f);
            set
            {
                cachedSensitivity = Mathf.Clamp(value, 0.04f, 0.4f);
                PlayerPrefs.SetFloat(SensitivityKey, cachedSensitivity.Value);
                PlayerPrefs.Save();
            }
        }

        public static bool Fullscreen
        {
            get => PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) != 0;
            set
            {
                PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
                PlayerPrefs.Save();
                Screen.fullScreen = value;
            }
        }

        public static void ApplySavedDisplay()
        {
            int width = PlayerPrefs.GetInt(WidthKey, 0);
            int height = PlayerPrefs.GetInt(HeightKey, 0);
            if (width > 0 && height > 0)
                Screen.SetResolution(width, height, Fullscreen);
        }

        public static void SetResolution(Resolution resolution)
        {
            PlayerPrefs.SetInt(WidthKey, resolution.width);
            PlayerPrefs.SetInt(HeightKey, resolution.height);
            PlayerPrefs.Save();
            Screen.SetResolution(resolution.width, resolution.height, Fullscreen);
        }
    }
}
