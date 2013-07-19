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
using Fody.DependencyInjection;
using Ninject;
using Ninject.Parameters;

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
        WeaverHelper.Execute<IKernel>(ModuleDefinition, ConfigureObject, LogInfo);
    }

    private void ConfigureObject(MethodDefinition ensureConfigurationMethod, TypeDefinition type)
    {
        //container.Inject(this)
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Newarr,
            ModuleDefinition.Import(typeof(IParameter))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt,
            ModuleDefinition.Import(typeof(IKernel).GetMethod("Inject", new[] { typeof(object), typeof(IParameter[]) }))));
    }
}
