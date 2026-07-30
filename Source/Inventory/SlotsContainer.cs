using Godot;
using ZNet.Communication.Rpc;
using ZNet.Serialization;

namespace ZNet.Source.Inventory
{
    [GlobalClass]
    public partial class SlotsContainer : Resource, INetworkSerializable
    {

        [Export] private Godot.Collections.Array<Slot> _slots = new();

        public void NetworkDeserialize(BinaryReader reader)
        {
            int slotCount = reader.ReadVarInt();
            for (int i = 0; i < slotCount; i++)
            {
                _slots.Add(reader.ReadSerializable<Slot>());
            }
        }

        public void NetworkSerialize(BinaryWriter writer)
        {
            writer.WriteVarInt(_slots.Count);
            foreach (Slot slot in _slots)
                writer.WriteSerializable(slot);
        }
    }
}
