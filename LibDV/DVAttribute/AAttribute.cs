using LibDV.DVEntityType;
using LibUtil.UtilAttribute;
using LibUtil.UtilGlobal;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDV.DVAttribute
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class AAttributeAttribute : Attribute
    {
        private EAttributeName attributeName; // the name of this attribute as used in dataverse
        private EEntityType[] entityTypes; // the entity types that this attribute applies to

        internal AAttributeAttribute(EAttributeName attributeName, params EEntityType[] entityTypes)
        {
            this.attributeName = attributeName;
            this.entityTypes = entityTypes;
        }

        public string LogicalName() => attributeName.LogicalName();
        public string Name() => attributeName.Name();
        public EAttributeName AttributeName() => attributeName;
        internal EEntityType[] EntityTypes() => entityTypes;
    }

    // If this attribtue is present on an EAttribute, it indicates that the EAttribute should be included in the columnset when fetching from dataverse
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class ADVReadAttribute : Attribute { }

    // If this attribute is present on an EAttribute, it indicates that the EAttribute should be written to dataverse when creating or updating the relevant entity.
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class ADVWriteAttribute : Attribute { }
}
