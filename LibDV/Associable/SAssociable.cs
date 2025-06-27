using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LibCMS.Data.Associable;
using Microsoft.Xrm.Sdk.Query;
using LibDV.Entity;
using LibDV.EntityType;
using LibDV.Attribute;

namespace LibDV.Associable
{
    public static class SAssociable
    {
        #region generic
        public static bool Equals(this CAssociable a, CAssociable b)
            => DVFilter.Matches(a,b);

        // Retrieves a filter expression which is used to perform comparisons against this object and entities.
        public static FilterExpression EqualExpression(this CAssociable a)
            => DVFilter.EqualExpression(a);

        // Returns true if the entity's relevant attributes match this object.
        public static bool Matches(this CAssociable a, Microsoft.Xrm.Sdk.Entity e)
            => DVFilter.Matches(a, e);

        // Retrieves the logical name of this associable as used in Dataverse.
        public static string LogicalName(this CAssociable a)
            => a.EntityType().LogicalName();

        // Returns a ColumnSet that contains all the attributes that are relevant to this object.
        public static ColumnSet ColumnSet(this CAssociable a)
            => a.EntityType().ColumnSet();

        // Adds this object's properties to the provided Entity.
        public static void ApplyAttributes(this CAssociable a, Microsoft.Xrm.Sdk.Entity e)
            => SAttribute.WriteAttributes(a, e);

        public static object? AttributeValue(this CAssociable a, string attrName)
            => SAttribute.AttributeValue(a, attrName);

        public static bool HasAttribute(this CAssociable a, string attrName)
            => SAttribute.HasAttribute(a, attrName);

        public static Dictionary<EEntityType, List<CAssociable>> AssociationsByType(this CAssociable a)
            => SortByType(a.Associations());

        public static Dictionary<EEntityType, List<CAssociable>> SortByType(List<CAssociable> associables)
        {
            var sorted = new Dictionary<EEntityType, List<CAssociable>>();
            foreach (var associable in associables)
            {
                var type = associable.EntityType();
                if (!sorted.ContainsKey(type))
                {
                    sorted[type] = new List<LibCMS.Data.Associable.CAssociable>();
                }
                sorted[type].Add(associable);
            }
            return sorted;
        }
        #endregion generic

        #region clinician
        public static OptionSetValue Sex(this CClinician c)
        {
            // code for male = 924040000, code for female = 924040001
            return new OptionSetValue(c.sex.Contains("M") ? 924040000 : 924040001);
        }
        #endregion clinician
    }
}
