using Godot;

namespace ArmaMovement
{
    [GlobalClass]
    public partial class PlayerInput : Node
    {
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookDelta { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsProne { get; private set; }
        public bool IsAiming { get; private set; }
        public bool IsJumping { get; private set; }
    
        private Vector2 _lookDeltaAccumulator;

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                _lookDeltaAccumulator += mouseMotion.Relative;
            }
        }

        public override void _Process(double delta)
        {
            MoveInput = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
            LookDelta = _lookDeltaAccumulator;
            _lookDeltaAccumulator = Vector2.Zero;

            IsSprinting = Input.IsActionPressed("sprint");
            IsCrouching = Input.IsActionPressed("crouch");
            IsProne = Input.IsActionPressed("prone");
            IsAiming = Input.IsActionPressed("aim");
            IsJumping = Input.IsActionPressed("jump");
        }
    }
}