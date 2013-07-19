using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fody.DependencyInjection;
using Ninject;
using Ninject.Selection.Heuristics;

namespace FodyNinject.Tests
{
    public static class NinjectConfiguration
    {
        public static void Configure(Assembly assembly)
        {
            IKernel kernel = new StandardKernel();
            kernel.Components.Add<IInjectionHeuristic, PropertyInjectionHeuristic>();

            kernel.Bind(assembly.GetType("AssemblyToProcess.IService")).To(assembly.GetType("AssemblyToProcess.Service"));
            kernel.Bind(assembly.GetType("AssemblyToProcess.IService2")).To(assembly.GetType("AssemblyToProcess.Service2"));

            kernel.Bind(assembly.GetType("AssemblyToProcess.ClassWithNoCtors")).ToSelf().InTransientScope();
            kernel.Bind(assembly.GetType("AssemblyToProcess.ClassWithStaticCtor")).ToSelf().InTransientScope();
            kernel.Bind(assembly.GetType("AssemblyToProcess.ClassWithNoDefaultCtor")).ToSelf().InTransientScope();
            kernel.Bind(assembly.GetType("AssemblyToProcess.ClassUsingInjectedPropertyInsideCtor")).ToSelf().InTransientScope();
            kernel.Bind(assembly.GetType("AssemblyToProcess.ClassWithManyCtors")).ToSelf().InTransientScope();
            kernel.Bind(assembly.GetType("AssemblyToProcess.ClassNotConfigurableInheritingFromBase")).ToSelf().InTransientScope();
            kernel.Bind(assembly.GetType("AssemblyToProcess.ClassConfigurableInheritingFromBase")).ToSelf().InTransientScope();

            ConfigurableInjection.InitializeContainer(kernel);
        }
    }

}
