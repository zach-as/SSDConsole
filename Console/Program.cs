using LibCMS.Connector;
using LibCMS.Specialty;
using LibDV.Connector;
using LibDV.DVEntity;
using LibUtil.UtilGlobal;
using LibCMS.Data.Associable;

namespace Console
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
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

            // Organize the entity data into a singular object
            var allEntities = new CEntitySuperSet();
            allEntities.AddSet(clinicianEntities);
            allEntities.AddSet(clinicEntities);
            allEntities.AddSet(medicalGroupEntities);


        }
    }
}
