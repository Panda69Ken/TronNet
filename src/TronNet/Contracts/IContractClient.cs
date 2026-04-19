using System.Threading.Tasks;
using TronNet.Accounts;
using TronNet.Protocol;

namespace TronNet.Contracts
{
    public interface IContractClient
    {
        ContractProtocol Protocol { get; }

        Task<(Return Return, Transaction Transaction)> TransferAsync(string contractAddress, ITronAccount ownerAccount, string toAddress, decimal amount, string memo, long feeLimit);

        Task<decimal> BalanceOfAsync(string contractAddress, ITronAccount ownerAccount);

        Task<decimal> BalanceOfAsync(string contractAddress, string address);
    }
}
