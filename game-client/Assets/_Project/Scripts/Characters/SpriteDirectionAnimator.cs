using System;
using UnityEngine;

namespace LegadoPeru.Characters
{
    /// <summary>
    /// Anima un SpriteRenderer intercambiando frames segun direccion (Down/Left/Right/Up) y
    /// si el personaje se esta moviendo (prompt Fase 2 §3/§15): Idle + Walk (3 frames por
    /// direccion: idle, stepA, stepB). Right reutiliza los frames de Left con flipX en vez de
    /// duplicar arte (prompt: "Left puede reflejar Right" — aqui Right es la reflejada).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteDirectionAnimator : MonoBehaviour
    {
        [Serializable]
        public class DirectionalFrames
        {
            public Sprite[] down = new Sprite[3];
            public Sprite[] left = new Sprite[3];
            public Sprite[] up = new Sprite[3];

            [Tooltip("Opcional: si se deja vacio, 'right' se genera reflejando 'left' (flipX).")]
            public Sprite[] right = new Sprite[0];
        }

        [SerializeField] private DirectionalFrames frames;
        [SerializeField] private float framesPerSecond = 6f;

        private SpriteRenderer spriteRenderer;
        private float animTimer;
        private int frameIndex;
        private CharacterMotor motor;

        private enum Direction { Down, Left, Up, Right }
        private Direction currentDirection = Direction.Down;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            motor = GetComponentInParent<CharacterMotor>();
            ApplyFrame();
        }

        public void SetFrames(DirectionalFrames directionalFrames) => frames = directionalFrames;

        private void Update()
        {
            if (motor == null) return;

            currentDirection = ToDirection(motor.FacingDirection);

            if (motor.IsMoving)
            {
                animTimer += Time.deltaTime;
                float frameDuration = 1f / Mathf.Max(0.01f, framesPerSecond);
                if (animTimer >= frameDuration)
                {
                    animTimer = 0f;
                    frameIndex = frameIndex == 1 ? 2 : 1; // alterna stepA/stepB (indices 1 y 2, ver Art Bible)
                }
            }
            else
            {
                frameIndex = 0; // idle
                animTimer = 0f;
            }

            ApplyFrame();
        }

        private void ApplyFrame()
        {
            if (frames == null) return;

            bool useRightMirror = currentDirection == Direction.Right && (frames.right == null || frames.right.Length == 0);
            Sprite[] set = currentDirection switch
            {
                Direction.Down => frames.down,
                Direction.Left => frames.left,
                Direction.Up => frames.up,
                Direction.Right => useRightMirror ? frames.left : frames.right,
                _ => frames.down
            };

            if (set == null || set.Length == 0) return;
            int idx = Mathf.Clamp(frameIndex, 0, set.Length - 1);
            spriteRenderer.sprite = set[idx];
            spriteRenderer.flipX = useRightMirror;
        }

        private static Direction ToDirection(Vector2 facing)
        {
            if (facing == Vector2.left) return Direction.Left;
            if (facing == Vector2.right) return Direction.Right;
            if (facing == Vector2.up) return Direction.Up;
            return Direction.Down;
        }
    }
}
