using System.Collections.Generic;
using LegadoPeru.Calendar;
using LegadoPeru.Characters;
using LegadoPeru.Inventory;
using LegadoPeru.Narrative;
using LegadoPeru.World;

namespace LegadoPeru.Persistence
{
    /// <summary>
    /// DTO raíz de guardado (prompt §29-32). SaveVersion presente desde el primer save
    /// (prompt §31), aunque hoy solo exista la versión 1. Usa JsonUtility: sin Dictionary,
    /// sin referencias a GameObject — solo listas de structs/clases [Serializable] e IDs.
    /// </summary>
    [System.Serializable]
    public class SaveGameDataV1
    {
        public int saveVersion = 1;
        public string savedAtIso;

        public PlayerStateDto player;
        public WorldDate worldDate;
        public string currentLocationId;

        public List<ItemStack> inventory = new List<ItemStack>();
        public List<DecisionRecord> decisions = new List<DecisionRecord>();
        public List<WorldObjectStateDto> worldObjects = new List<WorldObjectStateDto>();
        public List<DiscoveryEntryDto> discovery = new List<DiscoveryEntryDto>();
    }
}
