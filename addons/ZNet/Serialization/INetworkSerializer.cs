
namespace ZNet.Serialization
{
    public interface INetworkSerializer
    {
        void Serialize(object value, BinaryWriter writer);
        object Deserialize(BinaryReader reader);
    }
}
