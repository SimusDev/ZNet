using Godot;
using ZNet.Serialization;

namespace ZNet.Source.Inventory
{
    [GlobalClass]
    public partial class Slot : Resource, INetworkSerializable
    {
        [Export] private ItemStack _itemStack;

        public void NetworkDeserialize(BinaryReader reader)
        {
            _itemStack = reader.ReadSerializable<ItemStack>();
        }

        public void NetworkSerialize(BinaryWriter writer)
        {
            writer.WriteSerializable(_itemStack);
        }
    }
}
