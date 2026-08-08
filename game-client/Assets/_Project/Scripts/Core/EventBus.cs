using System;
using System.Collections.Generic;

namespace LegadoPeru.Core
{
    /// <summary>
    /// Bus de eventos desacoplado (ver 01_ARCHITECTURE.md §3). Se usa para notificaciones
    /// transversales orientadas a UI (prompt de interacción, stamina) donde el emisor no debe
    /// conocer a sus suscriptores. Los cambios de estado de dominio (inventario, diálogo,
    /// calendario) usan eventos C# directos en el servicio correspondiente en lugar de este bus.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Handlers = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            Handlers[type] = Handlers.TryGetValue(type, out var existing)
                ? Delegate.Combine(existing, handler)
                : handler;
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!Handlers.TryGetValue(type, out var existing)) return;

            var combined = Delegate.Remove(existing, handler);
            if (combined == null) Handlers.Remove(type);
            else Handlers[type] = combined;
        }

        public static void Publish<T>(T evt)
        {
            if (Handlers.TryGetValue(typeof(T), out var existing) && existing is Action<T> action)
                action.Invoke(evt);
        }

        /// <summary>Solo para tests EditMode.</summary>
        public static void ClearAll() => Handlers.Clear();
    }
}
