using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StowayNet
{
    public static class ServiceCollectionExtension
    {
        public static IStowayNetBuilder AddStowayNet(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();

            var configuration = provider.GetService<IConfiguration>();
            var serviceRegisterType = typeof(IStowayServiceRegister);
            var deps = Microsoft.Extensions.DependencyModel.DependencyContext.Default;

            var allTypes = new List<Type>();

            var libs = deps.RuntimeLibraries.Where(lib => !lib.Serviceable && lib.Type != "package");
            foreach (var lib in libs)
            {
                try
                {
                    var assemblyName = new AssemblyName(lib.Name);
                    var assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
                    allTypes.AddRange(assembly.GetTypes().Where(type => type != null));
                }
                catch (Exception ex)
                {
                    throw new Exception($"load assembly exception：{lib.Name}.", ex);
                }
            }

            var registerTypes = allTypes.Where(t => serviceRegisterType.IsAssignableFrom(t) && t != serviceRegisterType).ToArray();
            var serviceRegisters = new List<IStowayServiceRegister>();
            foreach (var drType in registerTypes)
            {
                serviceRegisters.Add((IStowayServiceRegister)Activator.CreateInstance(drType));
            }

            InternalServiceRegister.Register(services, allTypes, configuration);
            foreach (var manager in serviceRegisters)
            {
                manager.Register(services, allTypes, configuration);
            }

            var builder = new InternalStowayNetBuilder(services);
            return builder;
        }

        public static IServiceCollection RegisterTypes(this IServiceCollection services, Type[] types, ServiceLifetime serviceLifetime, bool asImplementedInterfaces = true, bool asSelf = false)
        {
            var typeDic = new Dictionary<Type, Type[]>();
            var ignoreTypes = new Type[] { typeof(IStowayDependency), typeof(IDisposable) };
            foreach (var t in types)
            {
                if (asSelf)
                {
                    services.TryAdd(new ServiceDescriptor(t, t, serviceLifetime));
                }
                if (!asImplementedInterfaces) continue;

                var interfaces = t.GetInterfaces().Where(p => !ignoreTypes.Contains(p)).ToArray();
                if (interfaces.Length == 0)
                {
                    continue;
                }
                typeDic.Add(t, interfaces);
            }
            if (typeDic.Count == 0) return services;
            foreach (var instanceType in typeDic.Keys)
            {
                foreach (var interfaceType in typeDic[instanceType])
                {
                    services.Add(new ServiceDescriptor(interfaceType, instanceType, serviceLifetime));
                }
            }

            return services;
        }

    }

}
