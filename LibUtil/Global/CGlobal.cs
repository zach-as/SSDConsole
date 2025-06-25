using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.UtilGlobal
{
    public static class CGlobal
    {
        public static string Prefix() => "ssd_";

        public static string OptionSet_ClinicianSpecialtyChoice() => $"{Prefix()}clinicianspecialtychoice";

        public const string Entity_Clinician = "Clinician";
        public const string Entity_MedicalGroup = "Medical Group";
        public const string Entity_Clinic = "Clinic";

        public const string Entity_ClinicianAtClinic = "Clinician at Clinic";
        public const string Entity_ClinicianAtMedicalGroup = "Clinician at Medical Group";
        public const string Entity_ClinicAtMedicalGroup = "Clinic at Medical Group";

        public const string Attribute_AddressId = "addressid";
        public const string Attribute_AddressLine1 = "addressline1";
        public const string Attribute_AddressLine2 = "addressline2";
        public const string Attribute_Line2Suppressed = "addressline2suppressed";
        public const string Attribute_Name = "name";
        public const string Attribute_Pac = "pac";
        public const string Attribute_ClinicianId = "clinicianid";
        public const string Attribute_MedicalGroupId = "medicalgroupid";
        public const string Attribute_ClinicId = "clinicid";
        public const string Attribute_ClinicianAtClinicId = "clinicianatclinicid";
        public const string Attribute_ClinicianAtMedicalGroupId = "clinicianatmedicalgroupid";
        public const string Attribute_ClinicAtMedicalGroupId = "clinicatmedicalgroupid";
        public const string Attribute_PhoneNumber = "phonenumber";
        public const string Attribute_State = "state";
        public const string Attribute_City = "city";
        public const string Attribute_Zip = "zip";
        public const string Attribute_Npi = "npi";
        public const string Attribute_Enrl = "enrl";
        public const string Attribute_FirstName = "firstname";
        public const string Attribute_MiddleName = "middlename";
        public const string Attribute_LastName = "lastname";
        public const string Attribute_PrimarySpecialty = "primaryspecialty";
        public const string Attribute_Suffix = "suffix";
        public const string Attribute_Sex = "sex";
        public const string Attribute_Credentials = "credentials";
        public const string Attribute_MedicalSchool = "medicalschool";
        public const string Attribute_GraduationYear = "graduationyear";
        public const string Attribute_Telehealth = "telehealth";
        public const string Attribute_FullMedicare = "fullmedicare";
        public const string Attribute_PrimarySpecialties = "primaryspecialties";
        public const string Attribute_SecondarySpecialties = "secondaryspecialties";
        public const string Attribute_ClinicianCount = "cliniciancount";


    }
}
