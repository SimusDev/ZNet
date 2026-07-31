namespace ArmaMovement.States.Movement {
    public class Prone : Base
    {
        public override float Speed => 0.0f;
        public override Godot.Vector3 HeadPosition => new(0.0f, 0.25f, 0.0f);
    }
}