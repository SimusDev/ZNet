using Godot;

namespace ArmaMovement.Components
{
	[GlobalClass]
	public partial class PlayerHead : Node3D
	{
		[Export] private Camera3D _camera;
		public Camera3D Camera => _camera;

		[Export] private Node3D _pitchPivot;

		[Export] private CharacterBody3D _characterBody;
		public CharacterBody3D CharacterBody => _characterBody;

		private float _mouseSensitivity = 0.0f;
		private const float SENS_NORMALIZE = 0.005f;

		public override void _Ready()
		{
			_mouseSensitivity = ProjectSettings.GetSetting("camera/mouse_sensitivity", 1.0f).As<float>();
			
		}
		
		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				float totalSens = _mouseSensitivity * SENS_NORMALIZE;

				RotateY(-mouseMotion.Relative.X * totalSens);
				_pitchPivot.RotateX(-mouseMotion.Relative.Y * totalSens);

				Vector3 pitchRot = _pitchPivot.Rotation;
				pitchRot.X = Mathf.Clamp(pitchRot.X, -Mathf.Pi / 2.2f, Mathf.Pi / 2.2f);
				_pitchPivot.Rotation = pitchRot;
			}

			if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
			{
				Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
					? Input.MouseModeEnum.Visible
					: Input.MouseModeEnum.Captured;
			}
		}
	}
}
