using Godot;

namespace ArmaMovement.Components
{
    [GlobalClass]
    public partial class MovementController : Node
    {
        public Vector2 MoveInput { get; private set; }
        [Export] public StateMachine StateMachine { get; private set; }

        public bool WantsSprint = false;
        public bool WantsCrouch = false;
        public bool WantsProne = false;
        public bool WantsJump = false;

        public override void _Process(double delta)
        {
            MoveInput = Input.GetVector("move_left", "move_right", "move_forward", "move_back");

            WantsSprint = Input.IsActionPressed("sprint");
            WantsCrouch = Input.IsActionPressed("crouch");
            WantsProne = Input.IsActionPressed("prone");
            WantsJump = Input.IsActionPressed("jump");
        }
    }
}