using System;
using System.Collections.Generic;

namespace LegadoPeru.World
{
    [Serializable]
    public class WorldObjectStateDto
    {
        public string persistentId;
        public string state;
    }

    /// <summary>
    /// Registro genérico de PersistentWorldObject por PersistentId (prompt §33-34). SaveSystem
    /// captura/restaura a través de este registro sin conocer los tipos concretos de cada objeto.
    /// </summary>
    public class WorldObjectRegistry
    {
        private readonly Dictionary<string, PersistentWorldObject> objects = new Dictionary<string, PersistentWorldObject>();

        public void Register(PersistentWorldObject obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.PersistentId)) return;
            objects[obj.PersistentId] = obj;
        }

        public List<WorldObjectStateDto> CaptureAll()
        {
            var list = new List<WorldObjectStateDto>();
            foreach (var kvp in objects)
                list.Add(new WorldObjectStateDto { persistentId = kvp.Key, state = kvp.Value.CaptureState() });
            return list;
        }

        public void RestoreAll(List<WorldObjectStateDto> states)
        {
            if (states == null) return;
            foreach (var entry in states)
                if (objects.TryGetValue(entry.persistentId, out var obj))
                    obj.RestoreState(entry.state);
        }

        /// <summary>Solo para tests EditMode.</summary>
        public void ClearForTests() => objects.Clear();
    }
}
