using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;

namespace FodyAutofac.Tests
{
    [TestFixture]
    public class AutofacWeaverTests : WeaverTests<ModuleWeaver, IComponentContext>
    {
        protected override void CopyDependenciesToDirectoryAndInitializeContainer(string path)
        {
            File.Copy(Path.Combine(Environment.CurrentDirectory, @"Autofac.dll"),
                      Path.Combine(Path.GetDirectoryName(path), "Autofac.dll"), true);

            AutofacConfiguration.Configure(assembly);
        }
    }
}
