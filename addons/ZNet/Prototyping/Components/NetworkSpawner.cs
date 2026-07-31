using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZNet.Communication.Rpc;
using ZNet.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ZNet.Prototyping.Components
{
	public partial class NetworkSpawner : Node
	{
		[Export] public Node RootNode;

		protected ZNetMultiplayer ZNetMultiplayer;

		private RpcSystem _rpcSystem = new();

		private BinaryWriter _writer = new();
		private BinaryReader _reader = new();

		private List<Node> _nodesToSerialize = new();

		public override void _Ready()
		{
			if (ZNetMultiplayer == null)
				ZNetMultiplayer = ZNetMultiplayer.Instance;

			if (RootNode == null)
			{
				GD.PushError($"{GetPath()}: RootNode is null!");
				return;
			}

			_rpcSystem.SetMultiplayer(ZNetMultiplayer);
			_rpcSystem.RegisterByNode(this);
			_rpcSystem.BindDelegates(this);

			RootNode.ChildEnteredTree += OnChildEnteredTree;
			RootNode.ChildExitingTree += OnChildExitingTree;

			ZNetMultiplayer.NetworkStatusChanged += OnNetworkStatusChanged;
			OnNetworkStatusChanged(ZNetMultiplayer.Status);
			
		}


		private void OnChildExitingTree(Node node)
		{
			if (!ZNetMultiplayer.IsServer)
				return;

			if (CanSerializeNode(node))
				_rpcSystem.Invoke(UnpackAndDespawn, node.Name);

		}

		private async void OnChildEnteredTree(Node node)
		{
			if (!ZNetMultiplayer.IsServer)
				return;
			
			if (!CanSerializeNode(node))
				return;

			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

			_writer.Reset();
			_writer.WriteVarInt(1);
			SerializeNode(node, _writer);
			_rpcSystem.Invoke(UnpackAndSpawn, _writer.ToArray());

		}

		private void SerializeNode(Node node, BinaryWriter writer)
		{
			string validatedName = node.Name.ToString().ValidateNodeName();
			node.Name = validatedName;
			writer.WriteResource(GD.Load<PackedScene>(node.SceneFilePath));
			writer.WriteString(validatedName);
			writer.WriteVarInt(node.GetMultiplayerAuthority());
			if (node is Node2D || node is Node3D)
			{
				writer.WriteByte(1);
				writer.WriteBytesDynamic(GD.VarToBytes(node.Get("transform")));
			}
			else
			{
				writer.WriteByte(0);
			}


		}

		protected virtual void OnNetworkStatusChanged(ZNetMultiplayer.NetworkStatus status)
		{
			if (status == ZNetMultiplayer.NetworkStatus.Ready)
			{
				Synchronize();

			}

			if (status == ZNetMultiplayer.NetworkStatus.Disconnected)
			{
				ClearNodes();
			}
		}

		private async Task ClearNodes()
		{
			if (RootNode == null)
			{
				GD.PushError("RootNode is null!");
				return;
			}

			foreach (var child in RootNode.GetChildren())
			{
				child.QueueFree();
				await ToSignal(child, Node.SignalName.TreeExited);
			}

		}

		private async Task Synchronize()
		{
			if (ZNetMultiplayer.IsServer)
				return;

			await ClearNodes();

			_rpcSystem.Invoke(ServerAcceptClient);
		}

		private bool CanSerializeNode(Node node)
		{
			return !node.SceneFilePath.Equals(string.Empty);
		}

		[RemoteFunc(RpcType.ToServer)]
		protected void ServerAcceptClient()
		{
			if (RootNode == null)
				return;

			_nodesToSerialize.Clear();

			foreach (var child in RootNode.GetChildren())
			{
				if (!CanSerializeNode(child))
					continue;

				_nodesToSerialize.Add(child);
			}

			if (_nodesToSerialize.Count < 1)
				return;

			_writer.Reset();
			_writer.WriteVarInt(_nodesToSerialize.Count);

			foreach (var node in _nodesToSerialize)
			{
				SerializeNode(node, _writer);
			}

			_rpcSystem.InvokeId(_rpcSystem.RemoteSenderId, UnpackAndSpawn, _writer.ToArray());
		}

		[RemoteFunc(RpcType.ToObserver)]
		protected void UnpackAndSpawn(byte[] data)
		{
			if (RootNode == null)
				return;

			_reader.SetBuffer(data);
			_reader.Seek(0);

			int nodeCount = _reader.ReadVarInt();

			for (int i = 0; i < nodeCount; i++)
			{
				PackedScene scene = _reader.ReadResource<PackedScene>();
				Node node = scene.Instantiate();
				string nodeName = _reader.ReadString();
				int auth = _reader.ReadVarInt();
				node.Name = nodeName;
				node.SetMultiplayerAuthority(auth);

				byte type = _reader.ReadByte();
				if (type == 1)
				{
					Variant transform = GD.BytesToVar(_reader.ReadBytesDynamic());
					node.Set("transform", transform);
				}

				RootNode.AddChild(node);
			}



		}

		[RemoteFunc(RpcType.ToObserver)]
		protected void UnpackAndDespawn(string path)
		{
			if (RootNode == null)
				return;

			Node node = RootNode.GetNode(path);
			node.QueueFree();
		}



	}


}
