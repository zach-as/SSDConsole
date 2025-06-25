using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.UtilEquality
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class AEqualityAttribute : System.Attribute
    {
        private string logicalName; // the logical name of this attribute
        public AEqualityAttribute(string logicalName)
        {
            this.logicalName = logicalName;
        }

        public override string ToString() => logicalName;
        public string LogicalName() => logicalName;
    }
}
