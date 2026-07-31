namespace ArmaMovement.States.Movement {
    public class CrouchedRunning : Base
    {
        public override float Speed => 2.5f;
        public override Godot.Vector3 HeadPosition => new(0.0f, 1.2f, 0.0f);
    }
}