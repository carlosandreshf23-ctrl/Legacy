namespace LegadoPeru.Persistence
{
    /// <summary>
    /// Puente mínimo entre MainMenu (donde se elige qué cargar) y la escena Sandbox (donde
    /// se aplica el estado, una vez todos los PersistentWorldObject ya se registraron en su
    /// propio Awake). Deuda técnica documentada: es estado estático global; aceptable para
    /// el alcance de Fase 1 de una sola escena de juego.
    /// </summary>
    public static class PendingLoadRequest
    {
        public static SaveGameDataV1 Data { get; private set; }
        public static bool HasPending => Data != null;

        public static void Set(SaveGameDataV1 data) => Data = data;
        public static void Clear() => Data = null;
    }
}
