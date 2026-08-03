using Godot;
using ZNet.Serialization;

namespace ZNet.Prototyping.Components
{
    public partial class NetworkTransfrom2D : NetworkTransformBase
    {
        [Export] public Node2D Target;

        protected override void Serialize(BinaryWriter writer)
        {
            writer.WriteVector2(Target.Position);
            writer.WriteFloat(Target.Rotation);
            writer.WriteBool(SyncScale);
            if (SyncScale)
                writer.WriteVector2(Target.Scale);

        }

        protected override void Deserialize(BinaryReader reader)
        {
            Target.Position = reader.ReadVector2();
            Target.Rotation =  reader.ReadFloat();
            if (reader.ReadBool())
                Target.Scale = reader.ReadVector2();
        }
    }
}
