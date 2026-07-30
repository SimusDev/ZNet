namespace ArmaMovement.States {
    public abstract class BaseState
    {
        protected PlayerController PlayerController;

        public virtual void Initialize(PlayerController playerController)
        {
            PlayerController = playerController;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update(double delta) { }

        public virtual void PhysicsUpdate(double delta)
        {
            var input = PlayerController.PlayerInput;
            var movement = PlayerController.PlayerMovement;
            bool isGrounded = PlayerController.CharacterBody.IsOnFloor();

            movement.UpdateMovement(
                (float)delta,
                input.MoveInput,
                input.IsSprinting,
                input.IsAiming,
                input.IsCrouching,
                input.IsProne,
                isGrounded,
                PlayerController.CameraForward,
                PlayerController.CameraRight
            );
        }

        protected void TransitionTo<T>() where T : BaseState
        {
            PlayerController.StateMachine.ChangeState<T>();
        }
    }
}