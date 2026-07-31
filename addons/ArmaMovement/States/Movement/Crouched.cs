namespace ArmaMovement.States.Movement {
    public class Crouched : Base
    {
        public override float Speed => 0.0f;
        public override Godot.Vector3 HeadPosition => new(0.0f, 1.2f, 0.0f);
    }
}