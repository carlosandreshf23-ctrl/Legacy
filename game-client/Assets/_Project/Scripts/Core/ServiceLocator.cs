using System;
using System.Collections.Generic;

namespace LegadoPeru.Core
{
    /// <summary>
    /// Registro central de servicios de gameplay (Core.EventBus / Core.ServiceLocator, ver 01_ARCHITECTURE.md §3).
    /// Los sistemas se registran una vez (normalmente desde GameBootstrap) y se consultan por tipo,
    /// evitando referencias directas entre sistemas.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

        public static void Register<T>(T instance) where T : class
        {
            Services[typeof(T)] = instance;
        }

        public static T Get<T>() where T : class
        {
            if (Services.TryGetValue(typeof(T), out var instance))
                return (T)instance;

            throw new InvalidOperationException(
                $"[ServiceLocator] No hay un servicio registrado de tipo {typeof(T).Name}. " +
                "Verifica que GameBootstrap se haya ejecutado antes de este acceso.");
        }

        public static bool TryGet<T>(out T instance) where T : class
        {
            if (Services.TryGetValue(typeof(T), out var raw))
            {
                instance = (T)raw;
                return true;
            }

            instance = null;
            return false;
        }

        public static bool IsRegistered<T>() where T : class => Services.ContainsKey(typeof(T));

        /// <summary>Solo para tests EditMode: limpia todos los servicios registrados entre casos de prueba.</summary>
        public static void Clear() => Services.Clear();
    }
}
