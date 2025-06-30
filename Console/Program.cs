using LibCMS.Connector;
using LibCMS.Specialty;
using LibDV.Connector;
using LibDV.DVEntity;
using LibUtil.UtilGlobal;
using LibCMS.Data.Associable;
using LibUtil.UtilDisplay;
using LibDV.EntityType;

namespace Console
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Initiate the display
            SDisplay.BeginDisplay();

            // Retrieve the entity data from CMS
            var clinicians = await SConnectorCMS.GetClinicians();
            var clinics = await SConnectorCMS.GetClinics();
            var medicalGroups = await SConnectorCMS.GetMedicalGroups();

            // Retrieve the specialty data from CMS and upload it to DV
            var specialties = await SSpecialty.Specialties();
            SConnectorDV.AddOptionSetData(CGlobal.OptionSet_ClinicianSpecialtyChoice(), specialties);

            // Format entity data from CMS into DV entity data
            var clinicianEntities = new CEntitySet(clinicians.AsParent());
            var clinicEntities = new CEntitySet(clinics.AsParent());
            var medicalGroupEntities = new CEntitySet(medicalGroups.AsParent());

            // Retrieve existing entity data from DV
            var DVClinicians = SConnectorDV.FetchEntities(EEntityType.Clinician);
            var DVClinics = SConnectorDV.FetchEntities(EEntityType.Clinic);
            var DVMedicalGroups = SConnectorDV.FetchEntities(EEntityType.MedicalGroup);

            // Identify the new entitites from CMS which must be pushed to DV
            var newClinicians = clinicianEntities.Excluding(DVClinicians);
            var newClinics = clinicEntities.Excluding(DVClinics);
            var newMedicalGroups = medicalGroupEntities.Excluding(DVMedicalGroups);

            // Organize the new entity data into a singular object for pushing to DV
            var allNewEntities = new CEntitySuperSet();
            allNewEntities.AddSet(newClinicians);
            allNewEntities.AddSet(newClinics);
            allNewEntities.AddSet(newMedicalGroups);

            // Push the new entity data to DV
            // This will also generate IDs for each entity and update the information in allNewEntities accordingly
            allNewEntities = SConnectorDV.PushEntityCreate(allNewEntities);

            // Identify the entities from CMS that already exist in DV and should be updated
            var existingClinicians = clinicianEntities.Overlapping(DVClinicians);
            var existingClinics = clinicEntities.Overlapping(DVClinics);
            var existingMedicalGroups = medicalGroupEntities.Overlapping(DVMedicalGroups);

            // Organize the entity data to be updated to DV into a singular object
            var allExistingEntities = new CEntitySuperSet();
            allExistingEntities.AddSet(existingClinicians);
            allExistingEntities.AddSet(existingClinics);
            allExistingEntities.AddSet(existingMedicalGroups);

            // Push the updated entity data to DV
            SConnectorDV.PushEntityUpdate(allExistingEntities);

            // Merge all existing and new entities into one set
            var allEntities = new CEntitySuperSet();
            allEntities.AddSet(allNewEntities);
            allEntities.AddSet(allExistingEntities);

            //var relationships = S
        }
    }
}
