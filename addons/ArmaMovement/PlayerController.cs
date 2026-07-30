using Godot;

namespace ArmaMovement
{
    [GlobalClass]
    public partial class PlayerController : Node3D
    {
        [Export] public CharacterBody3D CharacterBody { get; private set; }
        [Export] public Node3D BodyMesh { get; private set; }
        [Export] public Camera3D Camera { get; private set; }
        [Export] public PlayerInput PlayerInput { get; private set; }
        [Export] public PlayerMovement PlayerMovement { get; private set; }
        [Export] public PlayerStateMachine StateMachine { get; private set; }
        [Export] public AnimationPlayer Animation { get; private set; }

        [ExportGroup("Stance Settings")]
        [Export] public float StandingHeight = 2.0f;
        [Export] public float CrouchingHeight = 1.2f;
        [Export] public float ProneHeight = 0.5f;
        [Export] public float StandingCameraOffset = 1.8f;
        [Export] public float CrouchingCameraOffset = 0.8f;
        [Export] public float ProneCameraOffset = 0.2f;

        [ExportGroup("Mouse Sensitivity")]
        [Export] public float MouseSensitivity = 0.01f;

        private CollisionShape3D _collisionShape;
        private Vector3 _collisionShapeCenter;

        public Vector3 CameraForward => -Camera.GlobalTransform.Basis.Z;
        public Vector3 CameraRight => Camera.GlobalTransform.Basis.X;

        public override void _Ready()
        {
            // Получение ссылок, если не назначены в инспекторе
            CharacterBody ??= GetNode<CharacterBody3D>("CharacterBody3D");
            BodyMesh ??= GetNode<Node3D>("BodyMesh");
            Camera ??= GetNode<Camera3D>("Camera3D");
            PlayerInput ??= GetNode<PlayerInput>("PlayerInput");
            PlayerMovement ??= GetNode<PlayerMovement>("PlayerMovement");
            StateMachine ??= GetNode<PlayerStateMachine>("PlayerStateMachine");
            Animation ??= GetNode<AnimationPlayer>("AnimationPlayer");

            _collisionShape = CharacterBody.GetNode<CollisionShape3D>("CollisionShape3D");
            if (_collisionShape.Shape is CapsuleShape3D capsule)
            {
                //_collisionShapeCenter = capsule;
            }

            PlayerMovement.Initialize(CharacterBody, BodyMesh);
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }

        public override void _Input(InputEvent @event)
        {
            // Поворот камеры мышью
            if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                Camera.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
                Camera.RotateY(-mouseMotion.Relative.X * MouseSensitivity);

                // Ограничение вертикального угла
                Vector3 rot = Camera.Rotation;
                rot.X = Mathf.Clamp(rot.X, -Mathf.Pi / 2.2f, Mathf.Pi / 2.2f);
                Camera.Rotation = rot;
            }

            // Переключение захвата мыши по Escape
            if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
            {
                Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                    ? Input.MouseModeEnum.Visible
                    : Input.MouseModeEnum.Captured;
            }
        }

        /// <summary>
        /// Изменяет стойку: высоту коллизии и позицию камеры.
        /// </summary>
        public void AdjustStance(Stance stance)
        {
            float height = 0f;
            float cameraOffset = 0f;

            switch (stance)
            {
                case Stance.Standing:
                    height = StandingHeight;
                    cameraOffset = StandingCameraOffset;
                    break;
                case Stance.Crouching:
                    height = CrouchingHeight;
                    cameraOffset = CrouchingCameraOffset;
                    break;
                case Stance.Prone:
                    height = ProneHeight;
                    cameraOffset = ProneCameraOffset;
                    break;
            }

            if (_collisionShape.Shape is CapsuleShape3D capsule)
            {
                capsule.Height = height;
                Vector3 center = _collisionShapeCenter;
                center.Y = height / 2.0f;
                _collisionShape.Position = center;
            }

            Camera.Position = new Vector3(0, cameraOffset, 0);
        }
    }
}