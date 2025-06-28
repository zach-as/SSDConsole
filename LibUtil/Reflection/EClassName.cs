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
    internal class AClassNameAttribute : System.Attribute
    {
        private string className; // the class name as a string
        private ENamespace ns; // the namespace of this class

        internal AClassNameAttribute(string className, ENamespace ns)
        {
            this.ns = ns;
            this.className = className;
        }

        public string Name() => className;
        public string QualifiedName() => $"{ns.Name()}.{className}";
    }

    public enum EClassName 
    {
        [AClassName("SAssociable", ENamespace.LibDV_Associable)]
        LibDV_SAssociable,

    }

    public static class SClassName
    {
        private static AClassNameAttribute ClassNameAttribute(this EClassName className)
            => className.InternalAttribute<AClassNameAttribute>();
        public static string Name(this EClassName className)
            => className.ClassNameAttribute().Name();
        public static string QualifiedName(this EClassName className)
            => className.ClassNameAttribute().QualifiedName();
        public static Type GetType(this EClassName className)
        {
            var classNameAttribute = className.ClassNameAttribute();
            var type = Type.GetType(classNameAttribute.QualifiedName());
            if (type is null)
                throw new ArgumentException($"Type '{classNameAttribute.QualifiedName()}' not found.");
            else
                return type;
        }
    }
}
