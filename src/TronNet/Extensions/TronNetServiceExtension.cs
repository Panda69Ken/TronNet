using Microsoft.Extensions.DependencyInjection;
using System;
using TronNet.Accounts;

namespace TronNet
{
    public static class TronNetServiceExtension
    {
        public static StowayNet.IStowayNetBuilder AddTronNet(StowayNet.IStowayNetBuilder builder, Action<TronNetOptions> setupAction)
        {
            builder.Services.AddTronNet(setupAction);

            return builder;
        }

        public static IServiceCollection AddTronNet(this IServiceCollection services, Action<TronNetOptions> setupAction)
        {
            var options = new TronNetOptions();

            setupAction(options);

            services.AddTransient<ITransactionClient, TransactionClient>();
            services.AddTransient<IGrpcChannelClient, GrpcChannelClient>();
            services.AddTransient<ITronClient, TronClient>();
            services.AddTransient<IWalletClient, WalletClient>();
            services.AddSingleton<Contracts.IContractClientFactory, Contracts.ContractClientFactory>();
            services.AddTransient<Contracts.TRC20ContractClient>();
            services.AddTransient<ITronAccountWallet, TronAccountWallet>();
            services.Configure(setupAction);

            return services;
        }
    }
}
