using LibUtil.UtilGlobal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.UtilAttribute
{
    public enum EAttributeName
    {
        [Description("addressid")]
        Attribute_AddressId,
        [Description("addressline1")]
        Attribute_AddressLine1,
        [Description("addressline2")]
        Attribute_AddressLine2,
        [Description("addressline2suppressed")]
        Attribute_Line2Suppressed,
        [Description("name")]
        Attribute_Name,
        [Description("pac")]
        Attribute_Pac,
        [Description("clinicianid")]
        Attribute_ClinicianId,
        [Description("medicalgroupid")]
        Attribute_MedicalGroupId,
        [Description("clinicid")]
        Attribute_ClinicId,
        [Description("clinicianatclinicid")]
        Attribute_ClinicianAtClinicId,
        [Description("clinicianatmedicalgroupid")]
        Attribute_ClinicianAtMedicalGroupId,
        [Description("clinicatmedicalgroupid")]
        Attribute_ClinicAtMedicalGroupId,
        [Description("phonenumber")]
        Attribute_PhoneNumber,
        [Description("state")]
        Attribute_State,
        [Description("city")]
        Attribute_City,
        [Description("zip")]
        Attribute_Zip,
        [Description("npi")]
        Attribute_Npi,
        [Description("enrl")]
        Attribute_Enrl,
        [Description("firstname")]
        Attribute_FirstName,
        [Description("middlename")]
        Attribute_MiddleName,
        [Description("lastname")]
        Attribute_LastName,
        [Description("primaryspecialty")]
        Attribute_PrimarySpecialty,
        [Description("suffix")]
        Attribute_Suffix,
        [Description("sex")]
        Attribute_Sex,
        [Description("credentials")]
        Attribute_Credentials,
        [Description("medicalschool")]
        Attribute_MedicalSchool,
        [Description("graduationyear")]
        Attribute_GraduationYear,
        [Description("telehealth")]
        Attribute_Telehealth,
        [Description("fullmedicare")]
        Attribute_FullMedicare,
        [Description("primaryspecialties")]
        Attribute_PrimarySpecialties,
        [Description("secondaryspecialties")]
        Attribute_SecondarySpecialties,
        [Description("cliniciancount")]
        Attribute_ClinicianCount,

        [Description("logicalname")]
        LogicalName,
    }

    public static class SAttributeName
    {
        private static List<EAttributeName>? attrNames;
        private static List<EAttributeName> AttrNames()
        {
            if (attrNames is null)
                attrNames = Enum.GetValues(typeof(EAttributeName)).Cast<EAttributeName>().ToList();
            return attrNames;
        }
        private static Dictionary<EAttributeName, string> AttrNamesWithLogical()
            => AttrNames().ToDictionary(attrName => attrName, attrName => attrName.LogicalName());

        public static DescriptionAttribute DescriptionAttribute(this EAttributeName attrName)
            => attrName.InternalAttribute<DescriptionAttribute>();
        public static string Name(this EAttributeName attrName)
            => attrName.DescriptionAttribute().Description;
        public static string LogicalName(this EAttributeName attrName)
            => CGlobal.Prefix() + attrName.Name();

        public static EAttributeName EnumFromLogical(string attrNameLogical)
        {
            var dict = AttrNamesWithLogical();
            if (dict.ContainsValue(attrNameLogical))
                return dict.FirstOrDefault(kvp => kvp.Value.Equals(attrNameLogical)).Key;
            throw new ArgumentException($"No EAttributeName found for logical name '{attrNameLogical}'.");
        }
    }
}
