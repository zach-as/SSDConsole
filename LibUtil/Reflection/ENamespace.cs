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
        private ENamespace? parent; // the namespace of the parent (if present)
        internal ANamespaceAttribute(string ns, ENamespace parent)
        {
            this.ns = ns;
            this.parent = parent;
        }
        internal ANamespaceAttribute(string ns)
        {
            this.ns = ns;
        }

        public string Name()
        {
            if (parent is null)
                return ns;
            return parent?.Name() + "." + ns;
        }
    }

    public enum ENamespace 
    {
        [ANamespace("LibUtil")]
        LibUtil,
        [ANamespace("Reflection", LibUtil)]
        LibUtil_Reflection,

        [ANamespace("LibDV")]
        LibDV,
        [ANamespace("Associable",LibDV)]
        LibDV_Associable,
    }

    public static class SNamespace
    {
        private static ANamespaceAttribute NamespaceAttribute(this ENamespace ns)
            => ns.InternalAttribute<ANamespaceAttribute>();

        public static string Name(this ENamespace ns)
            => ns.NamespaceAttribute().Name();

    }
}
