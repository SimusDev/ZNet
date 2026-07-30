using Godot;

namespace ZNet.Source.World;
public partial class FreeCamera3D : Camera3D
{
    [Export] public float Speed = 10f;
    [Export] public float Sensitivity = 0.5f;
    [Export] public float Acceleration = 20f;

    private Vector3 _velocity;
    private float _yaw, _pitch;
    private bool _captured = false;

    public override void _Ready()
    {
        CaptureMouse();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && _captured)
        {
            _yaw -= mouseMotion.Relative.X * Sensitivity * 0.01f;
            _pitch -= mouseMotion.Relative.Y * Sensitivity * 0.01f;
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);

            Rotation = new Vector3(_pitch, _yaw, 0);
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            if (_captured)
                ReleaseMouse();
            else
                CaptureMouse();
        }

        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.WheelUp)
                Speed *= 1.05f;
            else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
                Speed /= 1.05f;
        }
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        Vector3 input = Vector3.Zero;

        if (Input.IsKeyPressed(Key.W)) input.Z -= 1;
        if (Input.IsKeyPressed(Key.S)) input.Z += 1;
        if (Input.IsKeyPressed(Key.A)) input.X -= 1;
        if (Input.IsKeyPressed(Key.D)) input.X += 1;
        if (Input.IsKeyPressed(Key.Q)) input.Y -= 1;
        if (Input.IsKeyPressed(Key.E)) input.Y += 1;

        float currentSpeed = Speed;
        if (Input.IsKeyPressed(Key.Shift)) currentSpeed *= 3f;

        if (input.Length() > 0)
            input = input.Normalized();

        Vector3 targetVelocity = Transform.Basis * input * currentSpeed;
        _velocity = _velocity.Lerp(targetVelocity, Acceleration * dt);
        Position += _velocity * dt;
    }

    private void CaptureMouse()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        _captured = true;
    }

    private void ReleaseMouse()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        _captured = false;
    }
}