using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using LibCMS.Data.Associable;
using LibDV.Entity;
using LibUtil.UtilGlobal;
using LibDV.Relationship;
using LibDV.Associable;
using LibDV.EntityType;
using LibUtil.UtilAttribute;

namespace LibDV.Relationship
{
    // The purpose of this enum is to act as an accessibility wrapper around EENtityTypes that use the AEntityRelationship attribute
    internal enum ERelationshipType
    {
        [ARelationship(EEntityType.ClinicianAtClinic)]
        ClinicianAtClinic,
        [ARelationship(EEntityType.ClinicianAtMedicalGroup)]
        ClinicianAtMedicalGroup,
        [ARelationship(EEntityType.ClinicAtMedicalGroup)]
        ClinicAtMedicalGroup,
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class ARelationshipAttribute : System.Attribute
    {
        private EEntityType type;
        private AEntityRelationshipAttribute internalAttribute;

        internal ARelationshipAttribute(EEntityType type)
        {
            this.type = type;
            internalAttribute = type.InternalAttribute<AEntityRelationshipAttribute>(); // this will except if type is not right
        }

        internal EEntityType EntA() => internalAttribute.EntA();
        internal EEntityType EntB() => internalAttribute.EntB();

        internal AEntityRelationshipAttribute EntityAttribute() => internalAttribute;

    }
    
    internal static partial class SRelationship
    {
        internal static ARelationshipAttribute RelationshipAttribute(this ERelationshipType t)
            => t.InternalAttribute<ARelationshipAttribute>();
        internal static AEntityRelationshipAttribute EntityAttribute(this ERelationshipType t)
            => t.RelationshipAttribute().EntityAttribute();

        // Retrieve the friendly name of the relationship type table (e.g. Clinician At CClinic)
        internal static string FriendlyName(this ERelationshipType t)
            => t.EntityAttribute().FriendlyName();
        // Retrieve the schema name of the relationship type table (e.g. ssd_ClinicianatClinic)
        internal static string SchemaName(this ERelationshipType t)
            => t.EntityAttribute().SchemaName();
        // Retrieve the logical name of the relationship type table (e.g. ssd_clinicianatclinic)
        internal static string LogicalName(this ERelationshipType t)
            => t.EntityAttribute().LogicalName();
        // Retrieve the first entity of the relationship
        internal static EEntityType EntA(this ERelationshipType t)
            => t.RelationshipAttribute().EntA();
        // Retrieve the second entity of the relationship
        internal static EEntityType EntB(this ERelationshipType t)
            => t.RelationshipAttribute().EntB();
        // Retrieve the columnset containing the columns used in the provided relationship type
        internal static ColumnSet ColumnSet(this ERelationshipType t)
            => t.EntityAttribute().ColumnSet();
        // Retrieve a query expression used to identify all information for every relationship of the provided type
        internal static QueryExpression QueryExpression(this ERelationshipType t)
            => t.EntityAttribute().QueryExpression();
        
    }
}
