using Grpc.Core;
using System;
using System.Collections.Generic;

namespace TronNet
{
    public class TronNetOptions
    {
        public TronNetwork Network { get; set; }
        public GrpcChannelOption Channel { get; set; }

        public GrpcChannelOption SolidityChannel { get; set; }

        public List<string> ApiKeys { get; set; }

        internal Metadata GetgRPCHeaders()
        {
            var num = new Random().Next(0, ApiKeys.Count);

            return new Metadata
            {
                { "TRON-PRO-API-KEY", ApiKeys[num] }
            };
        }
    }
}
