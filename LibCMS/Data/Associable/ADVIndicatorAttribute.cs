using LibUtil.UtilGlobal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibCMS.Data.Associable
{
    // The presence of this attribute on a field indicates that the field is used in Dataverse for read/write operations.
    // This attribute contains the logical name of the field as used in dataverse
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class ADVIndicatorAttribute : System.Attribute
    {
        private string attributeName; // the logical name of this attribute as used in dataverse
        internal ADVIndicatorAttribute(string attributeName)
        {
            this.attributeName = CGlobal.Prefix() + attributeName;
        }
        public override string ToString()
            => attributeName;
        public string AttributeName()
            => attributeName;
    }

    // The presence of this attribute on a field indicates that the field is a complex type and should be expanded to check for more DVIndicatorAttributes
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class ADVIndicatorNestedAttribute : System.Attribute { }
}
