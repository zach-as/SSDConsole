using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using SSDConsole.CMS.Data.Associable;
using SSDConsole.CMS.Function;
using static SSDConsole.CMS.Function.Specialty;

namespace SSDConsole.Dataverse
{
    internal static class DVAttribute
    {

        #region applyattribute
        internal static void ApplyAttributes(Associable a, Entity e)
        {
            var attributes = GetAttributes(a);
            foreach (var attribute in attributes)
            {
                if (attribute.WriteToDV()) ApplyAttribute(attribute, e, a);
            }
        }
        private static void ApplyAttribute(Attribute attr, Entity e, Associable a)
            => ApplyAttribute(attr, e, GetAttributeValue(a, attr));

        private static void ApplyAttribute(Attribute a, Entity e, object? value)
        {
            e[a.Attribute()] = value;

            // jank-ass logic to make the ssd_name field in MedicalGroup = Pac
            if (e.EntityType() == EntityType.MedicalGroup)
                if (a == Attribute.Pac)
                    e["ssd_name"] = value;
        }
        #endregion applyattribute

        #region getattribute
        private static Dictionary<EntityType, Attribute[]>? attributes;
        internal static Attribute[] GetAttributes(EntityType entityType)
        {
            if (attributes is not null) return attributes[entityType];
            attributes = new Dictionary<EntityType, Attribute[]>();
            var allAttributes = GetAttributes();
            foreach (Attribute a in allAttributes)
            {
                foreach (EntityType et in a.EntityTypes())
                {
                    if (!attributes.ContainsKey(et))
                    {
                        attributes[et] = new Attribute[] { a };
                    }
                    else
                    {
                        var existingAttributes = attributes[et].ToList();
                        existingAttributes.Add(a);
                        attributes[et] = existingAttributes.ToArray();
                    }
                }
            }
            return attributes[entityType];
        }
        internal static Attribute[] GetAttributes()
            => Enum.GetValues(typeof(Attribute)).Cast<Attribute>().ToArray();
        internal static Attribute[] GetAttributes(Associable a)
            => GetAttributes(a.EntityType());
        internal static Attribute[] GetAttributes(Entity e)
            => GetAttributes(e.EntityType());

        internal static Attribute GetAttribute(string attrName)
        {
            try
            {
                return GetAttributes().First(a => a.Attribute().Equals(attrName));
            } catch (Exception ex)
            {
                throw new ArgumentException($"Attribute with name '{attrName}' not found.", nameof(attrName));
            }
        }
        #endregion getattribute

        #region getattributevalue

        internal static object? GetAttributeValue(Associable a, Attribute attribute)
        {
            if (a is Clinic clinic) return GetAttributeValue(clinic, attribute);
            if (a is Clinician clinician) return GetAttributeValue(clinician, attribute);
            if (a is Organization organization) return GetAttributeValue(organization, attribute);
            throw new NotImplementedException($"No attribute retrieval defined for {a.GetType().Name}");
        }
        internal static object? GetAttributeValue(Associable a, string attrName)
            => GetAttributeValue(a, GetAttribute(attrName));

        private static object? GetAttributeValue(Clinic c, Attribute a)
        {
            if (!a.EntityTypes().Contains(c.EntityType())) throw new Exception($"GetAttributeValue(): Attribute {a} does not apply to Clinic entity type.");
            return a switch
            {
                Attribute.AddressID => c.Location.AddressID,
                Attribute.AddressLine1 => c.Location.AddressLine1,
                Attribute.AddressLine2 => c.Location.AddressLine2,
                Attribute.Line2Suppressed => c.Location.Line2Suppressed,
                Attribute.City => c.Location.City,
                Attribute.Zip => c.Location.ZIP,
                Attribute.PhoneNumber => c.TelephoneNumber,
                Attribute.Name => c.Name,
                Attribute.ClinicianCount => c.NumberClinicians,
                Attribute.PrimarySpecialties => c.PrimarySpecialtyCodes(),
                Attribute.SecondarySpecialties => c.SecondarySpecialtyCodes(),
                _ => null,
            };
        }

        private static object? GetAttributeValue(Clinician c, Attribute a)
        {
            if (!a.EntityTypes().Contains(c.EntityType())) throw new Exception($"GetAttributeValue(): Attribute {a} does not apply to Clinic entity type.");
            return a switch
            { 
                Attribute.Pac => c.PacID,
                Attribute.Npi => c.NPI,
                Attribute.Enrl => c.EnrlID,
                Attribute.FirstName => c.FirstName,
                Attribute.MiddleName => c.MiddleName,
                Attribute.LastName => c.LastName,
                Attribute.PrimarySpecialty => c.PrimarySpecialtyCode(),
                Attribute.SecondarySpecialties => c.SecondarySpecialtyCodes(),
                Attribute.Suffix => c.Suffix,
                Attribute.Sex => c.Sex(),
                Attribute.Credentials => c.Credentials,
                Attribute.MedicalSchool => c.MedicalSchool,
                Attribute.GraduationYear => c.GraduationYear,
                Attribute.Telehealth => c.Telehealth,
                Attribute.FullMedicare => c.AcceptsFullMedicare,
                _ => null
            };
        }

        private static object? GetAttributeValue(Organization o, Attribute a)
        {
            if (!a.EntityTypes().Contains(o.EntityType())) throw new Exception($"GetAttributeValue(): Attribute {a} does not apply to Clinic entity type.");
            return a switch {
                Attribute.Pac => o.PacID,
                Attribute.PartitionId => o.PacID,
                Attribute.PrimarySpecialties => o.PrimarySpecialtyCodes(),
                Attribute.SecondarySpecialties => o.SecondarySpecialtyCodes(),
                Attribute.FullMedicare => o.AcceptsFullMedicare,
                Attribute.ClinicianCount => o.NumberClinicians,
                _ => null
            };
        }
        #endregion getattributevalue

        internal static bool HasAttribute(Associable a, string attrName)
            => HasAttribute(a, GetAttribute(attrName));
        internal static bool HasAttribute(Associable a, Attribute attr)
        {
            if (attr.EntityTypes().Contains(a.EntityType())) return GetAttributeValue(a, attr) != null;
            return false;
        }

        internal static Attribute IdAttribute(this EntityReference er)
            => IdAttribute(er.EntityType());
        internal static Attribute IdAttribute(this Entity e)
            => IdAttribute(e.EntityType());
        internal static Attribute IdAttribute(this Associable a)
            => IdAttribute(a.EntityType());
        internal static Attribute IdAttribute(EntityType entityType)
            => entityType.IdAttribute();
    }

    #region attributedef
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class DVAttributeAttribute : System.Attribute
    {
        private bool usedInColumnSet; // if this attribute should be included when generating a columnset for fetching from DV
        private bool writeToDV; // if this attribute should be written to dataverse when creating or updating an entity (some attributes are created in DV, this should be false if that is the case)
        private string attributeName; // the name of this attribute as used in dataverse
        private EntityType[] entityTypes; // the entity types that this attribute applies to

        internal DVAttributeAttribute(bool usedInColumnSet, bool writeToDV, string attributeName, params EntityType[] entityTypes)
        {
            this.attributeName = Program.PREFIX + attributeName;
            this.entityTypes = entityTypes;
            this.usedInColumnSet = usedInColumnSet;
            this.writeToDV = writeToDV;
        }

        internal bool UseInColumnSet()
            => usedInColumnSet;

        internal EntityType[] EntityTypes()
            => entityTypes;

        internal bool WriteToDV()
            => writeToDV;

        // Returns the string representation of the attribute
        public override string ToString()
        {
            return attributeName;
        }
    }

    internal enum Attribute
    {
        // This represents the partition attribute, used in elastic tables
        [DVAttribute(false, true, "partitionid", EntityType.Clinic, EntityType.Clinician, EntityType.MedicalGroup)]
        PartitionId,

        // These are the attributes used in comparison operations
        [DVAttribute(true, true, "addressid", EntityType.Clinic)]
        AddressID,
        [DVAttribute(true, true, "addressline1", EntityType.Clinic)]
        AddressLine1,
        [DVAttribute(true, true, "addressline2", EntityType.Clinic)]
        AddressLine2,
        [DVAttribute(true, true, "addressline2suppressed", EntityType.Clinic)]
        Line2Suppressed,
        [DVAttribute(true, true, "name", EntityType.Clinic)]
        Name,
        [DVAttribute(true, true, "pac", EntityType.Clinician, EntityType.MedicalGroup)]
        Pac,

        // These attributes are not set within this program, but rather are generated in dataverse
        [DVAttribute(false, false, "clinicianid", EntityType.Clinician)]
        ClinicianID,
        [DVAttribute(false, false, "medicalgroupid", EntityType.MedicalGroup)]
        MedicalGroupID,
        [DVAttribute(false, false, "clinicid", EntityType.Clinic)]
        ClinicID,

        // These are the remaining attributes that are not used in comparison operations
        [DVAttribute(false, true, "phonenumber", EntityType.Clinic)]
        PhoneNumber,
        [DVAttribute(false, true, "city", EntityType.Clinic)]
        City,
        [DVAttribute(false, true, "zip", EntityType.Clinic)]
        Zip,
        [DVAttribute(false, true, "npi", EntityType.Clinician)]
        Npi,
        [DVAttribute(false, true, "enrl", EntityType.Clinician)]
        Enrl,
        [DVAttribute(false, true, "firstname", EntityType.Clinician)]
        FirstName,
        [DVAttribute(false, true, "middlename", EntityType.Clinician)]
        MiddleName,
        [DVAttribute(false, true, "lastname", EntityType.Clinician)]
        LastName,
        [DVAttribute(false, true, "primaryspecialty", EntityType.Clinician)]
        PrimarySpecialty,
        [DVAttribute(false, true, "suffix", EntityType.Clinician)]
        Suffix,
        [DVAttribute(false, true, "sex", EntityType.Clinician)]
        Sex,
        [DVAttribute(false, true, "credentials", EntityType.Clinician)]
        Credentials,
        [DVAttribute(false, true, "medicalschool", EntityType.Clinician)]
        MedicalSchool,
        [DVAttribute(false, true, "graduationyear", EntityType.Clinician)]
        GraduationYear,
        [DVAttribute(false, true, "telehealth", EntityType.Clinician)]
        Telehealth,
        [DVAttribute(false, true, "fullmedicare", EntityType.Clinician, EntityType.MedicalGroup)]
        FullMedicare,
        [DVAttribute(false, true, "primaryspecialties", EntityType.Clinic, EntityType.MedicalGroup)]
        PrimarySpecialties,
        [DVAttribute(false, true, "secondaryspecialties", EntityType.Clinic, EntityType.Clinician, EntityType.MedicalGroup)]
        SecondarySpecialties,
        [DVAttribute(false, true, "cliniciancount", EntityType.Clinic, EntityType.MedicalGroup)]
        ClinicianCount,
    }
    #endregion attributedef

    #region attributeextension
    internal static class Attribute_Extension
    {
        // Returns the string representation of the attribute
        internal static string Attribute(this Attribute a)
        {
            var internalAttribute = a.InternalAttribute();
            return internalAttribute is not null ?
                internalAttribute.ToString()
                : a.ToString();
        }

        internal static EntityType[] EntityTypes(this Attribute a)
        {
            var internalAttribute = a.InternalAttribute();
            return internalAttribute is not null ?
                internalAttribute.EntityTypes()
                : new EntityType[] { };
        }

        internal static bool UseInColumnSet(this Attribute a)
        {
            var internalAttribute = a.InternalAttribute();
            return internalAttribute is not null ?
                internalAttribute.UseInColumnSet()
                : false;
        }

        internal static bool WriteToDV(this Attribute a)
        {
            var internalAttribute = a.InternalAttribute();
            return internalAttribute is not null ?
                internalAttribute.WriteToDV()
                : false;
        }

        private static DVAttributeAttribute? InternalAttribute(this Attribute a)
        {
            var fieldInfo = a.GetType().GetField(a.ToString());
            var attributes = fieldInfo?.GetCustomAttributes(typeof(DVAttributeAttribute), false) as DVAttributeAttribute[];
            return attributes is not null && attributes.Length > 0 ?
                attributes[0]
                : null;
        }
    }
    #endregion attributeextension

}
