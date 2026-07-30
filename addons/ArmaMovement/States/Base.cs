namespace ArmaMovement.States {
    public abstract class Base
    {
        public virtual void OnStateAdded(Components.StateMachine stateMachine) {  }
        public virtual void OnStateRemoved(Components.StateMachine stateMachine) {  }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update(double delta) { }
        public virtual void PhysicsUpdate(double delta) { }
    }
}