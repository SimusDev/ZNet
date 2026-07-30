
namespace ZNet.Serialization
{
    public struct NetworkRef
    {
        private ZNetMultiplayer _multiplayer;

        public NetworkRef(ZNetMultiplayer multiplayer, Godot.Node node)
        {
            _multiplayer = multiplayer;
        }

        public NetworkRef(Godot.Node node)
        {

        }

        public object Get()
        {
            return null;
        }

        public object Get<T>()
        {
            return (T)Get();
        }

    }


}
