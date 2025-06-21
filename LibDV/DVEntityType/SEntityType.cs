using LibCMS.Data.Associable;
using LibDV.DVAttribute;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDV.DVEntityType
{
    public static partial class SEntityType
    {
        private static EEntityType[] entityTypes = [];
        public static EEntityType[] EntityTypes()
        {
            if (entityTypes.Length == 0)
            {
                entityTypes = Enum.GetValues<EEntityType>();
            }
            return entityTypes;
        }

        // Retrieves an EEntityType from the provided logical name
        public static EEntityType EntityType(string logicalname)
        {
            if (string.IsNullOrEmpty(logicalname)) throw new ArgumentNullException(nameof(logicalname), "EEntityType cannot be determined from null or empty logical name.");
            var entityTypes = EntityTypes();
            var matchingType = entityTypes.Where(t => t.LogicalName().Equals(logicalname));
            if (matchingType.Any()) return matchingType.First();
            throw new Exception($"Unrecognized logicalName: {logicalname}");
        }

        #region extension
        public static EEntityType EntityType(this Entity e)
            => EntityType(e.LogicalName);
        public static EEntityType EntityType(this EntityReference er)
            => EntityType(er.LogicalName);
        public static EEntityType EntityType(this CAssociable a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a), "Cannot determine EEntityType from null CAssociable");
            if (a is CClinician) return EEntityType.Clinician;
            if (a is CClinic) return EEntityType.Clinic;
            if (a is CMedicalGroup) return EEntityType.MedicalGroup;
            throw new ArgumentException("Unknown CAssociable type", nameof(a));
        }
        #endregion extension
    }
}
