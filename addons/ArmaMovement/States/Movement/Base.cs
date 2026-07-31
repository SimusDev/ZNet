namespace ArmaMovement.States.Movement {
    public abstract class Base : States.Base
    {
        public virtual float Speed => 1.0f;
        public virtual float Acceleration => 10.0f;
        public virtual float Deceleration => 10.0f;
        public virtual float Friction => 1.0f;
        public virtual Godot.Vector3 HeadPosition => new(0.0f, 1.5f, 0.0f);
        public virtual float TurnSpeed => 8.5f;
    }
}