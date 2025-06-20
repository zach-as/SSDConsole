using LibCMS.Data.Associable;

namespace LibCMS.Function
{
    public static class SSpecialty
    {

        public enum SpecialtyType
        {
            Primary,
            Secondary,
            All
        }

        public static string PrimarySpecialty(this CClinician c)
            => c.primarySpecialty;
        public static string[] PrimarySpecialties(this CClinic c)
            => Specialties(c.Clinicians(), SpecialtyType.Primary);
        public static string[] PrimarySpecialties(this CMedicalGroup o)
            => Specialties(o.Clinicians(), SpecialtyType.Primary);

        public static string[] SecondarySpecialties(this CClinician c)
            => c.secondarySpecialties;
        public static string[] SecondarySpecialties(this CClinic c)
            => Specialties(c.Clinicians(), SpecialtyType.Secondary);
        public static string[] SecondarySpecialties(this CMedicalGroup o)
            => Specialties(o.Clinicians(), SpecialtyType.Secondary);

        public static async Task<string[]> Specialties(SpecialtyType type = SpecialtyType.All)
            => Specialties(await SCmsAccess.GetClinicians(), type);

        private static string[] Specialties(IEnumerable<CClinician> clinicians, SpecialtyType type = SpecialtyType.All)
            => clinicians.SelectMany(c => c.Specialties(type)).Distinct().ToArray();

        private static string[] Specialties(this CClinician c, SpecialtyType type = SpecialtyType.All)
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
    }
}
