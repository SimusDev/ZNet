using System;
using System.Collections.Generic;

namespace ZNet.Serialization
{
    public static class ObjectSerializer
    {
        private static Dictionary<Type, ushort> _typeToId = new();
        private static Dictionary<ushort, Type> _idToType = new();
        private static ushort _nextTypeId = 1;

        private const ushort INTERFACE_ID_START = 0x8000;
        private static ushort _nextInterfaceId = INTERFACE_ID_START;

        private static Dictionary<Type, Action<BinaryWriter, object>> _writers = new();
        private static Dictionary<Type, Func<BinaryReader, object>> _readers = new();

        static ObjectSerializer()
        {
            RegisterInterface<INetworkSerializable>(
                writer: (w, v) => w.WriteSerializable(v),
                reader: r => r.ReadSerializable()
            );

            Register<string>(
               writer: (w, v) => w.WriteString(v),
               reader: r => r.ReadString()
             );

            Register<byte[]>(
                writer: (w, v) => w.WriteBytesDynamic(v),
                reader: r => r.ReadBytesDynamic()
            );

            Register<byte>(
                writer: (w, v) => w.WriteByte(v),
                reader: r => r.ReadByte()
            );

            Register<sbyte>(
                writer: (w, v) => w.WriteSByte(v),
                reader: r => r.ReadSByte()
            );

            Register<short>(
                writer: (w, v) => w.WriteShort(v),
                reader: r => r.ReadShort()
            );

            Register<ushort>(
                writer: (w, v) => w.WriteUShort(v),
                reader: r => r.ReadUShort()
            );

            Register<int>(
                writer: (w, v) => w.WriteInt(v),
                reader: r => r.ReadInt()
            );

            Register<uint>(
                writer: (w, v) => w.WriteUInt(v),
                reader: r => r.ReadUInt()
            );

            Register<long>(
                writer: (w, v) => w.WriteLong(v),
                reader: r => r.ReadLong()
            );

            Register<ulong>(
               writer: (w, v) => w.WriteULong(v),
               reader: r => r.ReadULong()
            );

            Register<float>(
               writer: (w, v) => w.WriteFloat(v),
               reader: r => r.ReadFloat()
            );

            Register<double>(
               writer: (w, v) => w.WriteDouble(v),
               reader: r => r.ReadDouble()
            );

            Register<Godot.Vector2>(
              writer: (w, v) => w.WriteVector2(v),
              reader: r => r.ReadVector2()
            );

            Register<Godot.Vector3>(
              writer: (w, v) => w.WriteVector3(v),
              reader: r => r.ReadVector3()
            );




        }

        public static void Register<T>(Action<BinaryWriter, T> writer, Func<BinaryReader, T> reader)
        {
            var type = typeof(T);
            if (_writers.ContainsKey(type)) return;

            ushort typeId = _nextTypeId++;
            if (typeId >= INTERFACE_ID_START)
                throw new Exception("Too many types registered!");

            _typeToId[type] = typeId;
            _idToType[typeId] = type;
            _writers[type] = (w, obj) => writer(w, (T)obj);
            _readers[type] = r => reader(r);
        }

        public static void RegisterInterface<T>(Action<BinaryWriter, T> writer, Func<BinaryReader, T> reader) where T : class
        {
            var type = typeof(T);
            if (!type.IsInterface)
                throw new Exception($"{type} is not an interface!");
            if (_writers.ContainsKey(type)) return;

            ushort interfaceId = _nextInterfaceId++;
            if (interfaceId == 0)
                throw new Exception("Too many interfaces registered!");

            _typeToId[type] = interfaceId;
            _idToType[interfaceId] = type;
            _writers[type] = (w, obj) => writer(w, (T)obj);
            _readers[type] = r => reader(r);
        }

        public static void Write(BinaryWriter writer, object value)
        {
            if (value == null)
            {
                writer.WriteByte(0);
                return;
            }

            var type = value.GetType();

            if (!_typeToId.TryGetValue(type, out ushort typeId))
                throw new Exception($"No serializer for type: {type}");

            writer.WriteByte(1);
            writer.WriteUShort(typeId);
            _writers[type](writer, value);
        }

        public static object Read(BinaryReader reader)
        {
            byte marker = reader.ReadByte();
            if (marker == 0) return null;

            ushort typeId = reader.ReadUShort();

            if (!_idToType.TryGetValue(typeId, out Type type))
                throw new Exception($"Type ID not found: {typeId}");

            if (!_readers.TryGetValue(type, out var readerFunc))
                throw new Exception($"No deserializer for type ID: {typeId}");

            return readerFunc(reader);
        }
    }
}
