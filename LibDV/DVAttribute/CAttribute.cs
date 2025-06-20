using Microsoft.Xrm.Sdk;
using LibCMS.Data.Associable;
using LibUtil.UtilGlobal;
using LibDV.DVAssociable;
using LibDV.DVEntity;
using LibDV.DVEntityType;

namespace LibDV.DVAttribute
{
    public static partial class CAttribute
    {

        #region applyattribute
        internal static void ApplyAttributes(Associable a, Entity e)
        {
            var attributes = GetAttributes(a);
            foreach (var attribute in attributes)
            {
                if (attribute.DVRead()) ApplyAttribute(attribute, e, a);
            }
        }
        private static void ApplyAttribute(EAttribute attr, Entity e, Associable a)
            => ApplyAttribute(attr, e, GetAttributeValue(a, attr));

        private static void ApplyAttribute(EAttribute a, Entity e, object? value)
        {
            e[a.Attribute()] = value;

            // jank-ass logic to make the ssd_name field in MedicalGroup = Pac
            if (e.EntityType() == EEntityType.MedicalGroup)
                if (a == EAttribute.Pac)
                    e[$"{CGlobal.Prefix()}name"] = value;
        }
        #endregion applyattribute

        #region getattribute
        // this exists so that we don't have to keep using reflection whenever we want to access GetAttributes()
        private static Dictionary<EEntityType, EAttribute[]>? attributes;

        internal static EAttribute[] GetAttributes(EEntityType entityType)
        {
            if (attributes is not null) return attributes[entityType];
            attributes = new Dictionary<EEntityType, EAttribute[]>();
            var allAttributes = GetAttributes();
            foreach (EAttribute a in allAttributes)
            {
                foreach (EEntityType et in a.EntityTypes())
                {
                    if (!attributes.ContainsKey(et))
                    {
                        attributes[et] = new EAttribute[] { a };
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
        internal static EAttribute[] GetAttributes()
        {
            // if attributes is not initialized, initialize with reflection
            if (attributes is null)
            {
                attributes = new Dictionary<EEntityType, EAttribute[]>();
                var entityTypes = CEntityType.EntityTypes();
                var allAttributes = Enum.GetValues<EAttribute>();
                foreach (var t in entityTypes)
                {
                    attributes[t] = allAttributes.Where(a => a.EntityTypes().Contains(t)).ToArray();
                }

            }
            
            return attributes.Values.SelectMany(_ => _).ToArray();
        }
        internal static List<EAttribute> GetAttributes(Associable a)
            => GetAttributes(a.EntityType());
        internal static List<EAttribute> GetAttributes(Entity e)
            => GetAttributes(e.EntityType());

        internal static EAttribute GetAttribute(string attrName)
        {
            try
            {
                return GetAttributes().First(a => a.Attribute().Equals(attrName));
            } catch (Exception ex)
            {
                throw new ArgumentException($"EAttribute with name '{attrName}' not found.", nameof(attrName));
            }
        }

        internal static EAttribute GetAttributeFromString(string s)
        {
            var attributes = GetAttributes();
            foreach (var a in attributes)
            {
                if (a.Attribute().Equals(s)) return a;
            }
            throw new Exception($"Unable to identify attribute from string: {s}");
        }
        #endregion getattribute

        #region getattributevalue

        internal static object? GetAttributeValue(Associable a, EAttribute attribute)
        {
            if (a is Clinic clinic) return GetAttributeValue(clinic, attribute);
            if (a is Clinician clinician) return GetAttributeValue(clinician, attribute);
            if (a is Organization organization) return GetAttributeValue(organization, attribute);
            throw new NotImplementedException($"No attribute retrieval defined for {a.GetType().Name}");
        }
        internal static object? GetAttributeValue(Associable a, string attrName)
            => GetAttributeValue(a, GetAttribute(attrName));

        private static object? GetAttributeValue(Clinic c, EAttribute a)
        {
            if (!a.EntityTypes().Contains(c.EntityType())) throw new Exception($"GetAttributeValue(): EAttribute {a} does not apply to Clinic entity type.");
            return a switch
            {
                EAttribute.AddressID => c.location.AddressID,
                EAttribute.AddressLine1 => c.location.AddressLine1,
                EAttribute.AddressLine2 => c.location.AddressLine2,
                EAttribute.Line2Suppressed => c.location.Line2Suppressed,
                EAttribute.City => c.location.City,
                EAttribute.Zip => c.location.ZIP,
                EAttribute.PhoneNumber => c.telephoneNumber,
                EAttribute.Name => c.name,
                EAttribute.ClinicianCount => c.numClinicians,
                EAttribute.PrimarySpecialties => c.PrimarySpecialtyCodes(),
                EAttribute.SecondarySpecialties => c.SecondarySpecialtyCodes(),
                _ => null,
            };
        }

        private static object? GetAttributeValue(Clinician c, EAttribute a)
        {
            if (!a.EntityTypes().Contains(c.EntityType())) throw new Exception($"GetAttributeValue(): EAttribute {a} does not apply to Clinic entity type.");
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

        private static object? GetAttributeValue(Organization o, EAttribute a)
        {
            if (!a.EntityTypes().Contains(o.EntityType())) throw new Exception($"GetAttributeValue(): EAttribute {a} does not apply to Clinic entity type.");
            return a switch {
                EAttribute.Pac => o.PacID,
                EAttribute.PartitionId => o.PacID,
                EAttribute.PrimarySpecialties => o.PrimarySpecialtyCodes(),
                EAttribute.SecondarySpecialties => o.SecondarySpecialtyCodes(),
                EAttribute.FullMedicare => o.AcceptsFullMedicare,
                EAttribute.ClinicianCount => o.NumberClinicians,
                _ => null
            };
        }
        #endregion getattributevalue

        #region hasattribute
        internal static bool HasAttribute(Associable a, string attrName)
            => HasAttribute(a, GetAttribute(attrName));
        internal static bool HasAttribute(Associable a, EAttribute attr)
        {
            if (attr.EntityTypes().Contains(a.EntityType())) return GetAttributeValue(a, attr) != null;
            return false;
        }
        #endregion hasattribute

        #region idattribute
        internal static EAttribute IdAttribute(this EntityReference er)
            => IdAttribute(er.EntityType());
        internal static EAttribute IdAttribute(this Entity e)
            => IdAttribute(e.EntityType());
        internal static EAttribute IdAttribute(this Associable a)
            => IdAttribute(a.EntityType());
        internal static EAttribute IdAttribute(EEntityType entityType)
            => entityType.IdAttribute();
        #endregion idattribute

    }
}
