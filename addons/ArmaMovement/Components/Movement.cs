using Godot;

namespace ArmaMovement.Components {
    [GlobalClass]
    public partial class Movement : Node
    {
        [Export] private PlayerHead _playerHead;
        [Export] private MovementController _movementController;
        [Export] private Node3D _body;

        [ExportGroup("Settings")]
        [Export] public float Gravity = -9.8f;
        [Export] private float turnSpeed = 6.0f; // Radians



        public override void _PhysicsProcess(double delta)
        {

            RotateBodyTowards((float)delta);
        }

        private void RotateBodyTowards(float delta)
        {
            float maxDelta = turnSpeed * delta;

            float newY = Mathf.LerpAngle(Mathf.DegToRad(_body.Rotation.Y), _playerHead.TargetBodyRotationY, maxDelta);
            _body.RotateY(newY);
        }

    }
}