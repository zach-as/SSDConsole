using LibCMS.Record;

namespace LibCMS.Data.Associable
{
    public class Organization : Associable
    {
        // This is the unique ID that is associated with this organization in PECOS
        public string PacID { get; set; }

        // This is the number of clinicians affiliated with this organization
        public int NumberClinicians { get; set; }

        // This indicates if this organization accepts medicare payments in full or in part
        public bool AcceptsFullMedicare { get; set; }

        public Organization (RecordItem record)
        {
            PacID = record.IDPacOrg;
            AcceptsFullMedicare = record.MedicareFullOrg == "Y" ? true : false;
            int numClinicians = 1;
            int.TryParse(record.NumClinicians, out numClinicians);
            NumberClinicians = numClinicians;
        }
        public IEnumerable<Clinician> Clinicians()
        {
            return Associations().OfType<Clinician>();
        }

        public IEnumerable<Clinic> Clinics()
        {
            return Associations().OfType<Clinic>();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PacID);
        }
    }
}