namespace TronNet.Contracts
{
    public interface IContractClientFactory
    {
        IContractClient CreateClient(ContractProtocol protocol);
    }
}
