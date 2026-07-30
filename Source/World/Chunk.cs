using Godot;
using System;

namespace ZNet.Source.World
{
    public partial class Chunk : Node3D
    {
        public const int ChunkSize = 16;

        public struct ChunkPos : IEquatable<ChunkPos>
        {
            public int X;
            public int Y;
            public int Z;

            public ChunkPos(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public ChunkPos(Vector3 worldPosition)
            {
                X = Mathf.FloorToInt(worldPosition.X / Chunk.ChunkSize);
                Y = Mathf.FloorToInt(worldPosition.Y / Chunk.ChunkSize);
                Z = Mathf.FloorToInt(worldPosition.Z / Chunk.ChunkSize);
            }

            public Vector3 ToWorldPosition()
            {
                return new Vector3(
                    X * Chunk.ChunkSize,
                    Y * Chunk.ChunkSize,
                    Z * Chunk.ChunkSize
                );
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(X, Y, Z);
            }

            public bool Equals(ChunkPos other)
            {
                return X == other.X && Y == other.Y && Z == other.Z;
            }

            public override bool Equals(object obj)
            {
                return obj is ChunkPos other && Equals(other);
            }

            public static bool operator ==(ChunkPos a, ChunkPos b) => a.Equals(b);
            public static bool operator !=(ChunkPos a, ChunkPos b) => !a.Equals(b);

            public override string ToString() => $"Chunk({X}, {Y}, {Z})";
        }

        public override void _Ready()
        {
            SetProcess(false);
            SetPhysicsProcess(false);
            SetProcessInput(false);
            SetProcessShortcutInput(false);
            SetProcessUnhandledInput(false);
            SetProcessUnhandledKeyInput(false);
        }


    }
}
