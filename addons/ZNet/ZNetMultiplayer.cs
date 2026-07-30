using Godot;
using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using ZNet.Serialization;
using ZNet.Communication.Rpc;

namespace ZNet
{
	[GlobalClass]
	public partial class ZNetMultiplayer : Node
	{
		private static ZNetMultiplayer _instance;
		public static ZNetMultiplayer Instance => _instance;

		private NetManager _netManager;
		private EventBasedNetListener _listener;
		private bool _isServer = true;

		private Dictionary<int, NetPeer> _connections = new();
		private Dictionary<NetPeer, int> _connectionsId = new();

		private int _nextConnectionId = ServerId;

		[Signal] public delegate void DataReceivedEventHandler(int id, byte[] data);

		[Signal] public delegate void ServerPeerConnectedEventHandler(int id);
		[Signal] public delegate void ServerPeerDisconnectedEventHandler(int id);
		
		[Signal] public delegate void NetworkStatusChangedEventHandler(NetworkStatus status);

		public event OnNetworkStatisticsTick OnNetworkStatisticsTickEvent;
		public delegate void OnNetworkStatisticsTick(NetStatistics statistics);

		public const int ServerId = 1;

		public Dictionary<ulong, WeakReference<ZNet.Communication.SystemBase>> CommunicatorRegistry = new();
		
		public Dictionary<ulong, WeakReference> NetworkRefRegistry = new();

		private ulong _nextUniqueId = 0;

		public NetworkStatus _status = NetworkStatus.NotConnected;

		public NetworkStatus Status => _status;

		[ThreadStatic] private static BinaryWriter _writer = new BinaryWriter();
		[ThreadStatic] private static BinaryReader _reader = new BinaryReader();

		private void SetNetworkStatus(NetworkStatus status)
		{
			if (_status == status)
				return;

			_status = status;
			EmitSignal(SignalName.NetworkStatusChanged, (byte)status);
		}

		public static ulong HashStringDetermenistic(string str)
		{
			if (str == null)
				return 0;

			str += "ZNetMultiplayer";

			byte[] data = System.Text.Encoding.UTF8.GetBytes(str);

			ulong hash = 0x9e3779b97f4a7c15UL;

			for (int i = 0; i < data.Length; i++)
			{
				hash ^= data[i];
				hash *= 0xbf58476d1ce4e5b9UL;
				hash = (hash << 27) | (hash >> 37);
			}

			return hash;
		}

		public ulong GenerateId()
		{
			_nextUniqueId++;
			return _nextUniqueId;
		}

		public enum NetworkStatus : byte
		{
			NotConnected,
			Connecting,
			Disconnected,
			Ready
		}

		public enum PacketType : byte
		{
			ClientAuth = 0,
			MessageSystem,
			RpcSystem,
		}

		public bool IsServer => _isServer;
		public bool IsRunning { get; private set; }

		private int _uniqueId = 0;
		public int UniqueId => _uniqueId;

		public NetPeer ServerPeer { get; private set;  }

		public Dictionary<int, NetPeer> GetConnections()
		{
			return _connections;
		}

		public enum SendMode: byte
		{
			Unreliable = 4,
			ReliableUnordered = 0,
			Sequenced = 1,
			ReliableOrdered = 2,
			ReliableSequenced = 3
		}

		public override void _Ready()
		{
			if (Name == "ZNetSingleton")
				_instance = this;

			_listener = new EventBasedNetListener();
			_netManager = new NetManager(_listener);
			_netManager.AutoRecycle = true;

			_listener.ConnectionRequestEvent += OnConnectionRequest;
			_listener.PeerConnectedEvent += OnPeerConnected;
			_listener.PeerDisconnectedEvent += OnPeerDisconnected;
			_listener.NetworkReceiveEvent += OnNetworkReceive;

			_netManager.EnableStatistics = true;
		}

		public void StartServer(ushort port)
		{
			if (IsRunning)
				return;

			_isServer = true;

			if (_netManager.Start(port))
			{
				IsRunning = true;
				_uniqueId = ServerId;
				SetNetworkStatus(NetworkStatus.Ready);
			}
		}

		public void ConnectToServer(string host, ushort port)
		{
			if (_isServer && IsRunning)
			{
				GD.PushError("Cant Connect to Server while you running as Server");
				return;
			}

			_isServer = false;
			_netManager.Start();
			NetPeer netPeer = _netManager.Connect(host, port, "ZNetMultiplayerGodot");
			IsRunning = true;
			//GD.Print($"[ZNet] Подключение к {host}:{port}");
		}

		private void OnConnectionRequest(ConnectionRequest request)
		{
			request.AcceptIfKey("ZNetMultiplayerGodot");
		}

		private void OnPeerConnected(NetPeer peer)
		{
			if (_isServer)
			{
				_nextConnectionId++;
				int peerId = _nextConnectionId;
				_connections[peerId] = peer;
				_connectionsId[peer] = peerId;

				//GD.Print($"[ZNet] Клиент {peerId} подключился");
				EmitSignal(SignalName.ServerPeerConnected, peerId);

				_writer.Reset();
				_writer.WriteByte((byte)PacketType.ClientAuth);
				_writer.WriteInt(peerId);
				var span = _writer.GetSpan();
				SendTo(peerId, span, 0, SendMode.ReliableSequenced);
				//GD.Print("Sent: ", span.Length);
			}

			else
			{
				_connections[ServerId] = peer;
				_connectionsId[peer] = ServerId;

				ServerPeer = peer;
				
				//GD.Print($"[ZNet] Подключены к серверу");
				EmitSignal(SignalName.ServerPeerDisconnected, 1);
			}
		}

		private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			if (_isServer)
			{
				int peerId = _connectionsId[peer];
				_connectionsId.Remove(peer);
				_connections.Remove(peerId);
				//GD.Print($"[ZNet] Клиент {peerId} отключился: {disconnectInfo.Reason}");
				EmitSignal(SignalName.ServerPeerDisconnected, peerId);
			}
			else
			{
				Stop();

				SetNetworkStatus(NetworkStatus.Disconnected);
				SetNetworkStatus(NetworkStatus.NotConnected);

				//GD.Print($"[ZNet] Отключены от сервера: {disconnectInfo.Reason}");
				//EmitSignal(SignalName.ServerPeerDisconnected, 1);
			}
		}

		private void OnNetworkReceive(NetPeer peer, NetPacketReader netReader,
			byte channel, DeliveryMethod deliveryMethod)
		{
			int peerId = _connectionsId[peer];
			_reader.SetBuffer(netReader.GetRemainingBytes());
			_reader.Seek(0);

			PacketType type = (PacketType)_reader.ReadByte();

			switch (type)
			{
				case PacketType.ClientAuth:
					_uniqueId = _reader.ReadInt();
					SetNetworkStatus(NetworkStatus.Ready);
					break;

				case PacketType.RpcSystem:
					ulong networkId = _reader.ReadULong();
					
					if (!CommunicatorRegistry.TryGetValue(networkId, out var reference))
					{
						GD.PushError($"RpcSystem{networkId} was not found.");
						break;
					}

					if (!reference.TryGetTarget(out var target))
					{
						GD.PushError($"RpcSystem{networkId} was not found.");
						break;
					}

					(target as RpcSystem).OnRawDataReceived(peerId, _reader);

					break;
			}

		}

		public void SendTo(int peerId, ReadOnlySpan<byte> data, byte channel, SendMode sendMode)
		{
			if (_connections.TryGetValue(peerId, out var peer))
			{
				peer.Send(data, channel, (DeliveryMethod)sendMode);
			}

			else
			{
				GD.PushError($"Peer {peerId} was not found");
			}
		}

		public void SendToServer(ReadOnlySpan<byte> data, byte channel, SendMode sendMode)
		{
			if (ServerPeer == null)
			{
				GD.PushError("Server Peer was not found");
				return;
			}

			ServerPeer.Send(data, channel, (DeliveryMethod)sendMode);
		}

		public override void _Process(double delta)
		{
			if (!IsRunning) return;
			_netManager.PollEvents();
		}

		private int _statisticsStep = 0;
		private int _statisticsStepMax = 6;

		public override void _PhysicsProcess(double delta)
		{
			if (!IsRunning) return;

			_statisticsStep++;
			if (_statisticsStep >= _statisticsStepMax)
			{
				OnNetworkStatisticsTickEvent?.Invoke(_netManager.Statistics);
				_netManager.Statistics.Reset();
				_statisticsStep = 0;
			}
		}

		public void Stop()
		{
			if (!IsRunning)
				return;

			ServerPeer = null;
			_uniqueId = 0;
			IsRunning = false;
			_netManager.Stop();
		}

		public override void _ExitTree()
		{
			Stop();
		}


	}

}
