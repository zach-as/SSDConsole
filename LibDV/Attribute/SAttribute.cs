using LibCMS.Data.Associable;
using LibDV.Associable;
using LibDV.EntityType;
using LibUtil.UtilAttribute;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDV.Attribute
{
    // Partial class of SAttribute, contains all functions related to accessing attributes
    internal static partial class SAttribute
    {
        #region getattribute
        // this exists so that we don't have to keep using reflection whenever we want to access GetAttributes()
        private static List<EAttribute>? attributes;

        internal static List<EAttribute> GetAttributes()
        {
            // if attributes is not initialized, initialize with reflection
            if (attributes is null) attributes = Enum.GetValues<EAttribute>().ToList();
            return attributes;
        }

        internal static List<EAttribute> GetAttributes(EEntityType entityType)
            => GetAttributes().Where(a => a.EntityTypes().Contains(entityType)).ToList();
        internal static List<EAttribute> GetAttributes(CAssociable a)
            => GetAttributes(a.EntityType());
        internal static List<EAttribute> GetAttributes(Entity e)
            => GetAttributes(e.EntityType());

        internal static EAttribute GetAttribute(string attrName)
        {
            try
            {
                return GetAttributes().First(a => a.LogicalName().Equals(attrName));
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"EAttribute with name '{attrName}' not found.", nameof(attrName));
            }
        }
        #endregion getattribute
    }
}
