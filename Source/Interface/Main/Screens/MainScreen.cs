using Godot;

namespace ZNet.Source.Interface.Main.Screens
{
	public partial class MainScreen : Control
	{
		[Export] private Godot.Collections.Array<Button> _buttons = new();

		public override void _Ready()
		{
			foreach (var button in _buttons)
			{
				button.Pressed += () => OnButtonPressed(button);
			}
		}

		private void OnButtonPressed(Button button)
		{
			switch (button.Name)
			{
				case "Server":
					ZNetMultiplayer.Instance.StartServer(8080);
					break;
				case "Quit":
					GetTree().Quit();
					break;
				
			}
		}

	}
}
