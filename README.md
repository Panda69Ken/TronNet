# TronNet
TronNet is a SDK that includes libraries for working with TRON, TronNet makes it easy to build TRON applications with .net.

## Get Started
### NuGet 

You can run the following command to install the `TronNet` in your project.

```
PM> Install-Package TronNet
```

## 注意
为什么Fork，原库很久没更新维护，需要使用Tron的最新[protocol](https://github.com/tronprotocol/protocol)功能只能自行更新，使用NuGet版本依然是旧版本的。
并且根据自身业务进行了代码调整

### Configuration

First,You need to config `TronNet` in your `Startup.cs`:
```c#
......
using StowayNet;
using TronNet;
......

public void ConfigureServices(IServiceCollection services)
{
    ......

    services.AddTronNet(x =>
    {
        x.Network = TronNetwork.MainNet;
        x.Channel = new GrpcChannelOption { Host = "grpc.shasta.trongrid.io", Port = 50051 };
        x.SolidityChannel = new GrpcChannelOption { Host = "grpc.shasta.trongrid.io", Port = 50052 };
        x.ApiKey = "input your api key";
    });

    ......
}

```

### Sample

#### Sample 1: Generate Address Offline

```c#
using TronNet;

namespace TronNetTest
{
    class Class1
    {
        private readonly ITronClient _tronClient;

        public Class1(ITronClient tronClient)
        {
            _tronClient = tronClient;
        }

        public void GenerateAddress()
        {
            var key = TronECKey.GenerateKey(TronNetwork.MainNet);

            var address = key.GetPublicAddress();
        }
    }
}


```

#### Sample 2: Transaction Sign Offline
```c#
using TronNet;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Options;

namespace TronNetTest
{
    class Class1
    {
        private readonly ITransactionClient _transactionClient;
        private readonly IOptions<TronNetOptions> _options;
        public Class1(ITransactionClient transactionClient, IOptions<TronNetOptions> options)
        {
            _options = options;
            _transactionClient = transactionClient;
        }

        public async Task SignAsync()
        {
            var privateKey = "D95611A9AF2A2A45359106222ED1AFED48853D9A44DEFF8DC7913F5CBA727366";
            var ecKey = new TronECKey(privateKey, _options.Value.Network);
            var from = ecKey.GetPublicAddress();
            var to = "TGehVcNhud84JDCGrNHKVz9jEAVKUpbuiv";
            var amount = 100_000_000L;
            var transactionExtension = await _transactionClient.CreateTransactionAsync(from, to, amount);

            var transactionSigned = _transactionClient.GetTransactionSign(transactionExtension.Transaction, privateKey);
            
            var result = await _transactionClient.BroadcastTransactionAsync(transactionSigned);
        }
    }
}

```
also see: https://github.com/stoway/TronNet/blob/main/test/TronNet.Test/TransactionSignTest.cs

#### Sample 3: Contract TRC20 Transfer (USDT)
```c#
using TronNet;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Options;

namespace TronNetTest
{
    class Class1
    {
        private readonly IWalletClient _wallet;
        private readonly IContractClientFactory _contractClientFactory;

        public Class1(IWalletClient wallet, IContractClientFactory contractClientFactory)
        {
            _wallet = wallet;
            _contractClientFactory = contractClientFactory;
        }

        public async Task TransferAsync()
        {
            var privateKey = "8e812436a0e3323166e1f0e8ba79e19e217b2c4a53c970d4cca0cfb1078979df";
            var account = _wallet.GetAccount(privateKey);

            var contractAddress = "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t"; //USDT Contract Address
            var to = "TGehVcNhud84JDCGrNHKVz9jEAVKUpbuiv";
            var amount = 10; //USDT Amount
            var feeAmount = 5 * 1000000L;
            var contractClient = _contractClientFactory.CreateClient(ContractProtocol.TRC20);

            var result = await contractClient.TransferAsync(contractAddress, account, to, amount, string.Empty, feeAmount);
        }
    }
}

```

#### 2026-04-19修改内容
- 1.升级到Net8.0，相关库更新到最新稳定版
- 2.更新最新的Protos文件
- 3.配置实体TronNetOptions优化添加 ApiKeys 字段，删除ApiKey字段，支持多key随机使用
- 4.优化WalletClient实体的GetHeaders方法，支持多key随机使用
- 5.添加操作TRX交易与读余额的ITronAccountWallet接口与实现类
- 6.优化IContractClient、TRC20ContractClient里的TransferAsync、BalanceOfAsync

