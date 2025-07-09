using LibUtil.UtilGlobal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.UtilAttribute
{

    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public class AAttributeTypeAttribute : System.Attribute {

        private Type type;

        internal AAttributeTypeAttribute(Type type)
            => this.type = type;

        internal Type Type() => type;
    }
    public enum EAttributeName
    {
        [Description("addressid")]
        [AAttributeType(typeof(string))]
        Attribute_AddressId,
        [Description("addressline1")]
        [AAttributeType(typeof(string))]
        Attribute_AddressLine1,
        [Description("addressline2")]
        [AAttributeType(typeof(string))]
        Attribute_AddressLine2,
        [Description("addressline2suppressed")]
        [AAttributeType(typeof(bool))]
        Attribute_Line2Suppressed,
        [Description("name")]
        [AAttributeType(typeof(string))]
        Attribute_Name,
        [Description("pac")]
        [AAttributeType(typeof(string))]
        Attribute_Pac,
        [Description("clinicianid")]
        [AAttributeType(typeof(Guid))]
        Attribute_ClinicianId,
        [Description("medicalgroupid")]
        [AAttributeType(typeof(Guid))]
        Attribute_MedicalGroupId,
        [Description("clinicid")]
        [AAttributeType(typeof(Guid))]
        Attribute_ClinicId,
        [Description("clinicianatclinicid")]
        [AAttributeType(typeof(Guid))]
        Attribute_ClinicianAtClinicId,
        [Description("clinicianatmedicalgroupid")]
        [AAttributeType(typeof(Guid))]
        Attribute_ClinicianAtMedicalGroupId,
        [Description("clinicatmedicalgroupid")]
        [AAttributeType(typeof(Guid))]
        Attribute_ClinicAtMedicalGroupId,
        [Description("phonenumber")]
        [AAttributeType(typeof(string))]
        Attribute_PhoneNumber,
        [Description("state")]
        [AAttributeType(typeof(string))]
        Attribute_State,
        [Description("city")]
        [AAttributeType(typeof(string))]
        Attribute_City,
        [Description("zip")]
        [AAttributeType(typeof(string))]
        Attribute_Zip,
        [Description("npi")]
        [AAttributeType(typeof(string))]
        Attribute_Npi,
        [Description("enrl")]
        [AAttributeType(typeof(string))]
        Attribute_Enrl,
        [Description("firstname")]
        [AAttributeType(typeof(string))]
        Attribute_FirstName,
        [Description("middlename")]
        [AAttributeType(typeof(string))]
        Attribute_MiddleName,
        [Description("lastname")]
        [AAttributeType(typeof(string))]
        Attribute_LastName,
        [Description("primaryspecialty")]
        [AAttributeType(typeof(string))]
        Attribute_PrimarySpecialty,
        [Description("suffix")]
        [AAttributeType(typeof(string))]
        Attribute_Suffix,
        [Description("sex")]
        [AAttributeType(typeof(string))]
        Attribute_Sex,
        [Description("credentials")]
        [AAttributeType(typeof(string))]
        Attribute_Credentials,
        [Description("medicalschool")]
        [AAttributeType(typeof(string))]
        Attribute_MedicalSchool,
        [Description("graduationyear")]
        [AAttributeType(typeof(string))]
        Attribute_GraduationYear,
        [Description("telehealth")]
        [AAttributeType(typeof(bool))]
        Attribute_Telehealth,
        [Description("fullmedicare")]
        [AAttributeType(typeof(bool))]
        Attribute_FullMedicare,
        [Description("primaryspecialties")]
        [AAttributeType(typeof(List<>))]
        Attribute_PrimarySpecialties,
        [Description("secondaryspecialties")]
        [AAttributeType(typeof(List<>))]
        Attribute_SecondarySpecialties,
        [Description("cliniciancount")]
        [AAttributeType(typeof(int))]
        Attribute_ClinicianCount,

        // The attributes for relationships
        [Description("clinician")]
        [AAttributeType(typeof(object))]
        Attribute_Clinician,
        [Description("clinic")]
        [AAttributeType(typeof(object))]
        Attribute_Clinic,
        [Description("medicalgroup")]
        [AAttributeType(typeof(object))]
        Attribute_MedicalGroup,

        [Description("logicalname")]
        [AAttributeType(typeof(string))]
        Entity_LogicalName,
        [Description("id")]
        [AAttributeType(typeof(Guid))]
        Entity_Id,
    }

    public static class SAttributeName
    {
        private static List<EAttributeName>? attrNames;
        public static List<EAttributeName> AttrNames()
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
        public static Type DataType(this EAttributeName attrName)
            => attrName.InternalAttribute<AAttributeTypeAttribute>().Type();
        public static EAttributeName EnumFromLogical(string logicalName)
        {
            var dict = AttrNamesWithLogical();
            if (dict.ContainsValue(logicalName))
                return dict.FirstOrDefault(kvp => kvp.Value.Equals(logicalName)).Key;
            throw new ArgumentException($"No EAttributeName found for logical name '{logicalName}'.");
        }

        public static bool LogicalNameExists(string logicalName)
        {
            var dict = AttrNamesWithLogical();
            return dict.ContainsValue(logicalName);
        }
    }
}
