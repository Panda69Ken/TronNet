using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TronNet.Crypto;
using TronNet.Protocol;

namespace TronNet.Accounts
{
    class TronAccountWallet(ILogger<TronAccountWallet> logger, IWalletClient walletClient, ITransactionClient transactionClient) : ITronAccountWallet
    {
        private readonly ILogger<TronAccountWallet> _logger = logger;
        private readonly IWalletClient _walletClient = walletClient;
        private readonly ITransactionClient _transactionClient = transactionClient;

        public async Task<(long BlockHeight, Transaction Transaction)> TransferAsync(ITronAccount ownerAccount, string toAddress, decimal amount)
        {
            var ownerAddressBytes = Base58Encoder.DecodeFromBase58Check(ownerAccount.Address);
            var callerAddressBytes = Base58Encoder.DecodeFromBase58Check(toAddress);

            var wallet = _walletClient.GetProtocol();

            var transferContract = new TransferContract
            {
                OwnerAddress = ByteString.CopyFrom(ownerAddressBytes),
                ToAddress = ByteString.CopyFrom(callerAddressBytes),
                Amount = TronUnit.TRXToSun(amount),    // 1 TRX, api only receive trx in Sun, and 1 trx = 1000000 Sun
            };

            var transaction = new Transaction();

            var contract = new Transaction.Types.Contract();

            try
            {
                contract.Parameter = Google.Protobuf.WellKnownTypes.Any.Pack(transferContract);
            }
            catch (Exception)
            {
                _logger.LogWarning($"创建TRX交易订单失败,code:{Return.Types.response_code.OtherError}");
                return (0, null);
            }
            var newestBlock = await wallet.GetNowBlock2Async(new EmptyMessage(), _walletClient.GetHeaders());

            contract.Type = Transaction.Types.Contract.Types.ContractType.TransferContract;
            transaction.RawData = new Transaction.Types.raw();
            transaction.RawData.Contract.Add(contract);
            transaction.RawData.Timestamp = DateTime.Now.Ticks;
            transaction.RawData.Expiration = newestBlock.BlockHeader.RawData.Timestamp + 10 * 60 * 60 * 1000;
            var blockHeight = newestBlock.BlockHeader.RawData.Number;
            var blockHash = Sha256Sm3Hash.Of(newestBlock.BlockHeader.RawData.ToByteArray()).GetBytes();

            var bb = ByteBuffer.Allocate(8);
            bb.PutLong(blockHeight);

            var refBlockNum = bb.ToArray();

            transaction.RawData.RefBlockHash = ByteString.CopyFrom(blockHash.SubArray(8, 8));
            transaction.RawData.RefBlockBytes = ByteString.CopyFrom(refBlockNum.SubArray(6, 2));

            var transactionExtension = new TransactionExtention
            {
                Transaction = transaction,
                Txid = ByteString.CopyFromUtf8(transaction.GetTxid()),
                Result = new Return { Result = true, Code = Return.Types.response_code.Success },
            };

            if (transactionExtension.Result.Result == false)
            {
                _logger.LogWarning($"创建TRX交易订单失败,code:{transactionExtension.Result.Code}");
                return (0, null);
            }

            var transactionNew = transactionExtension.Transaction;

            var transSign = _transactionClient.GetTransactionSign(transactionNew, ownerAccount.PrivateKey);
            if (transSign == null)
            {
                _logger.LogWarning($"创建TRX交易签名失败,transSign返回null");
                return (0, null);
            }

            var result = await wallet.BroadcastTransactionAsync(transSign, _walletClient.GetHeaders());
            if (result == null || result.Code != Return.Types.response_code.Success)
            {
                _logger.LogWarning($"发起TRX交易失败,code:{result.Code},msg:{result.Message.ToStringUtf8()}");
                return (0, null);
            }

            return (blockHeight, transSign);
        }


        public async Task<decimal> BalanceOfAsync(ITronAccount ownerAccount)
        {
            try
            {
                var addressBytes = Base58Encoder.DecodeFromBase58Check(ownerAccount.Address);
                var account = await _walletClient.GetProtocol().GetAccountAsync(new Account
                {
                    Address = ByteString.CopyFrom(addressBytes),
                    //Type = AccountType.Contract,
                }, headers: _walletClient.GetHeaders());

                if (account == null) return 0;

                // 1 TRX, api only receive trx in Sun, and 1 trx = 1000000 Sun
                return TronUnit.SunToTRX(account.Balance); //account.Balance / 1000000m;
            }
            catch (Exception ex)
            {
                _logger.LogError($"获取地址TRX异常,Address:{ownerAccount.Address},error:{ex.Message}");
                return 0;
            }
        }

        public async Task<decimal> BalanceOfAsync(string address)
        {
            try
            {
                var addressBytes = Base58Encoder.DecodeFromBase58Check(address);
                var account = await _walletClient.GetProtocol().GetAccountAsync(new Account
                {
                    Address = ByteString.CopyFrom(addressBytes),
                    //Type = AccountType.Contract,
                }, headers: _walletClient.GetHeaders());

                if (account == null) return 0;

                // 1 TRX, api only receive trx in Sun, and 1 trx = 1000000 Sun
                return TronUnit.SunToTRX(account.Balance); //account.Balance / 1000000m;
            }
            catch (Exception ex)
            {
                _logger.LogError($"获取地址TRX异常,Address:{address},error:{ex.Message}");
                return 0;
            }
        }

    }
}
