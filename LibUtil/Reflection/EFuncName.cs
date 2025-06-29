using LibUtil.UtilAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.Reflection
{

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class AClassFuncAttribute : System.Attribute
    {
        private string funcName; // the func name as a string
        private EClassName cn; // the name of this class

        internal AClassFuncAttribute(string funcName, EClassName cn)
        {
            this.cn = cn;
            this.funcName = funcName;
        }

        public string Name() => funcName;
        public string QualifiedName() => $"{cn.QualifiedName()}.{funcName}";

        public EClassName ClassName() => cn;
    }

    public enum EFuncName 
    {
        // This function accepts a string input representing clinician sex ("M" or "F")
        // and returns an OptionSetValue representing a specific option set in DV.
        // This function accepts exactly one parameter, which is the string input.
        [AClassFunc("Sex", EClassName.LibDV_SAssociable)]
        LibDV_SAssociable_Sex,

    }

    public static class SFuncName
    {
        private static AClassFuncAttribute FuncNameAttribute(this EFuncName funcName)
            => funcName.InternalAttribute<AClassFuncAttribute>();
        public static string Name(this EFuncName funcName)
                => funcName.FuncNameAttribute().Name();
        public static string QualifiedName(this EFuncName funcName)
            => funcName.FuncNameAttribute().QualifiedName();
        public static MethodInfo GetMethod(this EFuncName funcName)
        {
            var className = funcName.FuncNameAttribute().ClassName();
            var func = className.GetType().GetMethod(funcName.Name());
            if (func is null)
                throw new Exception($"Function {funcName.Name()} not found in {className.QualifiedName()}.");
            else
                return func;
        }
    }
}
