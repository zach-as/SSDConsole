using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ExpressiveAnnotations.Analysis;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using SSDConsole.CMS.Data.Associable;
using SSDConsole.CMS.Function;
using static SSDConsole.CMS.Function.Specialty;

namespace SSDConsole.Dataverse
{
    // This class contains extension methods for the Associable class, enabling for the more effective use of this class within the Dataverse framework
    internal static class DVAssociable
    {
        // Retrieves a filter expression which is used to perform comparisons against this object and entities.
        internal static FilterExpression EqualExpression(this Associable a)
            => DVFilter.EqualExpression(a);

        // Returns true if the entity's relevant attributes match this object.
        internal static bool Matches(this Associable a, Entity e)
            => DVFilter.Matches(a, e);

        // Returns true if the other associable's relevant attributes match this object.
        internal static bool Matches(this Associable a1, Associable a2)
            => DVFilter.Matches(a1, a2);

        // Retrieves the logical name of this associable as used in Dataverse.
        internal static string LogicalName(this Associable a)
            => DVEntity.LogicalName(a);

        // Returns a ColumnSet that contains all the attributes that are relevant to this object.
        internal static ColumnSet ColumnSet(this Associable a)
            => DVEntity.ColumnSet(a);

        // Returns the EntityType of the Associable object.
        internal static EntityType EntityType(this Associable a)
            => DVEntity.EntityType(a);

        // Adds this object's properties to the provided Entity.
        internal static void ApplyAttributes(this Associable a, Entity e)
            => DVAttribute.ApplyAttributes(a, e);

        internal static object? AttributeValue(this Associable a, string attrName)
            => DVAttribute.GetAttributeValue(a, attrName);

        internal static bool HasAttribute(this Associable a, string attrName)
            => DVAttribute.HasAttribute(a, attrName);

        internal static Dictionary<EntityType, List<Associable>> AssociationsByType(this Associable a)
            => SortByType(a.Associations());

        internal static Dictionary<EntityType, List<Associable>> SortByType(List<Associable> associables)
        {
            var sorted = new Dictionary<EntityType, List<Associable>>();
            foreach (var associable in associables)
            {
                var type = associable.EntityType();
                if (!sorted.ContainsKey(type))
                {
                    sorted[type] = new List<Associable>();
                }
                sorted[type].Add(associable);
            }
            return sorted;
        }
    }
}
