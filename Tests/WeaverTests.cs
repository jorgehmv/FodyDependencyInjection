using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

[TestFixture]
public abstract class WeaverTests<TModule, TContainer> where TModule : new()
{
    protected Assembly assembly;

    [TestFixtureSetUp]
    public void Setup()
    {
        assembly = WeaverHelper.WeaveAssembly<TModule, TContainer>();
        CopyDependenciesToDirectoryAndInitializeContainer(Path.GetFullPath(assembly.CodeBase.Remove(0, 8)));
    }

    [Test]
    public void ValidateObjectIsInjected()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassWithNoCtors");
        var instance = (dynamic)Activator.CreateInstance(type);
        
        Assert.AreEqual("string from service", instance.InjectedService.GetStringFromService());
    }

    [Test]
    public void ValidateSupportsClassesWithStaticCtors()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassWithStaticCtor");
        var instance = (dynamic)Activator.CreateInstance(type);

        Assert.AreEqual("string from service", instance.InjectedService.GetStringFromService());
    }

    [Test]
    public void ValidateSupportsClassesWithNoDefaultCtor()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassWithNoDefaultCtor");
        var instance = (dynamic)Activator.CreateInstance(type, "arg");

        Assert.AreEqual("string from service", instance.InjectedService.GetStringFromService());
    }

    [Test]
    public void ValidateSupportsUseInjectedPropertiesInsideCtor()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassUsingInjectedPropertyInsideCtor");
        var instance = (dynamic)Activator.CreateInstance(type, " appended text");

        Assert.AreEqual("string from service appended text", instance.TestString);
    }

    [Test]
    public void ValidateSupportsClassWithManyCtorsCallingDefault()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassWithManyCtors");
        var instance = (dynamic)Activator.CreateInstance(type);

        Assert.AreEqual("string from service", instance.InjectedService.GetStringFromService());
    }

    [Test]
    public void ValidateSupportsClassWithManyCtorsCallingNotDefaultThatCallsDefault()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassWithManyCtors");
        var instance = (dynamic)Activator.CreateInstance(type, " appended text 1");

        Assert.AreEqual("string from service appended text 1", instance.TestString);
    }

    [Test]
    public void ValidateSupportsClassWithManyCtorsCallingInChain()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassWithManyCtors");
        var instance = (dynamic)Activator.CreateInstance(type, " appended text 1", " appended text 2");

        Assert.AreEqual("string from service appended text 1 appended text 2", instance.TestString);
    }

    [Test]
    public void ValidateSupportsClassNotConfigurableInheritingFromConfigurable()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassNotConfigurableInheritingFromBase");
        var instance = (dynamic)Activator.CreateInstance(type);

        Assert.AreEqual("string from service appended from subclass", instance.TestString);
    }

    [Test]
    public void ValidateSupportsClassConfigurableInheritingFromConfigurable()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassConfigurableInheritingFromBase");
        var instance = (dynamic)Activator.CreateInstance(type);

        Assert.AreEqual("string from service", instance.InjectedService.GetStringFromService());
        Assert.AreEqual("string from service 2", instance.InjectedService2.GetStringFromService2());
    }

    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assembly.CodeBase.Remove(0, 8));
    }

    protected abstract void CopyDependenciesToDirectoryAndInitializeContainer(string path);

}