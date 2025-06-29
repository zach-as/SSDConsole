using LibCMS.Data.Associable;
using LibCMS.Specialty;
using LibDV.Associable;
using LibDV.Connector;
using LibUtil.UtilGlobal;
using Microsoft.Xrm.Sdk;

namespace LibDV.Associable
{
    public static class SSpecialtyCode
    {
        internal static OptionSetValue PrimarySpecialtyCode(this CClinician c)
            => GetSpecialtyCode(c.PrimarySpecialty());
        internal static OptionSetValueCollection PrimarySpecialtyCodes(this CClinic c)
            => GetSpecialtyCodes(c.PrimarySpecialties());
        internal static OptionSetValueCollection PrimarySpecialtyCodes(this CMedicalGroup o)
            => GetSpecialtyCodes(o.PrimarySpecialties());

        internal static OptionSetValueCollection SecondarySpecialtyCodes(this CClinician c)
            => GetSpecialtyCodes(c.SecondarySpecialties());
        internal static OptionSetValueCollection SecondarySpecialtyCodes(this CClinic c)
            => GetSpecialtyCodes(c.SecondarySpecialties());
        internal static OptionSetValueCollection SecondarySpecialtyCodes(this CMedicalGroup o)
            => GetSpecialtyCodes(o.SecondarySpecialties());

        public static OptionSetValue GetSpecialtyCode(string specialty)
        {
            var specialtyData = SConnectorDV.GetOptionSetData(CGlobal.OptionSet_ClinicianSpecialtyChoice());
            return new OptionSetValue(specialtyData.ValueFromLabel(specialty));
        }

        public static OptionSetValueCollection GetSpecialtyCodes(IEnumerable<string> specialties)
        {
            var specialtyData = SConnectorDV.GetOptionSetData(CGlobal.OptionSet_ClinicianSpecialtyChoice());
            var specialtyCodes = new OptionSetValueCollection();


            foreach (var specialty in specialties)
            {
                specialtyCodes.Add(new OptionSetValue(specialtyData.ValueFromLabel(specialty)));
            }

            return specialtyCodes;
        }
    }
}
