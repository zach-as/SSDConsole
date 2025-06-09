using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using SSDConsole.CMS.Data.Associable;
using SSDConsole.Dataverse;

namespace SSDConsole.CMS.Function
{
    internal static class Specialty
    {

        internal enum SpecialtyType
        {
            Primary,
            Secondary,
            All
        }

        internal static OptionSetValue PrimarySpecialtyCode(this Clinician c)
            => SpecialtyCode(c.PrimarySpecialty());
        internal static OptionSetValueCollection PrimarySpecialtyCodes(this Clinic c)
            => SpecialtyCodes(c.PrimarySpecialties());
        internal static OptionSetValueCollection PrimarySpecialtyCodes(this Organization o)
            => SpecialtyCodes(o.PrimarySpecialties());

        internal static OptionSetValueCollection SecondarySpecialtyCodes(this Clinician c)
            => SpecialtyCodes(c.SecondarySpecialties());
        internal static OptionSetValueCollection SecondarySpecialtyCodes(this Clinic c)
            => SpecialtyCodes(c.SecondarySpecialties());
        internal static OptionSetValueCollection SecondarySpecialtyCodes(this Organization o)
            => SpecialtyCodes(o.SecondarySpecialties());

        internal static string PrimarySpecialty(this Clinician c)
            => c.PrimarySpecialty;
        internal static string[] PrimarySpecialties(this Clinic c)
            => Specialties(c.Clinicians(), SpecialtyType.Primary);
        internal static string[] PrimarySpecialties(this Organization o)
            => Specialties(o.Clinicians(), SpecialtyType.Primary);

        internal static string[] SecondarySpecialties(this Clinician c)
            => c.SecondarySpecialties;
        internal static string[] SecondarySpecialties(this Clinic c)
            => Specialties(c.Clinicians(), SpecialtyType.Secondary);
        internal static string[] SecondarySpecialties(this Organization o)
            => Specialties(o.Clinicians(), SpecialtyType.Secondary);

        internal static async Task<string[]> Specialties(SpecialtyType type = SpecialtyType.All)
            => Specialties(await CMSConnector.GetClinicians(), type);

        private static string[] Specialties(IEnumerable<Clinician> clinicians, SpecialtyType type = SpecialtyType.All)
            => clinicians.SelectMany(c => c.Specialties(type)).Distinct().ToArray();

        private static string[] Specialties(this Clinician c, SpecialtyType type = SpecialtyType.All)
        {
            string[] primarySpecialty = [c.PrimarySpecialty()];
            string[] secondarySpecialties = c.SecondarySpecialties();

            switch (type)
            {
                case SpecialtyType.All:
                    return primarySpecialty.ToList().Concat(secondarySpecialties).ToArray();
                case SpecialtyType.Primary:
                    return primarySpecialty;
                case SpecialtyType.Secondary:
                    return secondarySpecialties;
                default:
                    // this will never be reached
                    throw new Exception($"Specialties() called with invalid type! Type: {type}"); 
            }
        }

        #region specialtycode
        private static OptionSetValue SpecialtyCode(string specialty)
        {
            Dictionary<string, int> specialtyData = DVConnector.GetOptionSetData(Program.OPTIONSET_CLINICIANSPECIALTY);

            ValidateSpecialty(specialty, specialtyData);

            return new OptionSetValue(specialtyData[specialty]);
        }

        private static OptionSetValueCollection SpecialtyCodes(IEnumerable<string> specialties)
        {
            var specialtyData = DVConnector.GetOptionSetData(Program.OPTIONSET_CLINICIANSPECIALTY);
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
                Program.Display(specialtyData.Keys);
                throw new Exception($"ValidateSpecialty({specialty}) did not find any matching specialties in {Program.OPTIONSET_CLINICIANSPECIALTY}.");
            }

            if (specialtyData[specialty] == -1)
            {
                throw new Exception("ValidateSpecialty{specialty} returned an invalid specialty code of -1.");
            }
        }
        #endregion specialtycode
    }
}
