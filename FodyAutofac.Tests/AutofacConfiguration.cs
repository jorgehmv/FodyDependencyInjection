using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Fody.DependencyInjection;

namespace FodyAutofac.Tests
{
    public static class AutofacConfiguration
    {
        public static void Configure(Assembly assembly)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(assembly.GetType("AssemblyToProcess.Service")).As(assembly.GetType("AssemblyToProcess.IService"));
            builder.RegisterType(assembly.GetType("AssemblyToProcess.Service2")).As(assembly.GetType("AssemblyToProcess.IService2"));
            
            builder.RegisterType(assembly.GetType("AssemblyToProcess.ClassWithNoCtors")).AsSelf();
            builder.RegisterType(assembly.GetType("AssemblyToProcess.ClassWithStaticCtor")).AsSelf();
            builder.RegisterType(assembly.GetType("AssemblyToProcess.ClassWithNoDefaultCtor")).AsSelf();
            builder.RegisterType(assembly.GetType("AssemblyToProcess.ClassUsingInjectedPropertyInsideCtor")).AsSelf();
            builder.RegisterType(assembly.GetType("AssemblyToProcess.ClassWithManyCtors")).AsSelf();
            builder.RegisterType(assembly.GetType("AssemblyToProcess.ClassNotConfigurableInheritingFromBase")).AsSelf();
            builder.RegisterType(assembly.GetType("AssemblyToProcess.ClassConfigurableInheritingFromBase")).AsSelf();

            ConfigurableInjection.InitializeContainer(builder.Build());
        }
    }
}
