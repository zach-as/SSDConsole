using LibCMS.Data;
using LibCMS.Record;
using static LibCMS.Function.SSpecialty;

namespace LibCMS.Data.Associable
{
    public class CClinic : CAssociable
    {
        // This is the name of this clinic
        public string name { get; set; }

        // This indicates the telephone number associated with this clinic
        public string telephoneNumber { get; set; }

        // This is the number of clinicians that operate at this clinic
        public int numClinicians { get; set; }
        
        // This is the address of this clinic
        public CAddress location { get; set; }

        internal CClinic (CRecordItem record)
        {
            name = record.FacilityName;
            telephoneNumber = record.PhoneNumber;
            location = new CAddress(record);
            numClinicians = 1; // Always start at 1 clinician, this number will be incremented as more clinicians are discovered 
        }

        public IEnumerable<CClinician> Clinicians()
        {
            return Associations().OfType<CClinician>();
        }

        public IEnumerable<CMedicalGroup> Organizations()
        {
            return Associations().OfType<CMedicalGroup>();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(location.addressID,
                                    name,
                                    location.addressLine1,
                                    location.addressLine2,
                                    location.line2Suppressed,
                                    location.city,
                                    location.state,
                                    location.zip);
        }
    }
}