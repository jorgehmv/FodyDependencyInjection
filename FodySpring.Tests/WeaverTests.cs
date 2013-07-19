using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Fody.DependencyInjection;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;

[TestFixture]
public class SpringWeaverTests : WeaverTests<ModuleWeaver, IApplicationContext>
{
    protected override void CopyDependenciesToDirectoryAndInitializeContainer(string path)
    {
        File.Copy(Path.Combine(Environment.CurrentDirectory, "Spring.Core.dll"),
                  Path.Combine(Path.GetDirectoryName(path), "Spring.Core.dll"), true);
        
        ConfigurableInjection.InitializeContainer(ContextRegistry.GetContext());
    }
}