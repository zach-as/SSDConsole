using LibCMS.Data.Associable;
using LibDV.DVAssociable;
using LibDV.DVEntityType;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDV.DVAttribute
{
    // Partial class of SAttribute, contains all functions related to accessing attributes
    internal static partial class SAttribute
    {
        #region getattribute
        // this exists so that we don't have to keep using reflection whenever we want to access GetAttributes()
        private static List<EAttribute> attributes;

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
                return GetAttributes().First(a => a.Attribute().Equals(attrName));
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"EAttribute with name '{attrName}' not found.", nameof(attrName));
            }
        }
        #endregion getattribute

        #region attributevalue

        internal static object? AttributeValue(CAssociable a, EAttribute attribute)
        {
            if (a is CClinic clinic) return AttributeValue(clinic, attribute);
            if (a is CClinician clinician) return AttributeValue(clinician, attribute);
            if (a is CMedicalGroup organization) return AttributeValue(organization, attribute);
            throw new NotImplementedException($"No attribute retrieval defined for {a.GetType().Name}");
        }
        internal static object? AttributeValue(CAssociable a, string attrName)
            => AttributeValue(a, GetAttribute(attrName));

        private static object? AttributeValue(CClinic c, EAttribute a)
        {
            if (!a.EntityTypes().Contains(c.EntityType())) throw new Exception($"AttributeValue(): EAttribute {a} does not apply to CClinic entity type.");
            return a switch
            {
                EAttribute.AddressID => c.location.addressID,
                EAttribute.AddressLine1 => c.location.addressLine1,
                EAttribute.AddressLine2 => c.location.addressLine2,
                EAttribute.Line2Suppressed => c.location.line2Suppressed,
                EAttribute.City => c.location.city,
                EAttribute.Zip => c.location.zip,
                EAttribute.PhoneNumber => c.telephoneNumber,
                EAttribute.Name => c.name,
                EAttribute.ClinicianCount => c.numClinicians,
                EAttribute.PrimarySpecialties => c.PrimarySpecialtyCodes(),
                EAttribute.SecondarySpecialties => c.SecondarySpecialtyCodes(),
                _ => null,
            };
        }

        private static object? AttributeValue(CClinician c, EAttribute a)
        {
            if (!a.EntityTypes().Contains(c.EntityType())) throw new Exception($"AttributeValue(): EAttribute {a} does not apply to CClinic entity type.");
            return a switch
            {
                EAttribute.Pac => c.PacID,
                EAttribute.Npi => c.NPI,
                EAttribute.Enrl => c.EnrlID,
                EAttribute.FirstName => c.FirstName,
                EAttribute.MiddleName => c.MiddleName,
                EAttribute.LastName => c.LastName,
                EAttribute.PrimarySpecialty => c.PrimarySpecialtyCode(),
                EAttribute.SecondarySpecialties => c.SecondarySpecialtyCodes(),
                EAttribute.Suffix => c.Suffix,
                EAttribute.Sex => c.Sex(),
                EAttribute.Credentials => c.Credentials,
                EAttribute.MedicalSchool => c.MedicalSchool,
                EAttribute.GraduationYear => c.GraduationYear,
                EAttribute.Telehealth => c.Telehealth,
                EAttribute.FullMedicare => c.AcceptsFullMedicare,
                _ => null
            };
        }

        private static object? AttributeValue(CMedicalGroup o, EAttribute a)
        {
            if (!a.EntityTypes().Contains(o.EntityType())) throw new Exception($"AttributeValue(): EAttribute {a} does not apply to CClinic entity type.");
            return a switch
            {
                EAttribute.Pac => o.pac,
                EAttribute.Name => o.pac,
                EAttribute.PrimarySpecialties => o.PrimarySpecialtyCodes(),
                EAttribute.SecondarySpecialties => o.SecondarySpecialtyCodes(),
                EAttribute.FullMedicare => o.acceptsFullMedicare,
                EAttribute.ClinicianCount => o.numClinicians,
                _ => null
            };
        }
        #endregion attributevalue

        #region hasattribute
        internal static bool HasAttribute(this CAssociable a, string attrName)
            => HasAttribute(a, GetAttribute(attrName));
        internal static bool HasAttribute(this CAssociable a, EAttribute attr)
        {
            if (attr.EntityTypes().Contains(a.EntityType())) return AttributeValue(a, attr) != null;
            return false;
        }
        internal static bool HasAttribute(this Entity e, EAttribute attr)
            => e.HasAttribute(attr.Attribute());

        internal static bool HasAttribute(this Entity e, string attrName)
            => e.Attributes.ContainsKey(attrName);
        internal static object? AttributeValue(this Entity e, string attrName)
            => e.Attributes.TryGetValue(attrName, out var value) ? value : null;
        #endregion hasattribute

        #region idattribute
        internal static EAttribute IdAttribute(this EntityReference er)
            => IdAttribute(er.EntityType());
        internal static EAttribute IdAttribute(this Entity e)
            => IdAttribute(e.EntityType());
        internal static EAttribute IdAttribute(this CAssociable a)
            => IdAttribute(a.EntityType());
        internal static EAttribute IdAttribute(this EEntityType entityType)
            => GetAttribute(entityType.LogicalName() + "id");
        #endregion idattribute
    }
}
