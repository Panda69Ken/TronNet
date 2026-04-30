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
            string apiKey = "";

            if (ApiKeys.Count != 0)
            {
                if (ApiKeys.Count == 1)
                {
                    apiKey = ApiKeys[0];
                }
                else
                {
                    var num = new Random().Next(0, ApiKeys.Count);

                    apiKey = ApiKeys[num];
                }
            }

            return new Metadata
            {
                { "TRON-PRO-API-KEY", apiKey }
            };
        }
    }
}
