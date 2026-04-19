using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TronNet.ABI;
using TronNet.ABI.FunctionEncoding;
using TronNet.ABI.Model;
using TronNet.Accounts;
using TronNet.Contracts;
using TronNet.Crypto;
using TronNet.Protocol;
using Xunit;

namespace TronNet.Test
{
    public class TransactionTest
    {
        private readonly TronTestRecord _record;
        private readonly Wallet.WalletClient _wallet;

        public TransactionTest()
        {
            _record = TronTestServiceExtension.GetTestRecord();
            _wallet = _record.TronClient.GetWallet().GetProtocol();
        }

        [Fact]
        public async Task TestTransferAsync()
        {
            var transactionClient = _record.ServiceProvider.GetService<ITransactionClient>();
            var privateKey = "8e812436a0e3323166e1f0e8ba79e19e217b2c4a53c970d4cca0cfb1078979df";
            var tronKey = new TronECKey(privateKey, _record.Options.Value.Network);
            var from = tronKey.GetPublicAddress();
            var to = "TGehVcNhud84JDCGrNHKVz9jEAVKUpbuiv";
            var amount = 1_000_000L; // 1 TRX, api only receive trx in Sun, and 1 trx = 1000000 Sun

            var fromAddress = Base58Encoder.DecodeFromBase58Check(from);
            var toAddress = Base58Encoder.DecodeFromBase58Check(to);

            var block = await _wallet.GetNowBlock2Async(new EmptyMessage());

            var transaction = await transactionClient.CreateTransactionAsync(from, to, amount);

            Assert.True(transaction.Result.Result);

            var transactionSigned = transactionClient.GetTransactionSign(transaction.Transaction, privateKey);

            var result = await transactionClient.BroadcastTransactionAsync(transactionSigned);

            Assert.True(result.Result);
        }

        [Fact]
        public async Task TestSignAsync()
        {
            var transactionClient = _record.ServiceProvider.GetService<ITransactionClient>();
            var privateKey = "D95611A9AF2A2A45359106222ED1AFED48853D9A44DEFF8DC7913F5CBA727366";
            var ecKey = new TronECKey(privateKey, _record.Options.Value.Network);
            var from = ecKey.GetPublicAddress();
            var to = "TGehVcNhud84JDCGrNHKVz9jEAVKUpbuiv";
            var amount = 100_000_000L;
            var result = await transactionClient.CreateTransactionAsync(from, to, amount);

            Assert.True(result.Result.Result);

            var transactionSigned = transactionClient.GetTransactionSign(result.Transaction, privateKey);

            //var remoteTransactionSigned = await _wallet.GetTransactionSign2Async(new Protocol.TransactionSign
            //{
            //    Transaction = result.Transaction,
            //    PrivateKey = ByteString.CopyFrom(privateKey.HexToByteArray()),
            //});

            //Assert.True(remoteTransactionSigned.Result.Result);

            var transactionBytes = result.Transaction.ToByteArray();
            var signedBytes = SignTransaction2Byte(transactionBytes, privateKey.HexToByteArray(), result.Transaction);
            var remoteTransactionSigned = Transaction.Parser.ParseFrom(signedBytes);

            Assert.Equal(remoteTransactionSigned.Signature[0], transactionSigned.Signature[0]);
        }
        private byte[] SignTransaction2Byte(byte[] transaction, byte[] privateKey, Transaction transactionSigned)
        {
            var ecKey = new ECKey(privateKey, true);
            var transaction1 = Transaction.Parser.ParseFrom(transaction);
            var rawdata = transaction1.RawData.ToByteArray();
            var hash = rawdata.ToSHA256Hash();
            var sign = ecKey.Sign(hash).ToByteArray();

            transaction1.Signature.Add(ByteString.CopyFrom(sign));

            return transaction1.ToByteArray();
        }


        [Fact]
        public async Task GetBalanceOf()
        {
            var transactionClient = _record.ServiceProvider.GetService<IContractClientFactory>();
            var privateKey = "8e812436a0e3323166e1f0e8ba79e19e217b2c4a53c970d4cca0cfb1078979df";
            var iAccount = new TronAccount(privateKey, TronNetwork.MainNet)
            {
            };
            var contractAddress = "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t";
            var result = await transactionClient.CreateClient(ContractProtocol.TRC20).BalanceOfAsync(contractAddress, iAccount);
            Console.WriteLine($"result:{result}");

            Assert.NotEqual(0M, result);
        }


        [Fact]
        public async Task TransferAsync()
        {
            var transactionClient = _record.ServiceProvider.GetService<IContractClientFactory>();
            var privateKey = "8e812436a0e3323166e1f0e8ba79e19e217b2c4a53c970d4cca0cfb1078979df";

            var iAccount = new TronAccount(privateKey, TronNetwork.MainNet);

            var contractAddress = "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t"; //USDT Contract Address
            var to = "TGehVcNhud84JDCGrNHKVz9jEAVKUpbuiv";
            decimal amount = (decimal)0.05; //USDT Amount
            var feeAmount = 8000000;

            //Return Return, Transaction Transaction
            var (result, transaction) = await transactionClient.CreateClient(ContractProtocol.TRC20).TransferAsync(contractAddress, iAccount, to, amount, string.Empty, feeAmount);

            Console.WriteLine($"result:{result.Result}");
            Assert.True(result.Result);

            Console.WriteLine($"Txid:{transaction.GetTxid()}");
            Assert.NotEmpty(transaction.GetTxid());
        }


        [Fact]
        public async Task GetTrc20Decimals()
        {
            var contractAddress = "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t"; //USDT Contract Address
            var ownerAddress = "TGehVcNhud84JDCGrNHKVz9jEAVKUpbuiv";

            var contractAddressBytes = Base58Encoder.DecodeFromBase58Check(contractAddress);
            var ownerAddressBytes = Base58Encoder.DecodeFromBase58Check(ownerAddress);

            var trc20Decimals = new DecimalsFunction();

            var callEncoder = new FunctionCallEncoder();
            var functionABI = ABITypedRegistry.GetFunctionABI<DecimalsFunction>();

            var encodedHex = callEncoder.EncodeRequest(trc20Decimals, functionABI.Sha3Signature);

            var trigger = new TriggerSmartContract
            {
                OwnerAddress = ByteString.CopyFrom(ownerAddressBytes),
                ContractAddress = ByteString.CopyFrom(contractAddressBytes),
                Data = ByteString.CopyFrom(encodedHex.HexToByteArray()),
            };

            var txnExt = await _wallet.TriggerConstantContractAsync(trigger, headers: _record.TronClient.GetWallet().GetHeaders());

            var message = txnExt.Result.Message.ToStringUtf8();
            Console.WriteLine($"{message}");

            var result = txnExt.ConstantResult[0].ToByteArray().ToHex();

            var fee = new FunctionCallDecoder().DecodeOutput<long>(result, new Parameter("uint8", "d"));

            Console.WriteLine(fee);
            Assert.NotEqual(0M, fee);
        }


        [Fact]
        public async Task GetTrc20Transfer()
        {
            decimal amount = 48M;
            var contractAddress = "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t"; //USDT Contract Address
            var ownerAddress = "TGehVcNhud84JDCGrNHKVz9jEAVKUpbuiv";

            var contractAddressBytes = Base58Encoder.DecodeFromBase58Check(contractAddress);
            var ownerAddressBytes = Base58Encoder.DecodeFromBase58Check(ownerAddress);

            var callerAddressBytes = Base58Encoder.DecodeFromBase58Check(ownerAddress);

            var toAddressBytes = new byte[20];
            Array.Copy(callerAddressBytes, 1, toAddressBytes, 0, toAddressBytes.Length);

            var toAddressHex = "0x" + toAddressBytes.ToHex();

            var tokenAmount = amount * Convert.ToDecimal(Math.Pow(10, 6));

            var functionABI = ABITypedRegistry.GetFunctionABI<TransferFunction>();

            var trc20Transfer = new TransferFunction
            {
                To = toAddressHex,
                TokenAmount = Convert.ToInt64(tokenAmount),
            };

            var encodedHex = new FunctionCallEncoder().EncodeRequest(trc20Transfer, functionABI.Sha3Signature);

            var trigger = new TriggerSmartContract
            {
                ContractAddress = ByteString.CopyFrom(contractAddressBytes),
                OwnerAddress = ByteString.CopyFrom(ownerAddressBytes),
                Data = ByteString.CopyFrom(encodedHex.HexToByteArray()),
            };

            var transactionExtention = await _wallet.TriggerConstantContractAsync(trigger, headers: _record.TronClient.GetWallet().GetHeaders());

            if (!transactionExtention.Result.Result)
            {
                Console.WriteLine($"[transfer]transfer failed, message={transactionExtention.Result.Message.ToStringUtf8()}.");
            }

            var message = transactionExtention.Result.Message.ToStringUtf8();
            Console.WriteLine($"{message}");

            var result = transactionExtention.ConstantResult[0].ToByteArray().ToHex();
            Console.WriteLine($"{result}");

            var transaction = transactionExtention.Transaction;

            if (transaction.Ret.Count > 0 && transaction.Ret[0].Ret == Transaction.Types.Result.Types.code.Failed)
            {
                Console.WriteLine($"{message}");
            }

            transaction.RawData.Data = ByteString.CopyFromUtf8("");
            transaction.RawData.FeeLimit = 1000000;

            Assert.NotEmpty(transaction.GetTxid());
        }

    }
}
