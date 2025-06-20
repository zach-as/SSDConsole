using LibCMS.Data;
using LibCMS.Record;
using static LibCMS.Function.Specialty;

namespace LibCMS.Data.Associable
{
    public class Clinic : Associable
    {
        // This is the name of this clinic
        public string name { get; set; }

        // This indicates the telephone number associated with this clinic
        public string telephoneNumber { get; set; }

        // This is the number of clinicians that operate at this clinic
        public int numClinicians { get; set; }
        
        // This is the address of this clinic
        public Address location { get; set; }

        public Clinic (RecordItem record)
        {
            name = record.FacilityName;
            telephoneNumber = record.PhoneNumber;
            location = new Address(record);
            numClinicians = 1; // Always start at 1 clinician, this number will be incremented as more clinicians are discovered 
        }

        public IEnumerable<Clinician> Clinicians()
        {
            return Associations().OfType<Clinician>();
        }

        public IEnumerable<Organization> Organizations()
        {
            return Associations().OfType<Organization>();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(location.AddressID,
                                    name,
                                    location.AddressLine1,
                                    location.AddressLine2,
                                    location.Line2Suppressed,
                                    location.City,
                                    location.State,
                                    location.ZIP);
        }
    }
}