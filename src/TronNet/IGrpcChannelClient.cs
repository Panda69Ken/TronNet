namespace TronNet
{
    public interface IGrpcChannelClient
    {
        
        Grpc.Core.Channel GetProtocol();
        Grpc.Core.Channel GetSolidityProtocol();
    }
}
