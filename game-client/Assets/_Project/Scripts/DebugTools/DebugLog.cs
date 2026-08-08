using System;
using UnityEngine;

namespace LegadoPeru.DebugTools
{
    /// <summary>
    /// Logging categorizado (Fase 1, prompt §40). Cada categoría puede desactivarse
    /// independientemente para evitar spam de consola; en builds de shipping futuras
    /// se puede fijar EnabledCategories = Category.None sin tocar los call sites.
    /// </summary>
    public static class DebugLog
    {
        [Flags]
        public enum Category
        {
            None = 0,
            Save = 1 << 0,
            Interaction = 1 << 1,
            World = 1 << 2,
            Player = 1 << 3,
            Time = 1 << 4,
            Narrative = 1 << 5,
            All = ~0
        }

        public static Category EnabledCategories = Category.All;

        public static void Log(Category category, string message)
        {
            if ((EnabledCategories & category) == 0) return;
            Debug.Log($"[{category}] {message}");
        }

        public static void LogWarning(Category category, string message)
        {
            if ((EnabledCategories & category) == 0) return;
            Debug.LogWarning($"[{category}] {message}");
        }
    }
}
