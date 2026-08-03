using Godot;
using ZNet.Serialization;

namespace ZNet.Prototyping.Components
{
	public partial class NetworkTransfrom3D : NetworkTransformBase
	{
		[Export] public Node3D Target;

		private Vector3 _lastSyncedPosition;
		private Vector3 _lastSyncedRotation;
		private Vector3 _lastSyncedScale;

		protected override void InterpolateProcess(double delta)
		{
			Target.Position = Target.Position.Lerp(_lastSyncedPosition, InterpolateScale * (float)delta);

			Quaternion currentRot = Target.Quaternion;
			Quaternion targetRot = Quaternion.FromEuler(_lastSyncedRotation);
			Target.Quaternion = currentRot.Slerp(targetRot, InterpolateScale * (float)delta);

			if (SyncScale)
				Target.Scale = Target.Scale.Lerp(_lastSyncedScale, InterpolateScale * (float)delta);
		}

		protected override void Serialize(BinaryWriter writer)
		{
			writer.WriteVector3(Target.Position);
			writer.WriteVector3(Target.Rotation);
			writer.WriteBool(SyncScale);
			if (SyncScale)
				writer.WriteVector3(Target.Scale);

		}

		protected override void Deserialize(BinaryReader reader)
		{

			_lastSyncedPosition = reader.ReadVector3();
			_lastSyncedRotation = reader.ReadVector3();
			if (reader.ReadBool())
				_lastSyncedScale = reader.ReadVector3();

			if (!Interpolate)
			{
				Target.Position = _lastSyncedPosition;
				Target.Rotation = _lastSyncedRotation;
				Target.Scale = _lastSyncedScale;
			}
		}
	}
}
