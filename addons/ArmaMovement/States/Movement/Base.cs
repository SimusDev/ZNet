namespace ArmaMovement.States.Movement {
    public abstract class Base : States.Base
    {
        public float Speed = 1.0f;
        public float Acceleration = 10.0f;
        public float Deceleration = 10.0f;
        public float Friction = 1.0f;
    }
}