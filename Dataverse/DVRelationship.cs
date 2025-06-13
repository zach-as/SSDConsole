using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using SSDConsole.CMS.Data.Associable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SSDConsole.Dataverse
{
    internal enum DVRelationshipType
    {
        [Relationship(EntityType.Clinician, EntityType.Clinic)]
        ClinicianAtClinic,
        [Relationship(EntityType.Clinician, EntityType.MedicalGroup)]
        ClinicianAtMedicalGroup,
        [Relationship(EntityType.Clinic, EntityType.MedicalGroup)]
        ClinicAtMedicalGroup,
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class RelationshipAttribute : System.Attribute
    {
        private EntityType a;
        private EntityType b;
        private string friendlyname { get; }
        private string schemaname { get; }
        private string logicalname { get; }

        internal RelationshipAttribute(EntityType a, EntityType b)
        {
            this.a = a;
            this.b = b;
            friendlyname = $"{a.FriendlyName()} at {b.FriendlyName()}";
            schemaname = $"{Program.PREFIX}{friendlyname.Replace(" ", "")}";
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
        internal EntityType EntA() => a;
        internal EntityType EntB() => b;
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
            if (attributes is null) throw new Exception("DVRelationshipType.Attribute(): No attribute found!");
            return attributes[0];
        }
        // Retrieve the friendly name of the relationship type table (e.g. Clinician At Clinic)
        internal static string FriendlyName(this DVRelationshipType t)
            => t.Attribute().FriendlyName();
        // Retrieve the schema name of the relationship type table (e.g. ssd_ClinicianatClinic)
        internal static string SchemaName(this DVRelationshipType t)
            => t.Attribute().SchemaName();
        // Retrieve the logical name of the relationship type table (e.g. ssd_clinicianatclinic)
        internal static string LogicalName(this DVRelationshipType t)
            => t.Attribute().LogicalName();
        // Retrieve the first entity of the relationship
        internal static EntityType EntA(this DVRelationshipType t)
            => t.Attribute().EntA();
        // Retrieve the second entity of the relationship
        internal static EntityType EntB(this DVRelationshipType t)
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
        
        internal static DVRelationshipType RelationshipType(EntityType a, EntityType b)
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
        internal static DVRelationshipType RelationshipType(Associable a, Associable b)
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
            entity[relType.EntA().LogicalName()] = a.ToEntityReference();
            // assign the reference value of B's col to B's id
            entity[relType.EntB().LogicalName()] = b.ToEntityReference();
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
            
            var aId = a.Id;
            var bId = b.Id;

            var aName = a.LogicalName;
            var bName = b.LogicalName;

            var aMatch = relationship[aName].Equals(aId);
            var bMatch = relationship[bName].Equals(bId);

            return aMatch && bMatch;
        }
    }
}
