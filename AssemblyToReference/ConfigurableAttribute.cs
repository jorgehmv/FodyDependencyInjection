using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fody.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigurableAttribute : Attribute
    {
    }
}
