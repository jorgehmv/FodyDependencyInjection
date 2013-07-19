using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Components;
using Ninject.Selection.Heuristics;

namespace FodyNinject.Tests
{
    /// <summary>
    /// This class is NOT required by this Fody addin, this is only used to avoid using [Inject] attributes
    /// in AssemblyToProcess project. 
    /// </summary>
    public class PropertyInjectionHeuristic : NinjectComponent, IInjectionHeuristic
    {
        private readonly IKernel kernel;

        public PropertyInjectionHeuristic(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public bool ShouldInject(MemberInfo member)
        {
            var propertyInfo = member.ReflectedType.GetProperty(member.Name);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                object service = kernel.TryGet(propertyInfo.PropertyType);

                return service != null;
            }

            return false;
        }
    }
}
