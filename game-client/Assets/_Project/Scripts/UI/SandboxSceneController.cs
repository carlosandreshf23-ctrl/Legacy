using LegadoPeru.Core;
using LegadoPeru.DebugTools;
using LegadoPeru.Persistence;
using UnityEngine;

namespace LegadoPeru.UI
{
    /// <summary>
    /// Punto de entrada de la escena Sandbox. Aplica una carga pendiente (si MainMenu la
    /// dejó preparada) en Start(), momento en el que TODOS los Awake() de la escena
    /// (incluidos los PersistentWorldObject) ya se ejecutaron — orden garantizado por Unity.
    /// </summary>
    public class SandboxSceneController : MonoBehaviour
    {
        private void Start()
        {
            if (!PendingLoadRequest.HasPending) return;

            ServiceLocator.Get<SaveSystem>().ApplyLoadedData(PendingLoadRequest.Data);
            PendingLoadRequest.Clear();
            DebugLog.Log(DebugLog.Category.Save, "Carga pendiente aplicada al iniciar Sandbox.");
        }
    }
}
