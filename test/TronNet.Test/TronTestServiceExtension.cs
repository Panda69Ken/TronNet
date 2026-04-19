using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace TronNet.Test
{
    public record TronTestRecord(IServiceProvider ServiceProvider, ITronClient TronClient, IOptions<TronNetOptions> Options);
    public static class TronTestServiceExtension
    {
        public static IServiceProvider AddTronNet()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTronNet(x =>
            {
                x.Network = TronNetwork.MainNet;
                x.Channel = new GrpcChannelOption { Host = "grpc.shasta.trongrid.io", Port = 50051 };
                x.SolidityChannel = new GrpcChannelOption { Host = "grpc.shasta.trongrid.io", Port = 50052 };
                x.ApiKeys = ["e5162bcc-a938-47fc-87c3-7f1198765907"];
            });
            services.AddLogging();
            return services.BuildServiceProvider();
        }

        public static TronTestRecord GetTestRecord()
        {
            var provider = TronTestServiceExtension.AddTronNet();
            var client = provider.GetService<ITronClient>();
            var options = provider.GetService<IOptions<TronNetOptions>>();

            return new TronTestRecord(provider, client, options);
        }
    }

}
