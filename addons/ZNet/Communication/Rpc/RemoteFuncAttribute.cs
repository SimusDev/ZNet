using Godot;
using System;

namespace ZNet.Communication.Rpc;

[AttributeUsage(AttributeTargets.Method)]
public class RemoteFunc : Attribute
{
    public byte Channel { get; init; }
    public RpcType Type { get; set; }
    public SendMode SendMode { get; init; } = SendMode.ReliableOrdered;

    public bool RunLocally { get; set; } = false;

    public RemoteFunc(RpcType type = RpcType.ToObserver)
    {
         Type = type;
    }
}

public enum RpcType : byte
{
    AuthToServer,
    ToServer,     
    ToObserver,
}

public enum SendMode : byte
{
    Unreliable = 4,
    ReliableUnordered = 0,
    Sequenced = 1,
    ReliableOrdered = 2,
    ReliableSequenced = 3
}



