namespace ArmaMovement.States {
    public class Prone : BaseState
    {
        public override void Enter()
        {
            // Player.Animation.SetBool("IsMoving", false);
            // Player.Animation.SetBool("IsSprinting", false);
            // Player.Animation.SetBool("IsCrouching", false);
            // Player.Animation.SetBool("IsProne", true);
            PlayerController.AdjustStance(Stance.Prone);
        }

        public override void Update(double delta)
        {
            var input = PlayerController.PlayerInput;
            bool isMoving = input.MoveInput.LengthSquared() > 0.001f;

            // Player.Animation.SetBool("IsMoving", isMoving);
            // Player.Animation.SetBool("IsAiming", input.IsAiming);

            if (input.IsCrouching)
            {
                TransitionTo<Crouching>();
                return;
            }

            if (!input.IsProne)
            {
                if (isMoving)
                    TransitionTo<Walking>();
                else
                    TransitionTo<Idle>();
            }
        }
    }
}