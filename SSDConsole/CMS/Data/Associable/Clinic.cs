using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.Xrm.Sdk;
using SSDConsole.CMS.Function;
using SSDConsole.CMS.Record;
using SSDConsole.Dataverse;
using static SSDConsole.CMS.Function.Specialty;

namespace SSDConsole.CMS.Data.Associable
{
    internal class Clinic : Associable
    {
        // This is the name of this clinic
        public string Name { get; set; }

        // This indicates the telephone number associated with this clinic
        public string TelephoneNumber { get; set; }

        // This is the number of clinicians that operate at this clinic
        public int NumberClinicians { get; set; }
        
        // This is the address of this clinic
        public Address Location { get; set; }

        public Clinic (RecordItem record)
        {
            Name = record.FacilityName;
            TelephoneNumber = record.PhoneNumber;
            Location = new Address(record);
            NumberClinicians = 1; // Always start at 1 clinician, this number will be incremented as more clinicians are discovered 
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
            return HashCode.Combine(Location.AddressID,
                                    Name,
                                    Location.AddressLine1,
                                    Location.AddressLine2,
                                    Location.Line2Suppressed,
                                    Location.City,
                                    Location.State,
                                    Location.ZIP);
        }
    }
}