using LibDV.DVEntityType;
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
        private string attributeName; // the name of this attribute as used in dataverse
        private EEntityType[] entityTypes; // the entity types that this attribute applies to

        internal AAttributeAttribute(string attributeName, params EEntityType[] entityTypes)
        {
            this.attributeName = CGlobal.Prefix() + attributeName;
            this.entityTypes = entityTypes;
        }

        public override string ToString()
            => attributeName;

        internal EEntityType[] EntityTypes()
            => entityTypes;
    }

    // If this attribtue is present on an EAttribute, it indicates that the EAttribute should be included in the columnset when fetching from dataverse
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class ADVReadAttribute : Attribute { }

    // If this attribute is present on an EAttribute, it indicates that the EAttribute should be written to dataverse when creating or updating the relevant entity.
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class ADVWriteAttribute : Attribute { }

    // If this attribute is present on an EAttribute, it indicates that when the relevant EAttribute is written to dataverse,
    // it should also be written to the specified location in duplicate form.
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    internal class ADVWriteDuplicate : Attribute
    {
        private EAttribute target; // the target attribute to which the duplicate value should be written
        private EEntityType[] types; // the entity types to which this duplication applies when writing (if empty, applies to all entity types)

        internal ADVWriteDuplicate(EAttribute target, params EEntityType[] types)
        {
            this.target = target;
            this.types = types;
        }

        internal EAttribute Target()
            => target;
        // returns true if the write duplicate rule should apply to the given entity
        internal bool AppliesTo(Entity entity)
            => AppliesTo(entity.EntityType());
        internal bool AppliesTo(EEntityType t)
            => !types.Any() || types.Contains(t);
    }
}
