using LibCMS.Record;
using LibUtil.UtilAttribute;
using static LibUtil.UtilAttribute.EAttributeName;

namespace LibCMS.Data.Associable
{
    public class CClinician : CAssociable
    {
        // This is a unique code that represents this clinician in PPES.
        [AAttributeTag(Attribute_Npi)]
        public string npi { get; set; }

        // This is a unique code that represents this clinician in PECOS.
        [AAttributeTag(Attribute_Pac)]
        public string pacId { get; set; }

        // This is a unique code that denotes the clinician's enrollment ID in CMS.
        [AAttributeTag(Attribute_Enrl)]
        public string enrlId { get; set; }

        // This is the clinician's first name.
        [AAttributeTag(Attribute_FirstName)]
        public string firstName { get; set; }

        // This is the clinician's middle name or initial.
        [AAttributeTag(Attribute_MiddleName)]
        public string middleName { get; set; }

        // This is the clinician's last name.
        [AAttributeTag(Attribute_LastName)]
        public string lastName { get; set; }

        // This is the clinician's primary specialty.
        [AAttributeTag(Attribute_PrimarySpecialty)]
        public string primarySpecialty { get; set; }

        // This is an array of the clinician's listed secondary specialties
        [AAttributeTag(Attribute_SecondarySpecialties)]
        public string[] secondarySpecialties { get; set; }

        // This is the suffix of the clinician (Jr., Sr., etc)
        [AAttributeTag(Attribute_Suffix)]
        public string suffix { get; set; }

        // This is the categorized sex of the clinician (M, F)
        [AAttributeTag(Attribute_Sex)]
        public string sex { get; set; }

        // This represents the medical credentials of the clinician (PA, CNA, NP, MD, etc)
        [AAttributeTag(Attribute_Credentials)]
        public string credentials { get; set; }

        // This represents the medical institution where the clinician received their primary clinical education.
        [AAttributeTag(Attribute_MedicalSchool)]
        public string medicalSchool { get; set; }

        // This represents the year in which the clinician graduated from their medical school.
        [AAttributeTag(Attribute_GraduationYear)]
        public string graduationYear { get; set; }

        // This represents if the clinician accepts telehealth
        [AAttributeTag(Attribute_Telehealth)]
        public bool telehealth { get; set; }

        // This indicates if this clinician accepts medicare payments in full or in part
        [AAttributeTag(Attribute_FullMedicare)]
        public bool acceptsFullMedicare { get; set; }

        internal CClinician(CRecordItem record)
        {
            if (record == null) throw new ArgumentNullException("Clinician created with null record");
            npi = record.IDNpi;
            pacId = record.IDPacInd;
            enrlId = record.IDEnrollment;
            firstName = record.ProviderNameFirst;
            middleName = record.ProviderNameMiddle; // This might be the full middle name, an empty string, or a middle initial
            lastName = record.ProviderNameLast;
            primarySpecialty = record.SpecialtyPrimary;
            secondarySpecialties = record.SpecialtySecondaryAll
                                        .Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            suffix = record.Suffix;
            sex = record.Sex; // This should always == "M" or "F" according to the schema
            credentials = record.Credentials;
            medicalSchool = record.SchoolName;
            graduationYear = record.SchoolGradYear;
            telehealth = record.AcceptsTelehealth == "Y" ? true : false;
            acceptsFullMedicare = record.MedicareFullInd == "Y" ? true : false;
        }

        public IEnumerable<CClinic> Clinics()
        {
            return Associations().OfType<CClinic>();
        }

        public IEnumerable<CMedicalGroup> Organizations()
        {
            return Associations().OfType<CMedicalGroup>();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(pacId, npi, enrlId, firstName, middleName, lastName);
        }
    }
}