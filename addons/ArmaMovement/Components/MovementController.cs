using Godot;

namespace ArmaMovement.Components
{
    [GlobalClass]
    public partial class MovementController : Node
    {
        private Vector2 _moveInput2D = new();
        public Vector3 MoveInput => new(_moveInput2D.X, 0.0f, _moveInput2D.Y);
        [Export] public StateMachine StateMachine { get; private set; }

        public bool WantsSprint = false;
        public bool WantsSlowDown = false;
        public bool WantsCrouch = false;
        public bool WantsProne = false;
        public bool WantsJump = false;

        public override void _Process(double delta)
        {
            _moveInput2D = Input.GetVector(
                "movement.move_left",
                "movement.move_right",
                "movement.move_forward",
                "movement.move_back"
                );

            WantsSprint = Input.IsActionPressed("movement.sprint");
            WantsSlowDown = Input.IsActionPressed("movement.slow_down");
            WantsCrouch = Input.IsActionPressed("movement.crouch");
            WantsProne = Input.IsActionPressed("movement.prone");
            WantsJump = Input.IsActionPressed("movement.jump");
        }
    }
}