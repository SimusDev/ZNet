using Godot;
using ZNet.Communication.Rpc;

namespace ZNet.Source.Inventory
{
	public partial class InventoryComponent : Node
	{
		private RpcSystem _rpc = new();

		[Export] private SlotsContainer _container;

		public override void _Ready()
		{
			_rpc.RegisterByNode(this);
			_rpc.BindDelegates(this);

			_rpc.Invoke(Send);
		}

		[RemoteFunc(RpcType.ToServer, Channel = (byte)Network.Channels.Inventory)]
		private void Send()
		{
			_rpc.InvokeId(_rpc.RemoteSenderId, Receive, _container);
		}

		[RemoteFunc(Channel = (byte)Network.Channels.Inventory)]
		private void Receive(SlotsContainer container)
		{
			_container = container;
		}



	}
}
