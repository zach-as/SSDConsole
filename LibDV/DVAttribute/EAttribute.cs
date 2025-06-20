using LibDV.DVEntityType;
using static LibUtil.UtilGlobal.CGlobal;
using LibUtil.UtilAttribute;
using Microsoft.Xrm.Sdk;

namespace LibDV.DVAttribute
{
    public enum EAttribute
    {
        // These are the attributes used in comparison operations
        [AAttribute(Attribute_AddressId, EEntityType.Clinic)]
        [ADVRead(), ADVWrite()]
        AddressID,
        [AAttribute(Attribute_AddressLine1, EEntityType.Clinic)]
        [ADVRead(), ADVWrite()]
        AddressLine1,
        [AAttribute(Attribute_AddressLine2, EEntityType.Clinic)]
        [ADVRead(), ADVWrite()]
        AddressLine2,
        [AAttribute(Attribute_Line2Suppressed, EEntityType.Clinic)]
        [ADVRead(), ADVWrite()]
        Line2Suppressed,
        [AAttribute(Attribute_Name, EEntityType.Clinic, EEntityType.MedicalGroup)]
        [ADVRead(), ADVWrite()]
        Name,
        [AAttribute(Attribute_Pac, EEntityType.Clinician, EEntityType.MedicalGroup)]
        [ADVRead(), ADVWrite()]
        Pac,

        // These attributes are not set within this program, but rather are generated in dataverse
        [AAttribute(Attribute_ClinicianId, EEntityType.Clinician)]
        [ADVRead()]
        ClinicianId,
        [AAttribute(Attribute_MedicalGroupId, EEntityType.MedicalGroup)]
        [ADVRead()]
        MedicalGroupId,
        [AAttribute(Attribute_ClinicId, EEntityType.Clinic)]
        [ADVRead()]
        ClinicId,
        [AAttribute(Attribute_ClinicianAtClinicId, EEntityType.ClinicianAtClinic)]
        [ADVRead()]
        ClinicianAtClinicId,
        [AAttribute(Attribute_ClinicianAtMedicalGroupId, EEntityType.ClinicianAtMedicalGroup)]
        [ADVRead()]
        ClinicianAtMedicalGroupId,
        [AAttribute(Attribute_ClinicAtMedicalGroupId, EEntityType.ClinicAtMedicalGroup)]
        [ADVRead()]
        ClinicAtMedicalGroupId,

        // These are the remaining attributes that are not used in comparison operations
        [AAttribute(Attribute_PhoneNumber, EEntityType.Clinic)]
        [ADVWrite()]
        PhoneNumber,
        [AAttribute(Attribute_City, EEntityType.Clinic)]
        [ADVWrite()]
        City,
        [AAttribute(Attribute_Zip, EEntityType.Clinic)]
        [ADVWrite()]
        Zip,
        [AAttribute(Attribute_Npi, EEntityType.Clinician)]
        [ADVWrite()]
        Npi,
        [AAttribute(Attribute_Enrl, EEntityType.Clinician)]
        [ADVWrite()]
        Enrl,
        [AAttribute(Attribute_FirstName, EEntityType.Clinician)]
        [ADVWrite()]
        FirstName,
        [AAttribute(Attribute_MiddleName, EEntityType.Clinician)]
        [ADVWrite()]
        MiddleName,
        [AAttribute(Attribute_LastName, EEntityType.Clinician)]
        [ADVWrite()]
        LastName,
        [AAttribute(Attribute_PrimarySpecialty, EEntityType.Clinician)]
        [ADVWrite()]
        PrimarySpecialty,
        [AAttribute(Attribute_Suffix, EEntityType.Clinician)]
        [ADVWrite()]
        Suffix,
        [AAttribute(Attribute_Sex, EEntityType.Clinician)]
        [ADVWrite()]
        Sex,
        [AAttribute(Attribute_Credentials, EEntityType.Clinician)]
        [ADVWrite()]
        Credentials,
        [AAttribute(Attribute_MedicalSchool, EEntityType.Clinician)]
        [ADVWrite()]
        MedicalSchool,
        [AAttribute(Attribute_GraduationYear, EEntityType.Clinician)]
        [ADVWrite()]
        GraduationYear,
        [AAttribute(Attribute_Telehealth, EEntityType.Clinician)]
        [ADVWrite()]
        Telehealth,
        [AAttribute(Attribute_FullMedicare, EEntityType.Clinician, EEntityType.MedicalGroup)]
        [ADVWrite()]
        FullMedicare,
        [AAttribute(Attribute_PrimarySpecialties, EEntityType.Clinic, EEntityType.MedicalGroup)]
        [ADVWrite()]
        PrimarySpecialties,
        [AAttribute(Attribute_SecondarySpecialties, EEntityType.Clinic, EEntityType.Clinician, EEntityType.MedicalGroup)]
        [ADVWrite()]
        SecondarySpecialties,
        [AAttribute(Attribute_ClinicianCount, EEntityType.Clinic, EEntityType.MedicalGroup)]
        [ADVWrite()]
        ClinicianCount,
    }

    // This class provides extension methods for EAttribute, allowing easy access to its attributes and properties.
    internal static partial class SAttribute
    {
        internal static AAttributeAttribute InternalAttribute(this EAttribute a)
            => CUtilAttribute.InternalAttribute<AAttributeAttribute>(a);

        internal static bool HasDVRead(this EAttribute a)
            => CUtilAttribute.HasInternalAttribute<ADVReadAttribute>(a);

        internal static bool HasDVWrite(this EAttribute a)
            => CUtilAttribute.HasInternalAttribute<ADVWriteAttribute>(a);

        public static EEntityType[] EntityTypes(this EAttribute a)
            => a.InternalAttribute().EntityTypes();

        public static string Attribute(this EAttribute a)
            => a.InternalAttribute().ToString();

    }
}
