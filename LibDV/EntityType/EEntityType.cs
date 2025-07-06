using LibDV.Relationship;
using LibUtil.UtilGlobal;
using Microsoft.Xrm.Sdk.Query;
using LibDV.Attribute;
using LibUtil.UtilAttribute;

namespace LibDV.EntityType
{

    #region entitytype
    public enum EEntityType
    {
        [AEntity("Clinician")]
        Clinician,
        [AEntity("Clinic")]
        Clinic,
        [AEntity("Medical Group")]
        MedicalGroup,

        [AEntityRelationship(Clinician, Clinic)]
        ClinicianAtClinic,
        [AEntityRelationship(Clinician, MedicalGroup)]
        ClinicianAtMedicalGroup,
        [AEntityRelationship(Clinic, MedicalGroup)]
        ClinicAtMedicalGroup,
    }
    #endregion entitytype

    

    #region entitytype_extension
    // this partial class contains extension methods for EEntityType
    public static partial class SEntityType
    {
        private static AEntityAttribute EntityAttribute(this EEntityType entityType)
            => entityType.InternalAttribute<AEntityAttribute>();

        // Returns the friendly name of the entity
        public static string FriendlyName(this EEntityType entityType)
            => entityType.EntityAttribute().FriendlyName();

        // Returns the schema name of the entity
        public static string SchemaName(this EEntityType entityType)
            => entityType.EntityAttribute().SchemaName();

        // Returns the logical name of the entity
        public static string LogicalName(this EEntityType entityType)
            => entityType.EntityAttribute().LogicalName();

        // Returns a ColumnSet that contains all the attributes that are relevant to this entity type.
        public static ColumnSet ColumnSet(this EEntityType entityType)
        {
            // Retrieve all attributes that are relevant to this entity
            var attrs = SAttribute.GetAttributes(entityType);
            // Returns a ColumnSet that contains all the attributes that are relevant to this entity.
            return new ColumnSet(attrs.Where(a => a.HasDVRead()).Select(a => a.LogicalName()).ToArray());
        }

        // Returns a basic QueryExpression that will return all occurences of the EEntityType with the specified columns in ColumnSet()
        public static QueryExpression QueryExpression(this EEntityType entityType)
            => new QueryExpression
            {
                EntityName = entityType.LogicalName(),
                ColumnSet = entityType.ColumnSet(),
                Orders = { new OrderExpression {
                            AttributeName = entityType.LogicalName() + "id", // Default ID attribute
                            OrderType = OrderType.Ascending // Default to ascending order
                         }}
            };

        public static bool IsRelationship(this EEntityType entityType)
            => entityType.EntityAttribute() is AEntityRelationshipAttribute;

    }
    #endregion entitytype_extension
}
