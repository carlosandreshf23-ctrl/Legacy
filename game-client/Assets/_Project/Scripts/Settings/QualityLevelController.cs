using UnityEngine;

namespace LegadoPeru.Settings
{
    public enum QualityTier
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    /// <summary>
    /// Infraestructura mínima de niveles de calidad (prompt §39), alineada con los
    /// presupuestos de 10_MOBILE_PERFORMANCE.md. No es una pasada de optimización profunda:
    /// solo fija distancia de render/sombras por tier.
    /// </summary>
    public static class QualityLevelController
    {
        public static void Apply(QualityTier tier)
        {
            QualitySettings.SetQualityLevel((int)tier, true);

            float farClip = tier switch
            {
                QualityTier.Low => 175f,
                QualityTier.Medium => 300f,
                _ => 450f
            };

            if (Camera.main != null)
                Camera.main.farClipPlane = farClip;

            QualitySettings.shadows = tier == QualityTier.Low ? ShadowQuality.Disable : ShadowQuality.All;
        }
    }
}
