using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fody.DependencyInjection;

namespace AssemblyToProcess
{
    [Configurable]
    public class ClassWithNoCtors
    {
        public IService InjectedService { get; set; }
    }

    [Configurable]
    public class ClassWithStaticCtor
    {
        private static readonly string someField;
        static ClassWithStaticCtor()
        {
            someField = "'supports static ctors' means it simply ignores them and do not break";
        }

        public IService InjectedService { get; set; }
    }

    [Configurable]
    public class ClassWithNoDefaultCtor
    {
        public ClassWithNoDefaultCtor(string arg)
        {
        }
        [Ninject.Inject]
        public IService InjectedService { get; set; }
    }

    [Configurable]
    public class ClassUsingInjectedPropertyInsideCtor
    {
        public ClassUsingInjectedPropertyInsideCtor(string textToAppend)
        {
            TestString = InjectedService.GetStringFromService() + textToAppend;
        }

        public IService InjectedService { get; set; }

        public string TestString { get; set; }
    }

    [Configurable]
    public class ClassWithManyCtors
    {
        public ClassWithManyCtors()
        {
            TestString = InjectedService.GetStringFromService();
        }

        public ClassWithManyCtors(string arg)
            : this()
        {
            TestString += arg;
        }

        public ClassWithManyCtors(string arg1, string arg2)
            : this(arg1)
        {
            TestString += arg2;
        }

        public IService InjectedService { get; set; }

        public string TestString { get; set; }
    }

    [Configurable]
    public static class ClassStatic
    {
        public static IService InjectedService { get; set; }
    }

    [Configurable]
    public class ClassBase
    {
        public IService InjectedService { get; set; }
    }

    public class ClassNotConfigurableInheritingFromBase : ClassBase
    {
        public ClassNotConfigurableInheritingFromBase()
        {
            TestString = InjectedService.GetStringFromService() + " appended from subclass";
        }

        public string TestString { get; set; }
    }

    [Configurable]
    public class ClassConfigurableInheritingFromBase : ClassBase
    {
        public IService2 InjectedService2 { get; set; }
    }

    public interface IService
    {
        string GetStringFromService();
    }

    public class Service : IService
    {
        public string GetStringFromService()
        {
            return "string from service";
        }
    }

    public interface IService2
    {
        string GetStringFromService2();
    }

    public class Service2 : IService2
    {
        public string GetStringFromService2()
        {
            return "string from service 2";
        }
    }
}
