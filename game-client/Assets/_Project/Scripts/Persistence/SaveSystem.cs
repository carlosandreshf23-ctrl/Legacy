using System;
using System.IO;
using LegadoPeru.Calendar;
using LegadoPeru.Characters;
using LegadoPeru.Core;
using LegadoPeru.DebugTools;
using LegadoPeru.Inventory;
using LegadoPeru.Narrative;
using LegadoPeru.World;
using UnityEngine;

namespace LegadoPeru.Persistence
{
    /// <summary>
    /// Primera versión funcional de SaveSystem (prompt §29-34; ver 08_SAVE_SYSTEM.md).
    /// Guarda un DTO único por slot (MVP local-only de Fase 1 — la arquitectura de
    /// snapshot+log completa de 08_SAVE_SYSTEM.md se implementa contra este mismo contrato
    /// cuando exista backend real, Fase 8-9). Mantiene una copia .bak antes de sobrescribir.
    /// </summary>
    public class SaveSystem
    {
        private const int CurrentSaveVersion = 1;

        private string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");
        private string PathFor(string slot) => Path.Combine(SaveDirectory, slot + ".json");
        private string BackupPathFor(string slot) => Path.Combine(SaveDirectory, slot + ".bak.json");

        public bool HasSave(string slot) => File.Exists(PathFor(slot));

        public void SaveToSlot(string slot)
        {
            Directory.CreateDirectory(SaveDirectory);

            var data = BuildSaveData();
            string json = JsonUtility.ToJson(data, true);

            string target = PathFor(slot);
            if (File.Exists(target))
                File.Copy(target, BackupPathFor(slot), true);

            string tempPath = target + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Copy(tempPath, target, true);
            File.Delete(tempPath);

            DebugLog.Log(DebugLog.Category.Save, $"Saved to slot '{slot}' ({json.Length} bytes).");
        }

        public bool TryLoadFromSlot(string slot, out SaveGameDataV1 data)
        {
            if (TryReadAndParse(PathFor(slot), out data)) return true;

            DebugLog.Log(DebugLog.Category.Save, $"Save primario de '{slot}' ausente/corrupto, probando backup.");
            if (TryReadAndParse(BackupPathFor(slot), out data)) return true;

            data = null;
            return false;
        }

        public void DeleteSave(string slot)
        {
            if (File.Exists(PathFor(slot))) File.Delete(PathFor(slot));
            if (File.Exists(BackupPathFor(slot))) File.Delete(BackupPathFor(slot));
        }

        private bool TryReadAndParse(string path, out SaveGameDataV1 data)
        {
            data = null;
            if (!File.Exists(path)) return false;

            try
            {
                string json = File.ReadAllText(path);
                var raw = JsonUtility.FromJson<SaveGameDataV1>(json);
                data = Migrate(raw);
                return data != null;
            }
            catch (Exception e)
            {
                DebugLog.LogWarning(DebugLog.Category.Save, $"No se pudo parsear el save en {path}: {e.Message}");
                return false;
            }
        }

        /// <summary>Placeholder de migración (prompt §31): hoy solo existe la v1, no hay nada que migrar todavía.</summary>
        private SaveGameDataV1 Migrate(SaveGameDataV1 raw)
        {
            if (raw == null) return null;
            if (raw.saveVersion == CurrentSaveVersion) return raw;

            DebugLog.LogWarning(DebugLog.Category.Save,
                $"SaveVersion {raw.saveVersion} distinta de la actual ({CurrentSaveVersion}); no hay migrador registrado aún.");
            return raw;
        }

        private SaveGameDataV1 BuildSaveData()
        {
            return new SaveGameDataV1
            {
                saveVersion = CurrentSaveVersion,
                savedAtIso = DateTime.UtcNow.ToString("o"),
                player = ServiceLocator.Get<PlayerCharacterController>().CaptureState(),
                worldDate = ServiceLocator.Get<GameCalendarSystem>().CaptureState(),
                currentLocationId = ServiceLocator.Get<LocationSystem>().CurrentLocationId,
                inventory = ServiceLocator.Get<InventoryModel>().CaptureState(),
                decisions = ServiceLocator.Get<DecisionService>().CaptureState(),
                worldObjects = ServiceLocator.Get<WorldObjectRegistry>().CaptureAll(),
                discovery = ServiceLocator.Get<DiscoverySystem>().CaptureState()
            };
        }

        /// <summary>
        /// Debe llamarse una vez que todos los PersistentWorldObject de la escena ya se
        /// registraron (ver SandboxSceneController.Start, que corre después de todos los Awake).
        /// </summary>
        public void ApplyLoadedData(SaveGameDataV1 data)
        {
            if (data == null) return;

            ServiceLocator.Get<PlayerCharacterController>().RestoreState(data.player);
            ServiceLocator.Get<GameCalendarSystem>().RestoreState(data.worldDate);
            ServiceLocator.Get<InventoryModel>().RestoreState(data.inventory);
            ServiceLocator.Get<DecisionService>().RestoreState(data.decisions);
            ServiceLocator.Get<WorldObjectRegistry>().RestoreAll(data.worldObjects);
            ServiceLocator.Get<DiscoverySystem>().RestoreState(data.discovery);
            ServiceLocator.Get<LocationSystem>().RestoreState(data.currentLocationId);

            DebugLog.Log(DebugLog.Category.Save, "Save data aplicado a los sistemas en vivo.");
        }
    }
}
