using LibCMS.Record;

namespace LibCMS.Data.Associable
{
    public class COrganization : CAssociable
    {
        // This is the unique ID that is associated with this organization in PECOS
        public string pac { get; set; }

        // This is the number of clinicians affiliated with this organization
        public int numClinicians { get; set; }

        // This indicates if this organization accepts medicare payments in full or in part
        public bool acceptsFullMedicare { get; set; }

        internal COrganization (CRecordItem record)
        {
            pac = record.IDPacOrg;
            acceptsFullMedicare = record.MedicareFullOrg == "Y" ? true : false;
            int numClinicians = 1;
            int.TryParse(record.NumClinicians, out numClinicians);
            this.numClinicians = numClinicians;
        }
        public IEnumerable<CClinician> Clinicians()
        {
            return Associations().OfType<CClinician>();
        }

        public IEnumerable<CClinic> Clinics()
        {
            return Associations().OfType<CClinic>();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(pac);
        }
    }
}