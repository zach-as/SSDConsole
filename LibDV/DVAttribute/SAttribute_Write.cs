using Microsoft.Xrm.Sdk;
using LibCMS.Data.Associable;
using LibUtil.UtilGlobal;
using LibDV.DVAssociable;
using LibDV.DVEntity;
using LibDV.DVEntityType;
using System.Reflection.Metadata.Ecma335;

namespace LibDV.DVAttribute
{
    // This class contains static functions relevant to EAttribute that are used in the context of writing attributes to entities in Dataverse.
    public static partial class SAttribute
    {

        #region applyattribute
        // Writes attributes to the provided Entity from the information within the provided CAssociable
        internal static void WriteAttributes(CAssociable a, Entity e)
        {
            var attributes = GetAttributes(a);
            foreach (var attribute in attributes)
            {
                if (attribute.HasDVWrite())
                    WriteAttribute(attribute, e, GetAttributeValue(a, attribute));
            }
        }

        // Attempts to write the value of the EAttribute to the entity; throws an error if invalid
        private static void WriteAttribute(EAttribute a, Entity e, object? value)
        {
            if (!CanWriteAttribute(a, e)) throw new Exception($"Unable to write to EAttribute {a} for entity of type {e.LogicalName}.");
            e[a.Attribute()] = value; // Write the value to the entity's attribute
            TryWriteDuplicate(a, e, value); // Attempt to write duplicate if applicable
        }

        // Attempts to write duplicate values if relevant (safe, will not except)
        private static void TryWriteDuplicate(EAttribute a, Entity e, object? value)
        {
            if (!a.HasDVWriteDuplicate()) return; // only write duplicate if duplicate System.Attribute is present on the EAttribute

            var dup = a.DVWriteDuplicate(); // access the internal System.Attribute (I know, overloaded term...)
            if (!dup.AppliesTo(e.EntityType())) return; // only write duplicate if the EAttribute should be duplicated for an entity of this type
            
            // Write the value to the duplicate attribute target
            WriteAttribute(dup.Target(), e, value);
        }

        // Returns true if the EAttribute can be written to the entity
        private static bool CanWriteAttribute(EAttribute a, Entity e)
            => a.EntityTypes().Contains(e.EntityType());

        #endregion applyattribute
    }
}
