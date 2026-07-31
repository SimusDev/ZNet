namespace ArmaMovement.States.Movement {
    public abstract class Base : States.Base
    {
        public virtual float Speed => 1.0f;
        public virtual float Acceleration => 10.0f;
        public virtual float Deceleration => 10.0f;
        public virtual float Friction => 1.0f;
        public virtual float HeadHeight => 1.8f;
        public virtual float TurnSpeed => 1.0f;
    }
}