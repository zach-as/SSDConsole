using LibDV.DVEntityType;
using LibUtil.UtilGlobal;
using LibUtil.UtilAttribute;

namespace LibDV.DVAttribute
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class AAttributeAttribute : System.Attribute
    {
        private string attributeName; // the name of this attribute as used in dataverse
        private EEntityType[] entityTypes; // the entity types that this attribute applies to

        internal AAttributeAttribute(string attributeName, params EEntityType[] entityTypes)
        {
            this.attributeName = CGlobal.Prefix() + attributeName;
            this.entityTypes = entityTypes;
        }

        public override string ToString()
            => attributeName;

        internal EEntityType[] EntityTypes()
            => entityTypes;
    }

    // If this attribtue is present on an EAttribute, it indicates that the EAttribute should be included in the columnset when fetching from dataverse
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal class ADVReadAttribute : System.Attribute { }

    // If this attribute is present on an EAttribute, it indicates that the EAttribute should be written to dataverse when creating or updating the relevant entity.
    internal class ADVWriteAttribute : System.Attribute { }

    public enum EAttribute
    {
        // This represents the partition attribute, used in elastic tables
        [AAttribute("partitionid", EEntityType.Clinic, EEntityType.Clinician, EEntityType.MedicalGroup)]
        [ADVWrite()]
        PartitionId,

        // These are the attributes used in comparison operations
        [AAttribute("addressid", EEntityType.Clinic)]
        [ADVRead(), ADVWrite()]
        AddressID,
        [AAttribute("addressline1", EEntityType.Clinic)]
        [ADVRead(), ADVWrite()]
        AddressLine1,
        [AAttribute("addressline2", EEntityType.Clinic)]
        [ADVRead(), ADVWrite()]
        AddressLine2,
        [AAttribute("addressline2suppressed", EEntityType.Clinic)]
        [ADVRead(), ADVWrite()]
        Line2Suppressed,
        [AAttribute("name", EEntityType.Clinic)]
        [ADVRead(), ADVWrite()]
        Name,
        [AAttribute("pac", EEntityType.Clinician, EEntityType.MedicalGroup)]
        [ADVRead(), ADVWrite()]
        Pac,

        // These attributes are not set within this program, but rather are generated in dataverse
        [AAttribute("clinicianid", EEntityType.Clinician)]
        ClinicianID,
        [AAttribute("medicalgroupid", EEntityType.MedicalGroup)]
        MedicalGroupID,
        [AAttribute("clinicid", EEntityType.Clinic)]
        ClinicID,


        // These are the remaining attributes that are not used in comparison operations
        [AAttribute("phonenumber", EEntityType.Clinic)]
        [ADVWrite()]
        PhoneNumber,
        [AAttribute("city", EEntityType.Clinic)]
        [ADVWrite()]
        City,
        [AAttribute("zip", EEntityType.Clinic)]
        [ADVWrite()]
        Zip,
        [AAttribute("npi", EEntityType.Clinician)]
        [ADVWrite()]
        Npi,
        [AAttribute("enrl", EEntityType.Clinician)]
        [ADVWrite()]
        Enrl,
        [AAttribute("firstname", EEntityType.Clinician)]
        [ADVWrite()]
        FirstName,
        [AAttribute("middlename", EEntityType.Clinician)]
        [ADVWrite()]
        MiddleName,
        [AAttribute("lastname", EEntityType.Clinician)]
        [ADVWrite()]
        LastName,
        [AAttribute("primaryspecialty", EEntityType.Clinician)]
        [ADVWrite()]
        PrimarySpecialty,
        [AAttribute("suffix", EEntityType.Clinician)]
        [ADVWrite()]
        Suffix,
        [AAttribute("sex", EEntityType.Clinician)]
        [ADVWrite()]
        Sex,
        [AAttribute("credentials", EEntityType.Clinician)]
        [ADVWrite()]
        Credentials,
        [AAttribute("medicalschool", EEntityType.Clinician)]
        [ADVWrite()]
        MedicalSchool,
        [AAttribute("graduationyear", EEntityType.Clinician)]
        [ADVWrite()]
        GraduationYear,
        [AAttribute("telehealth", EEntityType.Clinician)]
        [ADVWrite()]
        Telehealth,
        [AAttribute("fullmedicare", EEntityType.Clinician, EEntityType.MedicalGroup)]
        [ADVWrite()]
        FullMedicare,
        [AAttribute("primaryspecialties", EEntityType.Clinic, EEntityType.MedicalGroup)]
        [ADVWrite()]
        PrimarySpecialties,
        [AAttribute("secondaryspecialties", EEntityType.Clinic, EEntityType.Clinician, EEntityType.MedicalGroup)]
        [ADVWrite()]
        SecondarySpecialties,
        [AAttribute("cliniciancount", EEntityType.Clinic, EEntityType.MedicalGroup)]
        [ADVWrite()]
        ClinicianCount,
    }

    public static partial class CAttribute
    {
        public static AAttributeAttribute InternalAttribute(this EAttribute a)
            => CUtilAttribute.InternalAttribute<AAttributeAttribute>(a);

        public static bool DVRead(this EAttribute a)
            => CUtilAttribute.HasInternalAttribute<ADVReadAttribute>(a);

        public static bool DVWrite(this EAttribute a)
            => CUtilAttribute.HasInternalAttribute<ADVWriteAttribute>(a);

        public static EEntityType[] EntityTypes(this EAttribute a)
            => a.InternalAttribute().EntityTypes();

        public static string Attribute(this EAttribute a)
            => a.InternalAttribute().ToString();
    }
}
