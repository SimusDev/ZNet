using Godot;
using System;
using System.Text;

namespace ZNet.Serialization;

public unsafe class BinaryReader : IDisposable
{
    private byte[] _buffer;
    private int _position;
    private int _length;
    
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: освободить управляемое состояние (управляемые объекты)
            }

            // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
            // TODO: установить значение NULL для больших полей
            disposedValue = true;
        }
    }

    // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
    // ~BinaryWriter()
    // {
    //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }


    public BinaryReader()
    {
        _buffer = Array.Empty<byte>();
        _position = 0;
        _length = 0;
    }

    public BinaryReader(byte[] data)
    {
        _buffer = data;
        _position = 0;
        _length = data.Length;
    }

    public BinaryReader(ArraySegment<byte> segment)
    {
        _buffer = segment.Array;
        _position = segment.Offset;
        _length = segment.Offset + segment.Count;
    }

    public BinaryReader(byte[] data, int offset, int length)
    {
        _buffer = data;
        _position = offset;
        _length = offset + length;
    }

    public void SetBuffer(byte[] data)
    {
        _buffer = data;
        _position = 0;
        _length = data.Length;
    }

    public void SetBuffer(byte[] data, int offset, int length)
    {
        _buffer = data;
        _position = offset;
        _length = offset + length;
    }

    public void SetBuffer(ArraySegment<byte> segment)
    {
        _buffer = segment.Array;
        _position = segment.Offset;
        _length = segment.Offset + segment.Count;
    }

    public byte ReadByte()
    {
        CheckBounds(1);
        return _buffer[_position++];
    }

    public sbyte ReadSByte()
    {
        return (sbyte)ReadByte();
    }

    public bool ReadBool()
    {
        CheckBounds(1);
        return _buffer[_position++] != 0;
    }

    public ushort ReadUShort()
    {
        CheckBounds(2);
        fixed (byte* ptr = &_buffer[_position])
        {
            _position += 2;
            return *(ushort*)ptr;
        }
    }

    public short ReadShort()
    {
        CheckBounds(2);
        fixed (byte* ptr = &_buffer[_position])
        {
            _position += 2;
            return *(short*)ptr;
        }
    }

    public int ReadInt()
    {
        CheckBounds(4);
        fixed (byte* ptr = &_buffer[_position])
        {
            _position += 4;
            return *(int*)ptr;
        }
    }

    public uint ReadUInt()
    {
        CheckBounds(4);
        fixed (byte* ptr = &_buffer[_position])
        {
            _position += 4;
            return *(uint*)ptr;
        }
    }

    public long ReadLong()
    {
        CheckBounds(8);
        fixed (byte* ptr = &_buffer[_position])
        {
            _position += 8;
            return *(long*)ptr;
        }
    }

    public ulong ReadULong()
    {
        CheckBounds(8);
        fixed (byte* ptr = &_buffer[_position])
        {
            _position += 8;
            return *(ulong*)ptr;
        }
    }

    public float ReadFloat()
    {
        CheckBounds(4);
        fixed (byte* ptr = &_buffer[_position])
        {
            _position += 4;
            return *(float*)ptr;
        }
    }

    public double ReadDouble()
    {
        CheckBounds(8);
        fixed (byte* ptr = &_buffer[_position])
        {
            _position += 8;
            return *(double*)ptr;
        }
    }

    public Godot.Vector2 ReadVector2()
    {
        CheckBounds(8);
        fixed (byte* ptr = &_buffer[_position])
        {
            Godot.Vector2 v;
            v.X = *(float*)ptr;
            v.Y = *(float*)(ptr + 4);
            _position += 8;
            return v;
        }
    }

    public Godot.Vector3 ReadVector3()
    {
        CheckBounds(12);
        fixed (byte* ptr = &_buffer[_position])
        {
            _position += 12;
            return *(Godot.Vector3*)ptr;
        }
    }

    public int ReadVarInt()
    {
        int result = 0;
        int shift = 0;

        while (true)
        {
            CheckBounds(1);
            byte b = _buffer[_position++];
            result |= (b & 0x7F) << shift;
            shift += 7;
            if ((b & 0x80) == 0) break;
        }

        return result;
    }

    public long ReadVarLong()
    {
        long result = 0;
        int shift = 0;

        while (true)
        {
            CheckBounds(1);
            byte b = _buffer[_position++];
            result |= (long)(b & 0x7F) << shift;
            shift += 7;
            if ((b & 0x80) == 0) break;
        }

        return result;
    }

    public byte[] ReadBytes(int count)
    {
        CheckBounds(count);
        byte[] result = new byte[count];
        Buffer.BlockCopy(_buffer, _position, result, 0, count);
        _position += count;
        return result;
    }

    public void ReadBytes(Span<byte> destination)
    {
        CheckBounds(destination.Length);
        _buffer.AsSpan(_position, destination.Length).CopyTo(destination);
        _position += destination.Length;
    }

    public ReadOnlySpan<byte> ReadBytesSpan(int count)
    {
        CheckBounds(count);
        var span = _buffer.AsSpan(_position, count);
        _position += count;
        return span;
    }

    public byte[] ReadBytesDynamic()
    {
        return ReadBytes(ReadVarInt());
    }

    public string ReadString()
    {
        int length = ReadVarInt();
        if (length < 0) return null;
        if (length == 0) return string.Empty;

        CheckBounds(length);
        string result = Encoding.UTF8.GetString(_buffer, _position, length);
        _position += length;
        return result;
    }

    private void CheckBounds(int needed)
    {
        if (_position + needed > _length)
            throw new InvalidOperationException($"Not enough data: need {needed}, have {_length - _position}");
    }

    public void Skip(int bytes)
    {
        _position += bytes;
    }

    public void Seek(int position)
    {
        _position = position;
    }

    public int Position => _position;
    public int Length => _length;
    public int Available => _length - _position;

    // Godot

    public T ReadSerializable<T>() where T : INetworkSerializable
    {
        return (T)ReadSerializable();
    }

    public INetworkSerializable ReadSerializable()
    {
        var type = Type.GetType(ReadString());
        var result = (INetworkSerializable)Activator.CreateInstance(type);
        result.NetworkDeserialize(this);
        return result;
    }

    public T ReadResource<T>() where T : Godot.Resource
    {
        long hashId = ReadLong();
        return Godot.ResourceLoader.Load<T>(Godot.ResourceUid.GetIdPath(hashId));
    }

    public T ReadResourceOrNull<T>() where T : Godot.Resource
    {
        if (ReadBool())
            return null;
        return ReadResource<T>();
    }

}