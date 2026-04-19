using System.Threading.Tasks;
using TronNet.Protocol;

namespace TronNet.Accounts
{
    public interface ITronAccountWallet
    {
        Task<(long BlockHeight, Transaction Transaction)> TransferAsync(ITronAccount ownerAccount, string toAddress, decimal amount);

        Task<decimal> BalanceOfAsync(ITronAccount ownerAccount);

        Task<decimal> BalanceOfAsync(string address);
    }
}
