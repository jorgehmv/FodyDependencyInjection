using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fody.DependencyInjection
{
    public static class ConfigurableInjection
    {
        public static void InitializeContainer(object container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            Container = container;
        }

        public static object Container { get; private set; }
    }
}
