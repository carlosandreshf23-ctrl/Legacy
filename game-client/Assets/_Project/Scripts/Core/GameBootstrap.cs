using LegadoPeru.Audio;
using LegadoPeru.Calendar;
using LegadoPeru.DebugTools;
using LegadoPeru.Dialogue;
using LegadoPeru.Inventory;
using LegadoPeru.Narrative;
using LegadoPeru.Persistence;
using LegadoPeru.Settings;
using LegadoPeru.World;
using UnityEngine;

namespace LegadoPeru.Core
{
    /// <summary>
    /// Punto de entrada único de la aplicación (prompt §3-4). Se autoinstala antes de que
    /// cargue la primera escena, sin importar si esa escena es MainMenu o Sandbox
    /// (útil para iterar directamente en el Editor). Sobrevive a los cambios de escena.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureBootstrap()
        {
            if (ServiceLocator.IsRegistered<GameBootstrap>()) return;

            var go = new GameObject("_GameBootstrap");
            var bootstrap = go.AddComponent<GameBootstrap>();
            Object.DontDestroyOnLoad(go);

            bootstrap.InitializeServices();
            ServiceLocator.Register(bootstrap);
        }

        private void InitializeServices()
        {
            var calendar = new GameCalendarSystem();
            calendar.Initialize(WorldDate.Default, minutesPerRealSecond: 4f);
            ServiceLocator.Register(calendar);

            ServiceLocator.Register(new DiscoverySystem());
            ServiceLocator.Register(new LocationSystem());
            ServiceLocator.Register(new DecisionService());
            ServiceLocator.Register(new WorldObjectRegistry());
            ServiceLocator.Register(new DialogueRunner());

            var itemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
            if (itemDatabase == null)
                DebugLog.LogWarning(DebugLog.Category.World,
                    "No se encontró Resources/ItemDatabase.asset. Ejecuta 'Legado > Fase 1 > 1. Seed Content Data' en el Editor.");
            ServiceLocator.Register(new InventoryModel(itemDatabase));

            var settings = new SettingsService();
            settings.Load();
            ServiceLocator.Register(settings);

            ServiceLocator.Register(new SaveSystem());

            var sfxSource = gameObject.AddComponent<AudioSource>();
            var ambientSource = gameObject.AddComponent<AudioSource>();
            ServiceLocator.Register(new AudioService(sfxSource, ambientSource));

            DebugLog.Log(DebugLog.Category.World, "GameBootstrap: servicios centrales inicializados.");
        }
    }
}
