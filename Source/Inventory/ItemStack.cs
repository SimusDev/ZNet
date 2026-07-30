using Godot;
using ZNet.Serialization;

namespace ZNet.Source.Inventory
{
    [GlobalClass]
    public partial class ItemStack : Resource, INetworkSerializable
    {
        [Export] public RGameResource Resource;

        public void NetworkDeserialize(BinaryReader reader)
        {
            Resource = reader.ReadResourceOrNull<RGameResource>();
        }

        public void NetworkSerialize(BinaryWriter writer)
        {
            writer.WriteResourceOrNull(Resource);
        }
    }
}
