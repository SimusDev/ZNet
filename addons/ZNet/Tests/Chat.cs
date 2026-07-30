using Godot;
using System;
using ZNet.Serialization;
using ZNet.Communication.Rpc;
using Godot.Collections;

namespace ZNet.Tests.Chat
{
	public partial class Chat : Node
	{
		[Export] private CanvasLayer _lobbyLayer;
		[Export] private CanvasLayer _chatLayer;

		[Export] private LineEdit _usernameLine;
		[Export] private LineEdit _addressLine;
		[Export] private LineEdit _portLine;

		[Export] private Button _createServerButton;
		[Export] private Button _createClientButton;

		[Export] private RichTextLabel _chatWindow;
		[Export] private LineEdit _messageEnter;

		[Export] private Timer _timer;

		private RpcSystem _rpcSystem = new();

		private Dictionary<int, string> _userNames = new();

		public override void _Ready()
		{
			_timer.Timeout += OnTimerTimeout;

			_lobbyLayer.Show();
			_chatLayer.Hide();

			_createServerButton.Pressed += CreateServerPressed;
			_createClientButton.Pressed += CreateClientPressed;

			_messageEnter.TextSubmitted += MessageSubmitted;

			_rpcSystem.RegisterByNode(this);
			_rpcSystem.BindDelegates(this);

			ZNetMultiplayer.Instance.NetworkStatusChanged += OnNetworkStatusChanged;
			ZNetMultiplayer.Instance.ServerPeerDisconnected += OnServerPeerDisconnected;
		}

		private void OnTimerTimeout()
		{

		}

		[RemoteFunc(SendMode = SendMode.Unreliable)]
		private void PerformanceTestRpc()
		{

		}

		private void OnServerPeerDisconnected(int id)
		{
			if (_userNames.TryGetValue(id, out string name))
			{
				_rpcSystem.Invoke(MessageFromClientReceived, $"{name} disconnected!");
				_userNames.Remove(id);
			}
		}

		private void OnNetworkStatusChanged(ZNetMultiplayer.NetworkStatus status)
		{
			_timer.Stop();

			switch (status)
			{
				case ZNetMultiplayer.NetworkStatus.Disconnected:
					_lobbyLayer.Show();
					_chatLayer.Hide();
					break;
				case ZNetMultiplayer.NetworkStatus.Ready:
					_lobbyLayer.Hide();
					_chatLayer.Show();
					GD.Print($"Network ready with UniqueId {ZNetMultiplayer.Instance.UniqueId}");

					_rpcSystem.Invoke(ReceiveUserConnect, _usernameLine.Text);

					if (ZNetMultiplayer.Instance.IsServer)
					{
						_timer.Start();
					}

					break;
				
			}
		}

		private void CreateServerPressed()
		{
			ZNetMultiplayer.Instance.StartServer((ushort)_portLine.Text.ToInt());
		}

		private void CreateClientPressed()
		{
			ZNetMultiplayer.Instance.ConnectToServer(_addressLine.Text, (ushort)_portLine.Text.ToInt());
		}

		[RemoteFunc(RpcType.ToServer, RunLocally = true)]
		private void ReceiveUserConnect(string name)
		{
			_userNames[_rpcSystem.RemoteSenderId] = name;
			_rpcSystem.Invoke(ReceiveMessageFromServer, $"{name} connected!");
		}

		private void MessageSubmitted(string newText)
		{
			_messageEnter.Text = "";
			_rpcSystem.Invoke(MessageFromClientReceived, newText);
		}

		[RemoteFunc(RpcType.ToServer, RunLocally = true)]
		private void MessageFromClientReceived(string message)
		{
			string userName = _rpcSystem.RemoteSenderId.ToString();
			if (_userNames.TryGetValue(_rpcSystem.RemoteSenderId, out var user))
			{
				userName = user;
			}

			string broadcastMessage = $"{userName}: {message}";
			_rpcSystem.Invoke(ReceiveMessageFromServer, broadcastMessage);
		}

		[RemoteFunc(RpcType.ToObserver, RunLocally = true)]
		private void ReceiveMessageFromServer(string message)
		{
			_chatWindow.Text += message + "\n";
		}

	}

}
