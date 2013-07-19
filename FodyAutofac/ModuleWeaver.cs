using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Collections.Specialized;
using Autofac;
using Fody.DependencyInjection;

public class ModuleWeaver
{
    public Action<string> LogInfo { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public IAssemblyResolver AssemblyResolver { get; set; }

    public ModuleWeaver()
    {
        LogInfo = m => { };
    }

    public void Execute()
    {
        WeaverHelper.Execute<IComponentContext>(ModuleDefinition, ConfigureObject, LogInfo);
    }

    private void ConfigureObject(MethodDefinition ensureConfigurationMethod, TypeDefinition type)
    {
        //container.InjectProperties(this)
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call,
            ModuleDefinition.Import(typeof(ResolutionExtensions).GetMethod("InjectProperties")
                .MakeGenericMethod(new[] { typeof(object) }))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Pop));
    }
}
