using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Spring.Context;
using Spring.Objects.Factory;

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
        WeaverHelper.Execute<IApplicationContext>(ModuleDefinition, ConfigureObject, LogInfo);
    }

    private void ConfigureObject(MethodDefinition ensureConfigurationMethod, TypeDefinition type)
    {
        //container.ConfigureObject(objectToConfigure, "typeName");
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, type.Name));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt,
            ModuleDefinition.Import(typeof(IObjectFactory).GetMethod("ConfigureObject"))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Pop));
    }
}
