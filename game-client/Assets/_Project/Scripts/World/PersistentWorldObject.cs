using LegadoPeru.Core;
using UnityEngine;

namespace LegadoPeru.World
{
    /// <summary>
    /// Base para objetos de mundo con identidad persistente (prompt §33), independiente de
    /// cualquier referencia de GameObject/escena — coherente con 01_ARCHITECTURE.md §5
    /// (identificadores persistentes, nunca referencias de escena como identidad narrativa).
    /// </summary>
    public abstract class PersistentWorldObject : MonoBehaviour
    {
        [SerializeField] protected string persistentId;

        public string PersistentId => persistentId;

        public void SetPersistentId(string id) => persistentId = id;

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(persistentId))
                Debug.LogWarning($"[World] '{name}' no tiene PersistentId asignado; no se guardará/restaurará.");

            if (ServiceLocator.TryGet(out WorldObjectRegistry registry))
                registry.Register(this);
        }

        /// <summary>Serializa el estado relevante como string simple (JsonUtility-friendly a nivel del contenedor).</summary>
        public abstract string CaptureState();

        public abstract void RestoreState(string state);
    }
}
