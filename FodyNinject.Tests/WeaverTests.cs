using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using NUnit.Framework;

namespace FodyNinject.Tests
{
    [TestFixture]
    public class NinjectWeaverTests : WeaverTests<ModuleWeaver, IKernel>
    {
        protected override void CopyDependenciesToDirectoryAndInitializeContainer(string path)
        {
            File.Copy(Path.Combine(Environment.CurrentDirectory, @"Ninject.dll"),
                      Path.Combine(Path.GetDirectoryName(path), "Ninject.dll"), true);

            NinjectConfiguration.Configure(assembly);
        }
    }
}
