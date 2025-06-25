using LibUtil.UtilGlobal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.UtilAttribute
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class AAttributeTagAttribute : System.Attribute
    {
        private EAttributeName attrName; // the enum representing the statically typed attribute names
        private string logicalName; // the logical name of this attribute as used in dataverse
        public AAttributeTagAttribute(EAttributeName attrName)
        {
            this.attrName = attrName;
            logicalName = CGlobal.Prefix() + attrName.Name();
        }

        public EAttributeName AttributeName() => attrName;
        public override string ToString() => logicalName;
        public string LogicalName() => logicalName;
    }
}
