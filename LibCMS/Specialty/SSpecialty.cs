using LibCMS.Connector;
using LibCMS.Data.Associable;

namespace LibCMS.Specialty
{
    public static class SSpecialty
    {

        public enum ESpecialtyType
        {
            Primary,
            Secondary,
            All
        }

        public static string PrimarySpecialty(this CClinician c)
            => c.primarySpecialty;
        public static List<string> PrimarySpecialties(this CClinic c)
            => Specialties(c.Clinicians(), ESpecialtyType.Primary);
        public static List<string> PrimarySpecialties(this CMedicalGroup o)
            => Specialties(o.Clinicians(), ESpecialtyType.Primary);

        public static List<string> SecondarySpecialties(this CClinician c)
            => c.secondarySpecialties.ToList();
        public static List<string> SecondarySpecialties(this CClinic c)
            => Specialties(c.Clinicians(), ESpecialtyType.Secondary);
        public static List<string> SecondarySpecialties(this CMedicalGroup o)
            => Specialties(o.Clinicians(), ESpecialtyType.Secondary);

        public static async Task<List<string>> Specialties(ESpecialtyType type = ESpecialtyType.All)
            => Specialties(await SConnectorCMS.GetClinicians(), type);

        public static List<string> Specialties(this List<CClinician> clinicians, ESpecialtyType type = ESpecialtyType.All)
            => clinicians.SelectMany(c => c.Specialties(type)).Distinct().ToList();

        private static List<string> Specialties(this CClinician c, ESpecialtyType type = ESpecialtyType.All)
        {
            List<string> primarySpecialty = [c.PrimarySpecialty()];
            List<string> secondarySpecialties = c.SecondarySpecialties();

            switch (type)
            {
                case ESpecialtyType.All:
                    return primarySpecialty.Concat(secondarySpecialties).ToList();
                case ESpecialtyType.Primary:
                    return primarySpecialty;
                case ESpecialtyType.Secondary:
                    return secondarySpecialties;
                default:
                    // this will never be reached
                    throw new Exception($"Specialties() called with invalid type! Type: {type}"); 
            }
        }
    }
}
