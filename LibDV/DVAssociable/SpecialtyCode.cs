using LibCMS.Data.Associable;
using LibCMS.Function;
using LibDV.DVAssociable;
using LibUtil.UtilGlobal;
using Microsoft.Xrm.Sdk;

namespace LibDV.DVAssociable
{
    public static class SpecialtyCode
    {
        internal static OptionSetValue PrimarySpecialtyCode(this Clinician c)
            => GetSpecialtyCode(c.PrimarySpecialty());
        internal static OptionSetValueCollection PrimarySpecialtyCodes(this Clinic c)
            => GetSpecialtyCodes(c.PrimarySpecialties());
        internal static OptionSetValueCollection PrimarySpecialtyCodes(this Organization o)
            => GetSpecialtyCodes(o.PrimarySpecialties());

        internal static OptionSetValueCollection SecondarySpecialtyCodes(this Clinician c)
            => GetSpecialtyCodes(c.SecondarySpecialties());
        internal static OptionSetValueCollection SecondarySpecialtyCodes(this Clinic c)
            => GetSpecialtyCodes(c.SecondarySpecialties());
        internal static OptionSetValueCollection SecondarySpecialtyCodes(this Organization o)
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
