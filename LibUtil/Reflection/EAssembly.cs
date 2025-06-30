using LibUtil.UtilAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.Reflection
{

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class AAssemblyAttribute : System.Attribute
    {
        private string assemblyName; // the assembly name as a string

        internal AAssemblyAttribute(string assemblyName)
            => this.assemblyName = assemblyName;

        internal Assembly Load() => Assembly.Load(assemblyName);
        internal string Name() => assemblyName;
    }

    public enum EAssembly
    {
        [AAssembly("LibUtil")]
        LibUtil,
        [AAssembly("LibDV")]
        LibDV,
        [AAssembly("LibCMS")]
        LibCMS,
        [AAssembly("Console")]
        Console,
    }

    public static class SAssembly
    {
        private static AAssemblyAttribute AssemblyAttribute(this EAssembly assembly)
            => assembly.InternalAttribute<AAssemblyAttribute>();

        public static Assembly Load(this EAssembly assembly)
            => assembly.AssemblyAttribute().Load();

        public static string Name(this EAssembly assembly)
            => assembly.AssemblyAttribute().Name();
    }
}
