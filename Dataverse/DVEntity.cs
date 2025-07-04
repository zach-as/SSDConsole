﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using SSDConsole.CMS.Data.Associable;
using SSDConsole.CMS.Function;
using static SSDConsole.CMS.Function.Specialty;

namespace SSDConsole.Dataverse
{
    internal static class DVEntity
    {
        internal static EntityType[] EntityTypes()
            => [
                Dataverse.EntityType.Clinic,
                Dataverse.EntityType.MedicalGroup,
                Dataverse.EntityType.Clinician
                ];

    // Returns a ColumnSet that contains all the attributes that are relevant to this object.
    internal static ColumnSet ColumnSet(Associable a)
        {
            if (a is null) return new ColumnSet();
            return a.EntityType().ColumnSet();
        }

        internal static string LogicalName(Associable a)
        {
            return a.EntityType().LogicalName();
        }

        internal static EntityCollection EntityCollection(this List<Entity>? entities)
        {
            if (entities == null) return new EntityCollection();
            if (entities.Count == 0) return new EntityCollection();
            return new EntityCollection(entities)
            {
                EntityName = entities.ElementAt(0).LogicalName
            };
        }

        internal static EntityType EntityType(this Entity e)
            => EntityType(e.LogicalName);
        internal static EntityType EntityType(this EntityReference er)
            => EntityType(er.LogicalName);
        internal static EntityType EntityType(string logicalname)
        {
            if (string.IsNullOrEmpty(logicalname)) throw new ArgumentNullException(nameof(logicalname), "EntityType cannot be determined from null or empty logical name");
            var entityTypes = EntityTypes();
            
            foreach (EntityType t in entityTypes)
            {
                if (t.LogicalName().Equals(logicalname)) return t;
            }

            throw new Exception($"Unrecognized logicalname: {logicalname}");
        }

        internal static object? AttributeValue(this Entity e, string attrName)
            => e.Attributes.TryGetValue(attrName, out var value) ? value : null;
        internal static bool HasAttribute(this Entity e, string attrName)
            => e.Attributes.ContainsKey(attrName);
        
        internal static bool Matches(this Entity e1, Entity? e2)
        {
            if (e1 == null || e2 == null) return false;
            if (e1.LogicalName != e2.LogicalName) return false;
            if (e1.Id != e2.Id) return false;
            // Check if all attributes match
            foreach (var attr in e1.Attributes)
            {
                if (!e2.Attributes.TryGetValue(attr.Key, out var value) || !object.Equals(attr.Value, value))
                {
                    return false;
                }
            }
            // Check if e2 has any additional attributes that e1 does not have
            foreach (var attr in e2.Attributes)
            {
                if (!e1.Attributes.ContainsKey(attr.Key))
                {
                    return false;
                }
            }
            return true;
        }
        
        
    }

    #region entityattribute
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class EntityAttribute : System.Attribute
    {
        internal string friendlyname { get; }
        internal string schemaname { get; }
        internal string logicalname { get; }

        internal EntityAttribute(string entityname)
        {
            friendlyname = entityname;
            schemaname = $"{Program.PREFIX}{entityname.Replace(" ","")}";
            logicalname = schemaname.ToLower();
        }

        internal ColumnSet ColumnSet()
        {
            // Retrieve all attributes that are relevant to this entity
            var attrs = DVAttribute.GetAttributes(DVEntity.EntityType(logicalname));
            // Returns a ColumnSet that contains all the attributes that are relevant to this entity.
            return new ColumnSet(attrs.Where(a => a.UseInColumnSet()).Select(a => a.Attribute()).ToArray());
        }
    }
    #endregion entityattribute

    #region entitytype
    internal enum EntityType
    {
        [Entity("Clinician")]
        Clinician = 0,
        [Entity("Clinic")]
        Clinic = 1,
        [Entity("Medical Group")]
        MedicalGroup = 2, // aka Organization
    }

    internal static class EntityType_Extension
    {
        private enum NameType
        {
            Friendly,
            Schema,
            Logical
        }

        // Returns the friendly name of the entity
        internal static string FriendlyName(this EntityType entityType)
            => Name(entityType, NameType.Friendly);

        // Returns the schema name of the entity
        internal static string SchemaName(this EntityType entityType)
            => Name(entityType, NameType.Schema);

        // Returns the logical name of the entity
        internal static string LogicalName(this EntityType entityType)
            => Name(entityType, NameType.Logical);

        private static string Name(EntityType entityType, NameType nameType)
        {
            var fieldInfo = entityType.GetType().GetField(entityType.ToString());
            var attributes = fieldInfo?.GetCustomAttributes(typeof(EntityAttribute), false) as EntityAttribute[];
            if (attributes is not null && attributes.Length > 0)
            {
                switch (nameType)
                {
                    case NameType.Friendly:
                        return attributes[0].friendlyname;
                    case NameType.Schema:
                        return attributes[0].schemaname;
                    case NameType.Logical:
                        return attributes[0].logicalname;
                    default:
                        throw new Exception($"Unknown NameType: {nameType} for EntityType: {entityType}");
                }
            } else
            {
                return entityType.ToString();
            }
        }
        // Returns a ColumnSet that contains all the attributes that are relevant to this entity type.
        internal static ColumnSet ColumnSet(this EntityType entityType)
        {
            var fieldInfo = entityType.GetType().GetField(entityType.ToString());
            var attributes = fieldInfo?.GetCustomAttributes(typeof(EntityAttribute), false) as EntityAttribute[];
            return attributes is not null && attributes.Length > 0 ? attributes[0].ColumnSet() : new ColumnSet();
        }

        private static OrderExpression OrderExpression(this EntityType entityType)
        {
            return new OrderExpression
            {
                AttributeName = entityType.IdAttribute().Attribute(), // Default ID attribute
                OrderType = OrderType.Ascending // Default to ascending order
            };
        }

        internal static QueryExpression QueryExpression(this EntityType entityType)
        {
            return new QueryExpression
            {
                EntityName = entityType.LogicalName(),
                ColumnSet = entityType.ColumnSet(),
                Orders = { { entityType.OrderExpression() }  }
            };
        }

        // Returns true if *this* EntityType should go before the provided EntityType, sorted through the enum initialization
        internal static bool GoesBefore(this EntityType a, EntityType b)
            => a < b;

        internal static Attribute IdAttribute(this EntityType e)
        {
            return e switch
            {
                EntityType.Clinician => Attribute.ClinicianID,
                EntityType.Clinic => Attribute.ClinicID,
                EntityType.MedicalGroup => Attribute.MedicalGroupID,
                _ => throw new ArgumentException($"Unknown EntityType in IdAttribute: {e}", nameof(e))
            };
        }
    }
    #endregion entitytype
}
