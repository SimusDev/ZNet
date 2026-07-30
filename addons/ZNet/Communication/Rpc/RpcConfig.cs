namespace ZNet.Communication.Rpc;
public struct RpcConfig
{
    public RpcType RpcType;
    public SendMode SendMode;
    public byte Channel;
    public bool RunLocally;

    public static RpcConfig Default => new RpcConfig
    {
        RpcType = RpcType.ToObserver,
        SendMode = SendMode.ReliableOrdered,
        Channel = 0,
    };

    public RpcConfig(RpcType rpcType, byte channel, SendMode mode, bool runLocally)
    {
        RpcType = rpcType;
        Channel = channel;
        SendMode = mode;
        RunLocally = runLocally;
    }

}