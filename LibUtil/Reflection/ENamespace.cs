using LibUtil.UtilAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.Reflection
{

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class ANamespaceAttribute : System.Attribute
    {
        private string ns; // the namespace name as a string
        private ENamespace? parent; // the namespace of the parent (if child namespace)
        private EAssembly? assembly; // the assembly of the namespace (if root namespace)

        // Constructor for child namespace
        internal ANamespaceAttribute(ENamespace parent, string ns)
        {
            this.ns = ns;
            this.parent = parent;
        }
        // Constructor for root namespace
        internal ANamespaceAttribute(EAssembly assembly, string ns)
        {
            this.assembly = assembly;
            this.ns = ns;
        }   

        public string Name()
        {
            if (parent is null)
                return ns;
            return parent?.Name() + "." + ns;
        }

        public EAssembly Assembly()
            => assembly.HasValue ? 
                assembly.Value : // if assembly is not null, return assembly
                parent!.Value.Assembly(); // if assembly is null, return assembly of parent
    }

    public enum ENamespace 
    {
        [ANamespace(EAssembly.LibUtil, "LibUtil")]
        LibUtil,
        [ANamespace(LibUtil, "Reflection")]
        LibUtil_Reflection,

        [ANamespace(EAssembly.LibDV, "LibDV")]
        LibDV,
        [ANamespace(LibDV, "Associable")]
        LibDV_Associable,
    }

    public static class SNamespace
    {
        private static ANamespaceAttribute NamespaceAttribute(this ENamespace ns)
            => ns.InternalAttribute<ANamespaceAttribute>();

        public static string Name(this ENamespace ns)
            => ns.NamespaceAttribute().Name();

        public static EAssembly Assembly(this ENamespace ns)
            => ns.NamespaceAttribute().Assembly();
    }
}
