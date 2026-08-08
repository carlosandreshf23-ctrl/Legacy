using System.IO;
using UnityEngine;

namespace LegadoPeru.Settings
{
    /// <summary>Carga/guarda GameSettingsData en un archivo separado del save de campaña (prompt §37).</summary>
    public class SettingsService
    {
        private const string FileName = "settings.json";
        private string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public GameSettingsData Data { get; private set; } = new GameSettingsData();

        public void Load()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    string json = File.ReadAllText(FilePath);
                    Data = JsonUtility.FromJson<GameSettingsData>(json) ?? new GameSettingsData();
                }
                catch
                {
                    Data = new GameSettingsData();
                }
            }

            Apply();
        }

        public void Save()
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(Data, true));
        }

        public void Apply()
        {
            AudioListener.volume = Data.masterVolume;
            QualityLevelController.Apply((QualityTier)Mathf.Clamp(Data.qualityLevel, 0, 2));
        }
    }
}
