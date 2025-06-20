using LibDV.DVRelationship;
using LibUtil.UtilGlobal;
using Microsoft.Xrm.Sdk.Query;
using LibDV.DVAttribute;

namespace LibDV.DVEntityType
{

    #region entitytype
    public enum EEntityType
    {
        [AEntity("Clinician")]
        Clinician,
        [AEntity("CClinic")]
        Clinic,
        [AEntity("Medical Group")]
        MedicalGroup, // aka Organization

        [AEntity("Clinician at CClinic")]
        ClinicianAtClinic,
        [AEntity("Clinician at Medical Group")]
        ClinicianAtMedicalGroup,
        [AEntity("CClinic at Medical Group")]
        ClinicAtMedicalGroup,
    }
    #endregion entitytype

    #region entityattribute
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class AEntityAttribute : System.Attribute
    {
        internal string friendlyname { get; }
        internal string schemaname { get; }
        internal string logicalname { get; }

        internal AEntityAttribute(string entityname)
        {
            friendlyname = entityname;
            schemaname = $"{CGlobal.Prefix()}{entityname.Replace(" ", "")}";
            logicalname = schemaname.ToLower();
        }

        internal ColumnSet ColumnSet()
        {
            // Retrieve all attributes that are relevant to this entity
            var attrs = SAttribute.GetAttributes(SEntityType.EntityType(logicalname));
            // Returns a ColumnSet that contains all the attributes that are relevant to this entity.
            return new ColumnSet(attrs.Where(a => a.HasDVRead()).Select(a => a.Attribute()).ToArray());
        }
    }
    #endregion entityattribute

    #region entitytype_extension
    // this partial class contains extension methods for EEntityType
    public static partial class SEntityType
    {
        private enum NameType
        {
            Friendly,
            Schema,
            Logical
        }

        private static AEntityAttribute EntityAttribute(this EEntityType entityType)
        {
            var fieldInfo = entityType.GetType().GetField(entityType.ToString());
            var attributes = fieldInfo?.GetCustomAttributes(typeof(AEntityAttribute), false) as AEntityAttribute[];
            if (attributes is not null && attributes.Length > 0)
            {
                return attributes[0];
            }
            else
            {
                throw new Exception($"EEntityType {entityType} does not have an AEntityAttribute defined.");
            }
        }

        // Returns the friendly name of the entity
        public static string FriendlyName(this EEntityType entityType)
            => Name(entityType, NameType.Friendly);

        // Returns the schema name of the entity
        public static string SchemaName(this EEntityType entityType)
            => Name(entityType, NameType.Schema);

        // Returns the logical name of the entity
        public static string LogicalName(this EEntityType entityType)
            => Name(entityType, NameType.Logical);

        private static string Name(this EEntityType entityType, NameType nameType)
            => nameType switch
            {
                NameType.Friendly => entityType.EntityAttribute().friendlyname,
                NameType.Schema => entityType.EntityAttribute().schemaname,
                NameType.Logical => entityType.EntityAttribute().logicalname,
                _ => throw new Exception($"Unrecognized NameType: {nameType}")
            };

        // Returns a ColumnSet that contains all the attributes that are relevant to this entity type.
        public static ColumnSet ColumnSet(this EEntityType entityType)
            => entityType.EntityAttribute().ColumnSet();

        public static QueryExpression QueryExpression(this EEntityType entityType)
            => new QueryExpression
            {
                EntityName = entityType.LogicalName(),
                ColumnSet = entityType.ColumnSet(),
                Orders = { new OrderExpression {
                            AttributeName = entityType.IdAttribute().Attribute(), // Default ID attribute
                            OrderType = OrderType.Ascending // Default to ascending order
                         }}
            };

    }
    #endregion entitytype_extension
}
