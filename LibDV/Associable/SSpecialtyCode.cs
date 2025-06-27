using LibCMS.Data.Associable;
using LibCMS.Function;
using LibDV.Associable;
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
            Dictionary<string, int> specialtyData = DVConnector.GetOptionSetData(CGlobal.OptionSet_ClinicianSpecialtyChoice());

            ValidateSpecialty(specialty, specialtyData);

            return new OptionSetValue(specialtyData[specialty]);
        }

        public static OptionSetValueCollection GetSpecialtyCodes(IEnumerable<string> specialties)
        {
            var specialtyData = DVConnector.GetOptionSetData(CGlobal.OptionSet_ClinicianSpecialtyChoice());
            var specialtyCodes = new OptionSetValueCollection();


            foreach (var specialty in specialties)
            {
                ValidateSpecialty(specialty, specialtyData);
                specialtyCodes.Add(new OptionSetValue(specialtyData[specialty]));
            }

            return specialtyCodes;
        }

        private static void ValidateSpecialty(string specialty, Dictionary<string, int> specialtyData)
        {
            if (!specialtyData.ContainsKey(specialty))
            {
                throw new Exception($"ValidateSpecialty({specialty}) did not find any matching specialties in {CGlobal.OptionSet_ClinicianSpecialtyChoice()}.");
            }

            if (specialtyData[specialty] == -1)
            {
                throw new Exception("ValidateSpecialty{specialty} returned an invalid specialty code of -1.");
            }
        }
    }
}
