using Godot;

namespace ArmaMovement.Components
{
    [GlobalClass]
    public partial class PlayerHead : Node3D
    {
        [Export] private Camera3D _camera;
        public Camera3D Camera => _camera;

        [Export] private CharacterBody3D _characterBody;
        public CharacterBody3D CharacterBody => _characterBody;

        private float _mouseSensitivity = 0.0f;
        const float SENS_NORMALIZE_VALUE = 0.01f;

        private float _targetBodyRotY = 0.0f;
        public float TargetBodyRotationY => _targetBodyRotY;

        public override void _Ready()
        {
            _mouseSensitivity = ProjectSettings.GetSetting("input/mouse_sensitivity", 1.0f).As<float>();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                float totalSens = _mouseSensitivity * SENS_NORMALIZE_VALUE;
                RotateX(-mouseMotion.Relative.Y * totalSens);
                RotateY(mouseMotion.Relative.X * totalSens);
                
                _targetBodyRotY = Rotation.Y;

                Vector3 rot = Rotation;
                rot.X = Mathf.Clamp(rot.X, -Mathf.Pi / 2.2f, Mathf.Pi / 2.2f);
                Rotation = rot;
            }

            if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
            {
                Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                    ? Input.MouseModeEnum.Visible
                    : Input.MouseModeEnum.Captured;
            }
        }
    }
}