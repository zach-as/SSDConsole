using Microsoft.Xrm.Sdk;
using LibCMS.Data.Associable;
using LibUtil.UtilGlobal;
using LibDV.Associable;
using LibDV.DVEntity;
using LibDV.EntityType;
using System.Reflection.Metadata.Ecma335;

namespace LibDV.Attribute
{
    // This class contains static functions relevant to EAttribute that are used in the context of writing attributes to entities in Dataverse.
    internal static partial class SAttribute
    {

        #region applyattribute
        // Writes attributes to the provided Entity from the information within the provided CAssociable
        /*internal static void WriteAttributes(CAssociable a, Microsoft.Xrm.Sdk.Entity e)
        {
            var attributes = GetAttributes(a);
            foreach (var attribute in attributes)
            {
                if (attribute.HasDVWrite())
                    WriteAttribute(attribute, e, AttributeValue(a, attribute));
            }
        }

        // Attempts to write the value of the EAttribute to the entity; throws an error if invalid
        private static void WriteAttribute(EAttribute a, Microsoft.Xrm.Sdk.Entity e, object? value)
        {
            if (!CanWriteAttribute(a, e)) throw new Exception($"Unable to write to EAttribute {a} for entity of type {e.LogicalName}.");
            e[a.LogicalName()] = value; // Write the value to the entity's attribute
        }

        // Returns true if the EAttribute can be written to the entity
        private static bool CanWriteAttribute(EAttribute a, Microsoft.Xrm.Sdk.Entity e)
            => a.EntityTypes().Contains(e.EntityType());*/

        #endregion applyattribute
    }
}
