using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fody.DependencyInjection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public static class WeaverHelper
{
    public static void Execute<TContainer>(ModuleDefinition moduleDefinition, 
                                           Action<MethodDefinition, TypeDefinition> configureObject, 
                                           Action<string> logInfo = null)
    {
        logInfo = logInfo ?? (m => { });
        var configurableTypes = moduleDefinition.Types.Where(t => t.CustomAttributes
                            .Any(c => c.AttributeType.Name == typeof(ConfigurableAttribute).Name));

        foreach (var type in configurableTypes)
        {
            if (type.IsSealed && type.IsAbstract)
            {
                logInfo(string.Format("Type {0} is static and cannot be configurable. Skipping type", type));
                continue;
            }

            var isConfiguredField = new FieldDefinition("<>__isConfigured",
                                                        Mono.Cecil.FieldAttributes.Private,
                                                        moduleDefinition.TypeSystem.Boolean);
            type.Fields.Add(isConfiguredField);
            var ensureConfigurationMethod = GenerateEnsureConfigurationMethod<TContainer>(isConfiguredField, type, moduleDefinition, configureObject);
            type.Methods.Add(ensureConfigurationMethod);

            foreach (var ctor in type.GetConstructors().Where(ctor => !ctor.IsStatic))
            {
                var baseCtorCall = ctor.Body.Instructions.Single(i => IsCallToCtor(i));
                var baseCtorCallIndex = ctor.Body.Instructions.IndexOf(baseCtorCall);

                ctor.Body.Instructions.Insert(baseCtorCallIndex + 1, Instruction.Create(OpCodes.Ldarg_0));
                ctor.Body.Instructions.Insert(baseCtorCallIndex + 2, Instruction.Create(OpCodes.Callvirt, ensureConfigurationMethod));
            }
        }
    }

    private static MethodDefinition GenerateEnsureConfigurationMethod<TContainer>(FieldDefinition isConfiguredField, TypeDefinition type, ModuleDefinition moduleDefinition, Action<MethodDefinition, TypeDefinition> configureObject)
    {
        var exitMethod = Instruction.Create(OpCodes.Ret);
        var ensureConfigurationMethod = new MethodDefinition("<>__EnsureConfiguration",
                                                             Mono.Cecil.MethodAttributes.Family | Mono.Cecil.MethodAttributes.Virtual,
                                                             moduleDefinition.TypeSystem.Void);
        ensureConfigurationMethod.Body.SimplifyMacros();

        ensureConfigurationMethod.Body.InitLocals = true;
        IfNotIsConfigured<TContainer>(isConfiguredField, exitMethod, ensureConfigurationMethod);
        IfNotAvoidConfigurationSetting(exitMethod, ensureConfigurationMethod, moduleDefinition);
        EnsureContainerHasBeenSet<TContainer>(ensureConfigurationMethod, moduleDefinition);
        configureObject(ensureConfigurationMethod, type);
        SetIsConfiguredToTrue(isConfiguredField, exitMethod, ensureConfigurationMethod);

        ensureConfigurationMethod.Body.OptimizeMacros();
        return ensureConfigurationMethod;
    }

    private static void IfNotIsConfigured<TContainer>(FieldDefinition isConfiguredField, Instruction exitMethod, MethodDefinition ensureConfigurationMethod)
    {
        //if(<>__isConfigured)
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, isConfiguredField));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Brtrue, exitMethod));
    }

    private static void IfNotAvoidConfigurationSetting(Instruction exitMethod, MethodDefinition ensureConfigurationMethod, ModuleDefinition moduleDefinition)
    {
        //string avoidConfiguration = ConfigurationManager.AppSettings["Fody.DependencyInjection.AvoidConfiguration"];
        //if (!string.Equals(avoidConfiguration, "true", StringComparison.OrdinalIgnoreCase))
        ensureConfigurationMethod.Body.Variables.Add(new VariableDefinition("avoidConfiguration", moduleDefinition.TypeSystem.String));
        ensureConfigurationMethod.Body.Variables.Add(new VariableDefinition("equalsTrue", moduleDefinition.TypeSystem.Boolean));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call,
            moduleDefinition.Import(typeof(ConfigurationManager).GetMethod("get_AppSettings", Type.EmptyTypes))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, "Fody.DependencyInjection.AvoidConfiguration"));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt,
            moduleDefinition.Import(typeof(NameValueCollection).GetMethod("get_Item", new[] { typeof(string) }))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, "true"));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_5));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call,
            moduleDefinition.Import(typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(string), typeof(StringComparison) }))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc_1));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc_1));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Brtrue, exitMethod));
    }

    private static void EnsureContainerHasBeenSet<TContainer>(MethodDefinition ensureConfigurationMethod, ModuleDefinition moduleDefinition)
    {
        //TContainer container = ConfigurableInjection.Container as TContainer;
        ensureConfigurationMethod.Body.Variables.Add(new VariableDefinition("container",
            moduleDefinition.Import(typeof(TContainer))));
        ensureConfigurationMethod.Body.Variables.Add(new VariableDefinition("localConditionResult", moduleDefinition.TypeSystem.Boolean));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call,
            moduleDefinition.Import(typeof(ConfigurableInjection).GetMethod("get_Container", Type.EmptyTypes))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Isinst,
            moduleDefinition.Import(typeof(TContainer))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc_2));

        //if (container == null)
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc_2));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc_3));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc_3));
        var notNullBranch = Instruction.Create(OpCodes.Ldloc_2);
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Brtrue_S, notNullBranch));

        //throw new InvalidOperationException("Container cannot be null and must be of type TContainer.");
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr,
                                string.Format("Container cannot be null and must be of type {0}.", typeof(TContainer).Name)));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj,
            moduleDefinition.Import(typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) }))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Throw));

        ensureConfigurationMethod.Body.Instructions.Add(notNullBranch);
    }

    private static void SetIsConfiguredToTrue(FieldDefinition isConfiguredField, Instruction exitMethod, MethodDefinition ensureConfigurationMethod)
    {
        //<>__isConfigured = true;
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, isConfiguredField));
        ensureConfigurationMethod.Body.Instructions.Add(exitMethod);
    }

    private static bool IsCallToCtor(Instruction instruction)
    {
        const string ctor = ".ctor";

        if (instruction.OpCode == OpCodes.Call)
        {
            var methodReference = instruction.Operand as MethodReference;
            if (methodReference != null)
            {
                return methodReference.Name == ctor;
            }

            var methodDefinition = instruction.Operand as MethodDefinition;
            if (methodDefinition != null)
            {
                return methodDefinition.Name == ctor;
            }
        }

        return false;
    }
}