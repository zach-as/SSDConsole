using LibDV.DVEntityType;
using LibUtil.UtilGlobal;
using LibUtil.UtilAttribute;
using Microsoft.Xrm.Sdk;

namespace LibDV.DVAttribute
{
    public enum EAttribute
    {
        [AAttribute("name", EEntityType.MedicalGroup, EEntityType.ClinicAtMedicalGroup, EEntityType.ClinicianAtMedicalGroup, EEntityType.ClinicianAtClinic)]
        Unused_Name, // name attribute that is (mostly) not directly used in read/write operations, but rather acts as a meaningless primary key

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
        [ADVWriteDuplicate(Unused_Name, EEntityType.MedicalGroup)]
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

    // This class provides extension methods for EAttribute, allowing easy access to its attributes and properties.
    public static partial class SAttribute
    {
        internal static AAttributeAttribute InternalAttribute(this EAttribute a)
            => CUtilAttribute.InternalAttribute<AAttributeAttribute>(a);

        internal static bool HasDVRead(this EAttribute a)
            => CUtilAttribute.HasInternalAttribute<ADVReadAttribute>(a);

        internal static bool HasDVWrite(this EAttribute a)
            => CUtilAttribute.HasInternalAttribute<ADVWriteAttribute>(a);
        internal static bool HasDVWriteDuplicate(this EAttribute a)
            => CUtilAttribute.HasInternalAttribute<ADVWriteDuplicate>(a);

        internal static ADVWriteDuplicate DVWriteDuplicate(this EAttribute a)
            => CUtilAttribute.InternalAttribute<ADVWriteDuplicate>(a);

        public static EEntityType[] EntityTypes(this EAttribute a)
            => a.InternalAttribute().EntityTypes();

        public static string Attribute(this EAttribute a)
            => a.InternalAttribute().ToString();

    }
}
