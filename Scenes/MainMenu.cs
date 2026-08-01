using Godot;
using Godot.Collections;
using System;

namespace ZNet.Scenes
{
	public partial class MainMenu : Control
	{
		public override void _Ready()
		{
			ZNetMultiplayer.Instance.NetworkStatusChanged += OnNetworkStatusChanged;

		}
		private void OnNetworkStatusChanged(ZNetMultiplayer.NetworkStatus status)
		{
			if (status == ZNetMultiplayer.NetworkStatus.Ready)
			{
				Startup.LoadOrReloadGame();
			}
		}
	}
}
