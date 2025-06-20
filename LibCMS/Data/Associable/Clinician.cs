using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;
using LibCMS.Function;
using LibCMS.Record;

namespace LibCMS.Data.Associable
{
    public class Clinician : Associable
    {
        // This is a unique code that represents this clinician in PPES.
        public string NPI { get; set; }

        // This is a unique code that represents this clinician in PECOS.
        public string PacID { get; set; }

        // This is a unique code that denotes the clinician's enrollment ID in CMS.
        public string EnrlID { get; set; }

        // This is the clinician's first name.
        public string FirstName { get; set; }

        // This is the clinician's middle name or initial.
        public string MiddleName { get; set; }

        // This is the clinician's last name.
        public string LastName { get; set; }

        // This is the clinician's primary specialty.
        public string PrimarySpecialty { get; set; }

        // This is an array of the clinician's listed secondary specialties
        public string[] SecondarySpecialties { get; set; }

        // This is the suffix of the clinician (Jr., Sr., etc)
        public string Suffix { get; set; }

        // This is the categorized sex of the clinician (M, F)
        public string sex { get; set; }

        // This represents the medical credentials of the clinician (PA, CNA, NP, MD, etc)
        public string Credentials { get; set; }

        // This represents the medical institution where the clinician received their primary clinical education.
        public string MedicalSchool { get; set; }

        // This represents the year in which the clinician graduated from their medical school.
        public string GraduationYear { get; set; }

        // This represents if the clinician accepts telehealth
        public bool Telehealth { get; set; }

        // This indicates if this clinician accepts medicare payments in full or in part
        public bool AcceptsFullMedicare { get; set; }

        public Clinician(RecordItem record)
        {
            if (record == null) throw new ArgumentNullException("Clinician created with null record");
            NPI = record.IDNpi;
            PacID = record.IDPacInd;
            EnrlID = record.IDEnrollment;
            FirstName = record.ProviderNameFirst;
            MiddleName = record.ProviderNameMiddle; // This might be the full middle name, an empty string, or a middle initial
            LastName = record.ProviderNameLast;
            PrimarySpecialty = record.SpecialtyPrimary;
            SecondarySpecialties = record.SpecialtySecondaryAll
                                        .Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            Suffix = record.Suffix;
            sex = record.Sex; // This should always == "M" or "F" according to the schema
            Credentials = record.Credentials;
            MedicalSchool = record.SchoolName;
            GraduationYear = record.SchoolGradYear;
            Telehealth = record.AcceptsTelehealth == "Y" ? true : false;
            AcceptsFullMedicare = record.MedicareFullInd == "Y" ? true : false;
        }

        public IEnumerable<Clinic> Clinics()
        {
            return Associations().OfType<Clinic>();
        }

        public IEnumerable<Organization> Organizations()
        {
            return Associations().OfType<Organization>();
        }

        

        public override int GetHashCode()
        {
            return HashCode.Combine(PacID, NPI, EnrlID, FirstName, MiddleName, LastName);
        }
    }
}