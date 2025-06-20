using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LibCMS.Data.Associable;
using Microsoft.Xrm.Sdk.Query;

namespace LibDV.DVAssociable
{
    public static class Associable_Extension
    {
        #region generic
        public static bool Equals(this Associable a, Associable b)
            => DVFilter.Matches(a,b);

        // Retrieves a filter expression which is used to perform comparisons against this object and entities.
        public static FilterExpression EqualExpression(this Associable a)
            => DVFilter.EqualExpression(a);

        // Returns true if the entity's relevant attributes match this object.
        public static bool Matches(this Associable a, Entity e)
            => DVFilter.Matches(a, e);

        // Retrieves the logical name of this associable as used in Dataverse.
        public static string LogicalName(this Associable a)
            => DVEntity.LogicalName(a);

        // Returns a ColumnSet that contains all the attributes that are relevant to this object.
        public static ColumnSet ColumnSet(this Associable a)
            => DVEntity.ColumnSet(a);

        // Returns the EEntityType of the Associable object.
        public static EntityType EntityType(this Associable a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a), "Cannot determine EEntityType from null Associable");
            if (a is Clinician) return Dataverse.EntityType.Clinician;
            if (a is Clinic) return Dataverse.EntityType.Clinic;
            if (a is Organization) return Dataverse.EntityType.MedicalGroup;
            throw new ArgumentException("Unknown Associable type", nameof(a));
        }

        // Adds this object's properties to the provided Entity.
        public static void ApplyAttributes(this Associable a, Entity e)
            => DVAttribute.ApplyAttributes(a, e);

        public static object? AttributeValue(this Associable a, string attrName)
            => DVAttribute.GetAttributeValue(a, attrName);

        public static bool HasAttribute(this Associable a, string attrName)
            => DVAttribute.HasAttribute(a, attrName);

        public static Dictionary<EntityType, List<Associable>> AssociationsByType(this Associable a)
            => SortByType(a.Associations());

        public static Dictionary<EntityType, List<Associable>> SortByType(List<Associable> associables)
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
        #endregion generic

        #region clinician
        public static OptionSetValue Sex(this Clinician c)
        {
            // code for male = 924040000, code for female = 924040001
            return new OptionSetValue(c.sex.Contains("M") ? 924040000 : 924040001);
        }
        #endregion clinician
    }
}
