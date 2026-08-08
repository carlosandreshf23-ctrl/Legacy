namespace LegadoPeru.Core
{
    /// <summary>
    /// Identidad de build (prompt Fase 2 §45): toda versión jugable debe declarar
    /// Version/BuildNumber/Phase/Milestone. Se actualiza manualmente al cerrar cada
    /// milestone (ver CHANGELOG.md para el historial).
    /// </summary>
    public static class BuildInfo
    {
        public const string Version = "0.2.2-dev";
        public const int BuildNumber = 10;
        public const string Phase = "Fase 2 — Primer Vertical Slice Visual";
        public const string Milestone = "Milestone 2.1 — Look & Feel (iteración de fidelidad visual)";

        public static string DisplayString => $"LEGADO: PERÚ — build {Version} ({BuildNumber}) — {Phase} — {Milestone}";
    }
}
