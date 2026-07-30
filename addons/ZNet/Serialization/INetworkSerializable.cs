
namespace ZNet.Serialization
{
    public interface INetworkSerializable
    {
        void NetworkSerialize(BinaryWriter writer);
        void NetworkDeserialize(BinaryReader reader);
    }
}
