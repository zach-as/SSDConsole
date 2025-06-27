using LibDV.EntityType;
using LibUtil.UtilAttribute;

namespace LibDV.Attribute
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class AAttributeAttribute : System.Attribute
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
    internal class ADVReadAttribute : System.Attribute { }

    // If this attribute is present on an EAttribute, it indicates that the EAttribute should be written to dataverse when creating or updating the relevant entity.
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class ADVWriteAttribute : System.Attribute { }
}
