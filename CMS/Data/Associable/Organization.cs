using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.PowerPlatform.Dataverse.Client.Extensions;
using Microsoft.Xrm.Sdk;
using SSDConsole.CMS.Function;
using SSDConsole.CMS.Record;
using static SSDConsole.CMS.Function.Specialty;

namespace SSDConsole.CMS.Data.Associable
{
    internal class Organization : Associable
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