using LegadoPeru.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LegadoPeru.InputSystem
{
    /// <summary>
    /// Fachada de input consultada por gameplay (Move/Look/Interact/Run/Inventory, prompt §12).
    /// Selecciona automáticamente proveedor táctil o de teclado/mouse en Awake() (nunca antes:
    /// Application.isMobilePlatform en tiempo de Editor siempre es false, por eso la decisión
    /// se toma en runtime, no al construir la escena). Cachea los valores del frame en Update()
    /// (ejecutado temprano) para que todos los consumidores lean el mismo valor ese frame.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameInput : MonoBehaviour
    {
        [SerializeField] private bool forceTouchInEditor;

        [Header("Referencias táctiles (asignadas por el builder de escena)")]
        [SerializeField] private VirtualJoystick touchJoystick;
        [SerializeField] private TouchLookPad touchLookPad;
        [SerializeField] private Button interactButton;
        [SerializeField] private Button runButton;
        [SerializeField] private GameObject touchControlsRoot;

        private IInputProvider provider;

        public TouchInputProvider TouchProvider { get; private set; }

        public Vector2 MoveAxis { get; private set; }
        public Vector2 LookDelta { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool RunHeld { get; private set; }
        public bool InventoryTogglePressed { get; private set; }
        public bool PausePressed { get; private set; }

        public bool UsingTouch { get; private set; }

        public void ConfigureTouchReferences(VirtualJoystick joystick, TouchLookPad lookPad, Button interact, Button run, GameObject controlsRoot)
        {
            touchJoystick = joystick;
            touchLookPad = lookPad;
            interactButton = interact;
            runButton = run;
            touchControlsRoot = controlsRoot;
        }

        private void Awake()
        {
            UsingTouch = Application.isMobilePlatform || forceTouchInEditor;

            if (touchControlsRoot != null) touchControlsRoot.SetActive(UsingTouch);

            if (UsingTouch)
            {
                TouchProvider = new TouchInputProvider();
                TouchProvider.Configure(touchJoystick, touchLookPad);
                provider = TouchProvider;

                if (interactButton != null)
                    interactButton.onClick.AddListener(() => TouchProvider.QueueInteract());

                if (runButton != null)
                {
                    var trigger = runButton.gameObject.AddComponent<EventTrigger>();
                    AddTrigger(trigger, EventTriggerType.PointerDown, () => TouchProvider.SetRunHeld(true));
                    AddTrigger(trigger, EventTriggerType.PointerUp, () => TouchProvider.SetRunHeld(false));
                }
            }
            else
            {
                provider = new KeyboardMouseInputProvider();
            }

            ServiceLocator.Register(this);
        }

        private void Update()
        {
            MoveAxis = provider.GetMoveAxis();
            LookDelta = provider.GetLookDelta();
            InteractPressed = provider.GetInteractDown();
            RunHeld = provider.GetRunHeld();
            InventoryTogglePressed = provider.GetInventoryToggleDown();
            PausePressed = provider.GetPauseDown();
        }

        private static void AddTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(_ => action());
            trigger.triggers.Add(entry);
        }
    }
}
