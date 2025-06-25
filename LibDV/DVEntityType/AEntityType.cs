using LibDV.DVAttribute;
using LibUtil.UtilGlobal;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDV.DVEntityType
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    internal class AEntityAttribute : System.Attribute
    {
        private string friendlyName { get; }
        private string schemaName { get; }
        private string logicalName { get; }

        internal AEntityAttribute(string entityname)
        {
            friendlyName = entityname;
            schemaName = $"{CGlobal.Prefix()}{entityname.Replace(" ", "")}";
            logicalName = schemaName.ToLower();
        }

        internal string FriendlyName() => friendlyName;
        internal string SchemaName() => schemaName;
        internal string LogicalName() => logicalName;

        internal ColumnSet ColumnSet()
        {
            // Retrieve all attributes that are relevant to this entity
            var attrs = SAttribute.GetAttributes(SEntityType.EntityType(logicalName));
            // Returns a ColumnSet that contains all the attributes that are relevant to this entity.
            return new ColumnSet(attrs.Where(a => a.HasDVRead()).Select(a => a.LogicalName()).ToArray());
        }
        internal  QueryExpression QueryExpression()
            => new QueryExpression
            {
                EntityName = logicalName,
                ColumnSet = ColumnSet(),
                Orders = { new OrderExpression {
                            AttributeName = logicalName + "id", // Default ID attribute
                            OrderType = OrderType.Ascending // Default to ascending order
                         }}
            };
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class AEntityRelationshipAttribute : AEntityAttribute
    {
        // format is as follows: entA at entB (e.g. Clinician at Clinic)
        private EEntityType entA;
        private EEntityType entB;

        internal AEntityRelationshipAttribute(EEntityType entA, EEntityType entB)
            : base(EntityName(entA, entB)) // this assigns the appropriate logical, schema, and friendly names
        {
            
            this.entA = entA;
            this.entB = entB;
        }

        private static string EntityName(EEntityType a, EEntityType b)
            => a.FriendlyName() + " at " + b.FriendlyName(); // e.g. "Clinician at Clinic"

        internal EEntityType EntA() => entA;
        internal EEntityType EntB() => entB;
    }
}
