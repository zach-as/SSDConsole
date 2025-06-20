using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using LibCMS.Data.Associable;
using LibDV.DVEntity;
using LibUtil.UtilGlobal;
using LibDV.DVRelationship;
using LibDV.DVAssociable;
using LibDV.DVEntityType;

namespace LibDV.DVRelationship
{
    internal enum DVRelationshipType
    {
        [Relationship(EEntityType.Clinician, EEntityType.Clinic)]
        ClinicianAtClinic,
        [Relationship(EEntityType.Clinician, EEntityType.MedicalGroup)]
        ClinicianAtMedicalGroup,
        [Relationship(EEntityType.Clinic, EEntityType.MedicalGroup)]
        ClinicAtMedicalGroup,
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class RelationshipAttribute : Attribute
    {
        private EEntityType a;
        private EEntityType b;
        private string friendlyname { get; }
        private string schemaname { get; }
        private string logicalname { get; }

        internal RelationshipAttribute(EEntityType a, EEntityType b)
        {
            this.a = a;
            this.b = b;
            friendlyname = $"{a.FriendlyName()} at {b.FriendlyName()}";
            schemaname = $"{CGlobal.Prefix()}{friendlyname.Replace(" ", "")}";
            logicalname = schemaname.ToLower();
        }

        internal QueryExpression QueryExpression()
            => new QueryExpression()
            {
                ColumnSet = ColumnSet(),
                EntityName = LogicalName(),
                Orders = { new OrderExpression()
                    {
                        AttributeName = a.LogicalName(),
                        OrderType = OrderType.Ascending,
                    } }
            };
        internal ColumnSet ColumnSet()
            => new ColumnSet(a.LogicalName(), b.LogicalName());
        internal EEntityType EntA() => a;
        internal EEntityType EntB() => b;
        internal string FriendlyName() => friendlyname;
        internal string SchemaName() => schemaname;
        internal string LogicalName() => logicalname;

    }
    
    internal static class DVRelationship
    {
        internal static RelationshipAttribute Attribute(this DVRelationshipType t)
        {
            var fieldInfo = t.GetType().GetField(t.ToString());
            var attributes = fieldInfo?.GetCustomAttributes(typeof(RelationshipAttribute), false) as RelationshipAttribute[];
            if (attributes is null) throw new Exception("DVRelationshipType.EAttribute(): No attribute found!");
            return attributes[0];
        }
        // Retrieve the friendly name of the relationship type table (e.g. Clinician At CClinic)
        internal static string FriendlyName(this DVRelationshipType t)
            => t.Attribute().FriendlyName();
        // Retrieve the schema name of the relationship type table (e.g. ssd_ClinicianatClinic)
        internal static string SchemaName(this DVRelationshipType t)
            => t.Attribute().SchemaName();
        // Retrieve the logical name of the relationship type table (e.g. ssd_clinicianatclinic)
        internal static string LogicalName(this DVRelationshipType t)
            => t.Attribute().LogicalName();
        // Retrieve the first entity of the relationship
        internal static EEntityType EntA(this DVRelationshipType t)
            => t.Attribute().EntA();
        // Retrieve the second entity of the relationship
        internal static EEntityType EntB(this DVRelationshipType t)
            => t.Attribute().EntB();
        // Retrieve the columnset containing the columns used in the provided relationship type
        internal static ColumnSet ColumnSet(this DVRelationshipType t)
            => t.Attribute().ColumnSet();
        // Retrieve a query expression used to identify all information for every relationship of the provided type
        internal static QueryExpression QueryExpression(this DVRelationshipType t)
            => t.Attribute().QueryExpression();
        // Retrieve a list of all relationship types
        internal static List<DVRelationshipType> RelationshipTypes()
            => new List<DVRelationshipType>()
            {
                DVRelationshipType.ClinicianAtClinic,
                DVRelationshipType.ClinicianAtMedicalGroup,
                DVRelationshipType.ClinicAtMedicalGroup
            };
        
        internal static DVRelationshipType RelationshipType(EEntityType a, EEntityType b)
        {
            if (a == b) throw new ArgumentException($"RelationshipType({a}, {b}): Entities of same type can not be related!");
            var types = RelationshipTypes();
            foreach ( var t in types )
            {
                var aMatch = a == t.EntA() || a == t.EntB();
                var bMatch = b == t.EntA() || b == t.EntB();
                if (aMatch && bMatch) return t;
            }
            throw new Exception($"RelationshipType({a}, {b}): Unable to find matching relationship type."); // this should never be reached
        }
        internal static DVRelationshipType RelationshipType(LibCMS.Data.Associable.CAssociable a, LibCMS.Data.Associable.CAssociable b)
            => RelationshipType(a.EntityType(), b.EntityType());
        internal static DVRelationshipType RelationshipType(Entity a, Entity b)
            => RelationshipType(a.EntityType(), b.EntityType());
        internal static DVRelationshipType RelationshipType(Entity relationshipEntity)
            => RelationshipTypes()
            .First(relType => relType.LogicalName() == relationshipEntity.LogicalName);
        
        // Creates a new Entity for the purpose of recording a relationship in DV
        internal static Entity NewRelationship(Entity a, Entity b)
        {
            var relType = RelationshipType(a.EntityType(), b.EntityType());
            var entity = new Entity(relType.LogicalName());
            
            // assign the reference value of A's col to A's id
            entity[a.LogicalName] = a.ToEntityReference();
            // assign the reference value of B's col to B's id
            entity[b.LogicalName] = b.ToEntityReference();

            return entity;
        }
        
        // This accepts three entities
        // relationship = the entity storing the relationship information
        // a = the first entity to check
        // b = the second entity to check
        // this function returns true if the relationship entity is of the appropriate relType and if its stored relationship info matches the given  types
        internal static bool RelationshipMatch(Entity relationship, Entity a, Entity b)
        {
            var relType = RelationshipType(a.EntityType(), b.EntityType());
            if (relationship.LogicalName != relType.LogicalName()) return false;

            var aRef = (EntityReference)relationship[a.LogicalName];
            var bRef = (EntityReference)relationship[b.LogicalName];

            var aMatch = aRef.LogicalName == a.LogicalName && aRef.Id == a.Id;
            var bMatch = bRef.LogicalName == b.LogicalName && bRef.Id == b.Id;

            return aMatch && bMatch;
        }
    }
}
