namespace LegadoPeru.Core
{
    /// <summary>Payload de EventBus: cambia el texto de prompt de interacción en el HUD (o null para ocultarlo).</summary>
    public readonly struct InteractionPromptChangedEvent
    {
        public readonly string Prompt;
        public InteractionPromptChangedEvent(string prompt) => Prompt = prompt;
    }

    /// <summary>Payload de EventBus: cambio de stamina del jugador, consumido por el HUD.</summary>
    public readonly struct StaminaChangedEvent
    {
        public readonly float Current;
        public readonly float Max;

        public StaminaChangedEvent(float current, float max)
        {
            Current = current;
            Max = max;
        }
    }
}
