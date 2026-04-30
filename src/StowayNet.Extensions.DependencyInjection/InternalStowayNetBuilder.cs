using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowayNet
{
    internal class InternalStowayNetBuilder : IStowayNetBuilder
    {
        public IServiceCollection Services { get; }

        public InternalStowayNetBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
