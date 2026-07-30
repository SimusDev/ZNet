namespace ArmaMovement.States {
    public class Crouching : BaseState
    {
        public override void Enter()
        {
            // Player.Animation.SetBool("IsMoving", false);
            // Player.Animation.SetBool("IsSprinting", false);
            // Player.Animation.SetBool("IsCrouching", true);
            // Player.Animation.SetBool("IsProne", false);
            PlayerController.AdjustStance(Stance.Crouching);
        }

        public override void Update(double delta)
        {
            var input = PlayerController.PlayerInput;
            bool isMoving = input.MoveInput.LengthSquared() > 0.001f;

            // Player.Animation.SetBool("IsMoving", isMoving);
            // Player.Animation.SetBool("IsAiming", input.IsAiming);

            if (input.IsProne)
            {
                TransitionTo<Prone>();
                return;
            }

            if (!input.IsCrouching)
            {
                if (isMoving)
                    TransitionTo<Walking>();
                else
                    TransitionTo<Idle>();
            }
        }
    }
}