using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using LibCMS.Data.Associable;
using LibCMS.Data;
using LibUtil.UtilDisplay;

namespace LibCMS.Record
{
    // This class is constructed directly from the response from the CMS API using Json serialization.
    internal class CRecordResponse
    {
        [JsonInclude]
        [JsonPropertyName("results")]
        internal IEnumerable<CRecordItem>? records { get; set; }

        [JsonInclude]
        [JsonPropertyName("count")]
        // This refers to the number of records identified by the query that exist in the source DB.
        // Note that this count will likely be different from Records().Count()
        internal int RecordCountDB { get; set; }

        internal static async Task<CRecordResponse?> BuildFromHttpResponse(HttpResponseMessage message)
        {
            return JsonSerializer.Deserialize<CRecordResponse>(await message.Content.ReadAsStringAsync());
        }

        internal IEnumerable<CRecordItem> Records()
        {
            if (records is null) records = new List<CRecordItem>();
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
    internal class CRecordOutput
    {
        // The most recent time in which this record output was updated
        private DateTime? timeLastUpdated;
        internal DateTime TimeLastUpdated()
        {
            if (timeLastUpdated is null) timeLastUpdated = DateTime.MinValue;
            return (DateTime)timeLastUpdated;
        }

        // A list of all clinicians, extracted from CRecordResponse
        private List<CClinician>? clinicians {  get; set; }
        internal List<CClinician> Clinicians()
        {
            if (clinicians is null) clinicians = new List<CClinician>();
            return clinicians;
        }

        // A list of all clinics, extracted from CRecordResponse
        private List<CClinic>? clinics { get; set; }
        internal List<CClinic> Clinics()
        {
            if (clinics is null) clinics = new List<CClinic>();
            return clinics;
        }

        // A list of all organizations, extracted from CRecordResponse
        private List<CMedicalGroup>? organizations { get; set; }
        internal List<CMedicalGroup> Organizations()
        {
            if (organizations is null) organizations = new List<CMedicalGroup>();
            return organizations;
        }

        /// <summary>
        /// This function extracts the information from a item response and uses that to expand the fields of this class as the item output.
        /// </summary>
        /// <param name="response">The response to extract information from.</param>
        internal void AddRecordInput(CRecordResponse response)
        {
            if (response is null || !response.HasRecords()) return;

            Console.WriteLine(response.Records().Count());
            foreach (CRecordItem record in response.Records())
            {
                AddItem(record);    
            }
        }

        private void AddItem(CRecordItem item)
        {
            CClinician? clinician = null;
            CClinic? clinic = null;
            CMedicalGroup? medicalGroup = null;

            // Attempt to retrieve the clinician with the PAC ID in the current item from clinicians
            // This will return a default Clinician object if there is no Clinician with the existing Pac ID in clinicians
            clinician = Clinicians().Find(c => c.pacId == item.IDPacInd);

            if (clinician == null)
            {
                // Creates a new clinician and add it to clinicians
                clinician = new CClinician(item);
                Clinicians().Add(clinician);
            }

            // Some records will contain providers not affiliated with any organization
            if (!string.IsNullOrEmpty(item.IDPacOrg))
            {
                // This repeats the same process for clinicians, only applied to organizations
                // After this line, currentOrganization will == the relevant Organization or the default value
                medicalGroup = Organizations().Find(o => o.pac == item.IDPacOrg);

                if (medicalGroup == null)
                {
                    medicalGroup = new CMedicalGroup(item);
                    Organizations().Add(medicalGroup);
                }

                // Associate the identified organization with the clinician
                medicalGroup.Associate(clinician);
            }

            //------------------------------------------------------------------------------------------------------------------------

            // Create a temp clinic for comparison purposes
            CClinic compareClinic = new CClinic(item);
            // This will first check the immutable hash code values of the two clinics
            // In the case of collisions, it will then check the equality expression of the two clinics
            var existingClinic = Clinics().Find(c => compareClinic.GetHashCode() == c.GetHashCode());

            if (existingClinic is null)
            {
                // Save the temp clinic as a permanent clinic
                clinic = compareClinic;
                Clinics().Add(clinic);
            }
            else
            {
                clinic = existingClinic;

                // Increment the number of clinicians at this clinic
                clinic.numClinicians++;

                // If the clinic for this record already exists but in a supressed form, and the new item is not in this supressed form,
                // move over the new data
                if (clinic.location.line2Suppressed && item.Line2Supressed != "Y")
                {
                    clinic.location = new CAddress(item); // update the location to reflect the new record
                }
                if (clinic.telephoneNumber == string.Empty && item.PhoneNumber != string.Empty)
                {
                    clinic.telephoneNumber = item.PhoneNumber; // update the phone number
                }
            }

            // Associate the clinic with the organization (if valid) and the clinician
            clinic.Associate(clinician);
            if (medicalGroup is not null) clinic.Associate(medicalGroup);

            timeLastUpdated = DateTime.UtcNow;
        }
    }

    internal class CRecordItem
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