using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace StowayNet
{
    public interface IStowayServiceRegister
    {
        void Register(IServiceCollection services, List<Type> types, IConfiguration configuration);

    }
}
