namespace ArmaMovement.States {
    public class Running : BaseState
    {
        public override void Enter()
        {
            //Player.Animation.SetBool("IsMoving", true);
            //Player.Animation.SetBool("IsSprinting", true);
            //Player.Animation.SetBool("IsCrouching", false);
            //Player.Animation.SetBool("IsProne", false);
            PlayerController.AdjustStance(Stance.Standing);
        }

        public override void Update(double delta)
        {
            var input = PlayerController.PlayerInput;
            bool isMoving = input.MoveInput.LengthSquared() > 0.001f;

            //Player.Animation.SetBool("IsMoving", isMoving);
            //Player.Animation.SetBool("IsAiming", input.IsAiming);

            if (!isMoving)
            {
                TransitionTo<Idle>();
                return;
            }

            if (!input.IsSprinting)
                TransitionTo<Walking>();
            else if (input.IsCrouching)
                TransitionTo<Crouching>();
            else if (input.IsProne)
                TransitionTo<Prone>();
        }
    }
}