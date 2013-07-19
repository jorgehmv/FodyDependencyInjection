using System;
using System.IO;
using System.Reflection;
using System.Linq;
using Mono.Cecil;

public class WeaverHelper
{

    public static Assembly WeaveAssembly<TModule, TContainer>() where TModule : new()
    {
        var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToProcess\AssemblyToProcess.csproj"));
        var assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToProcess.dll");

#if (!DEBUG)
        assemblyPath = assemblyPath.Replace("Debug", "Release");
        addinBinPath = springBinPath.Replace("Debug", "Release");
#endif

        var newAssembly = assemblyPath.Replace(".dll", "2.dll");
        File.Copy(assemblyPath, newAssembly, true);

        var moduleDefinition = ModuleDefinition.ReadModule(newAssembly);
        dynamic weavingTask = new TModule();
        weavingTask.ModuleDefinition = moduleDefinition;

        weavingTask.Execute();
        moduleDefinition.Write(newAssembly);

        return Assembly.LoadFile(newAssembly);
    }
}