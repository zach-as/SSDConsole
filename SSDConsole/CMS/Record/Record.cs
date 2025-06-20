using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using SSDConsole.CMS.Data;
using SSDConsole.CMS.Data.Associable;
using SSDConsole.CMS.Record;

namespace SSDConsole.CMS.Record
{
    // This class is constructed directly from the response from the CMS API using Json serialization.
    internal class RecordResponse
    {
        [JsonInclude]
        [JsonPropertyName("results")]
        internal IEnumerable<RecordItem>? records { get; set; }

        [JsonInclude]
        [JsonPropertyName("count")]
        // This refers to the number of records identified by the query that exist in the source DB.
        // Note that this count will likely be different from Records().Count()
        internal int RecordCountDB { get; set; }

        internal static async Task<RecordResponse?> BuildFromHttpResponse(HttpResponseMessage message)
        {
            return System.Text.Json.JsonSerializer.Deserialize<RecordResponse>(await message.Content.ReadAsStringAsync());
        }

        internal IEnumerable<RecordItem> Records()
        {
            if (records is null) records = new List<RecordItem>();
            return records;
        }

        internal bool HasRecords()
        {
            if (records is null) return false;
            if (Records().Count() == 0) return false;
            return true;
        }
    }

    // This class processes and stores the information returned from the CMS API into a more usable form.
    internal class RecordOutput
    {
        // The most recent time in which this record output was updated
        private DateTime? timeLastUpdated;
        internal DateTime TimeLastUpdated()
        {
            if (timeLastUpdated is null) timeLastUpdated = DateTime.MinValue;
            return (DateTime)timeLastUpdated;
        }

        // A list of all clinicians, extracted from RecordResponse
        private List<Clinician>? clinicians {  get; set; }
        internal List<Clinician> Clinicians()
        {
            if (clinicians is null) clinicians = new List<Clinician>();
            return clinicians;
        }

        // A list of all clinics, extracted from RecordResponse
        private List<Clinic>? clinics { get; set; }
        internal List<Clinic> Clinics()
        {
            if (clinics is null) clinics = new List<Clinic>();
            return clinics;
        }

        // A list of all organizations, extracted from RecordResponse
        private List<Organization>? organizations { get; set; }
        internal List<Organization> Organizations()
        {
            if (organizations is null) organizations = new List<Organization>();
            return organizations;
        }

        /// <summary>
        /// This function extracts the information from a item response and uses that to expand the fields of this class as the item output.
        /// </summary>
        /// <param name="response">The response to extract information from.</param>
        internal void AddRecordInput(RecordResponse response)
        {
            if (response is null || !response.HasRecords()) return;
            
            foreach (RecordItem record in response.Records())
            {
                AddItem(record);    
            }
        }

        private void AddItem(RecordItem item)
        {
            Clinician? clinician = null;
            Clinic? clinic = null;
            Organization? organization = null;

            // Attempt to retrieve the clinician with the PAC ID in the current item from clinicians
            // This will return a default Clinician object if there is no Clinician with the existing Pac ID in clinicians
            clinician = Clinicians().FirstOrDefault(c => c.PacID == item.IDPacInd);

            if (clinician == null)
            {
                // CreateEntities a new clinician and add it to clinicians
                clinician = new Clinician(item);
                Clinicians().Add(clinician);
            }

            // Some records will contain providers not affiliated with any organization
            if (!string.IsNullOrEmpty(item.IDPacOrg))
            {
                // This repeats the same process for clinicians, only applied to organizations
                // After this line, currentOrganization will == the relevant Organization or the default value
                organization = Organizations().FirstOrDefault(o => o.PacID == item.IDPacOrg);

                if (organization == null)
                {
                    organization = new Organization(item);
                    Organizations().Add(organization);
                }

                // Associate the identified organization with the clinician
                organization.Associate(clinician);
            }
            //------------------------------------------------------------------------------------------------------------------------

            // Create a temp clinic for comparison purposes
            Clinic compareClinic = new Clinic(item);
            clinic = Clinics().FirstOrDefault(c => c.Equals(compareClinic)); // use the custom comparison operators

            if (clinic is null)
            {
                // Save the temp clinic as a permanent clinic
                clinic = compareClinic;
                Clinics().Add(clinic);
            }
            else
            {
                // Increment the number of clinicians at this clinic
                clinic.NumberClinicians++;

                // If the clinic for this record already exists but in a supressed form, and the new item is not in this supressed form,
                // move over the new data
                if (clinic.Location.Line2Suppressed && item.Line2Supressed != "Y")
                {
                    clinic.Location = new Address(item); // update the location to reflect the new record
                }
                if (clinic.TelephoneNumber == string.Empty && item.PhoneNumber != string.Empty)
                {
                    clinic.TelephoneNumber = item.PhoneNumber; // update the phone number
                }
            }

            // Associate the clinic with the organization (if valid) and the clinician
            clinic.Associate(clinician);
            if (organization is not null) clinic.Associate(organization);

            timeLastUpdated = DateTime.UtcNow;
        }
    }

    internal class RecordItem
    {

        [JsonInclude]
        [JsonPropertyName("npi")]
        internal required string IDNpi { get; set; }

        [JsonInclude]
        [JsonPropertyName("ind_pac_id")]
        internal required string IDPacInd { get; set; }

        [JsonInclude]
        [JsonPropertyName("org_pac_id")]
        internal required string IDPacOrg { get; set; }

        [JsonInclude]
        [JsonPropertyName("ind_enrl_id")]
        internal required string IDEnrollment { get; set; }

        [JsonInclude]
        [JsonPropertyName("provider_last_name")]
        internal required string ProviderNameLast { get; set; }

        [JsonInclude]
        [JsonPropertyName("provider_first_name")]
        internal required string ProviderNameFirst { get; set; }

        [JsonInclude]
        [JsonPropertyName("provider_middle_name")]
        internal required string ProviderNameMiddle { get; set; }

        [JsonInclude]
        [JsonPropertyName("suff")]
        internal required string Suffix { get; set; }

        [JsonInclude]
        [JsonPropertyName("gndr")]
        internal required string Sex { get; set; }

        [JsonInclude]
        [JsonPropertyName("cred")]
        internal required string Credentials { get; set; }

        [JsonInclude]
        [JsonPropertyName("med_sch")]
        internal required string SchoolName { get; set; }

        [JsonInclude]
        [JsonPropertyName("grd_yr")]
        internal required string SchoolGradYear { get; set; }

        [JsonInclude]
        [JsonPropertyName("pri_spec")]
        internal required string SpecialtyPrimary { get; set; }

        [JsonInclude]
        [JsonPropertyName("sec_spec_1")]
        internal required string SpecialtySecondary1 { get; set; }

        [JsonInclude]
        [JsonPropertyName("sec_spec_2")]
        internal required string SpecialtySecondary2 { get; set; }

        [JsonInclude]
        [JsonPropertyName("sec_spec_3")]
        internal required string SpecialtySecondary3 { get; set; }

        [JsonInclude]
        [JsonPropertyName("sec_spec_4")]
        internal required string SpecialtySecondary4 { get; set; }

        [JsonInclude]
        [JsonPropertyName("sec_spec_all")]
        internal required string SpecialtySecondaryAll { get; set; }

        [JsonInclude]
        [JsonPropertyName("telehlth")]
        internal required string AcceptsTelehealth { get; set; }

        [JsonInclude]
        [JsonPropertyName("facility_name")]
        internal required string FacilityName { get; set; }

        [JsonInclude]
        [JsonPropertyName("num_org_mem")]
        internal required string NumClinicians { get; set; }

        [JsonInclude]
        [JsonPropertyName("adr_ln_1")]
        internal required string AddrLine1 { get; set; }

        [JsonInclude]
        [JsonPropertyName("adr_ln_2")]
        internal required string AddrLine2 { get; set; }

        [JsonInclude]
        [JsonPropertyName("ln_2_sprs")]
        internal required string Line2Supressed { get; set; }

        [JsonInclude]
        [JsonPropertyName("citytown")]
        internal required string City { get; set; }

        [JsonInclude]
        [JsonPropertyName("state")]
        internal required string State { get; set; }

        [JsonInclude]
        [JsonPropertyName("zip_code")]
        internal required string Zip { get; set; }

        [JsonInclude]
        [JsonPropertyName("telephone_number")]
        internal required string PhoneNumber { get; set; }

        [JsonInclude]
        [JsonPropertyName("ind_assgn")]
        internal required string MedicareFullInd { get; set; }

        [JsonInclude]
        [JsonPropertyName("grp_assgn")]
        internal required string MedicareFullOrg { get; set; }

        [JsonInclude]
        [JsonPropertyName("adrs_id")]
        internal required string IdAddr { get; set; }

    }
}