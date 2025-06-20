using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SSDConsole.CMS;
using SSDConsole.Dataverse;
using System.Collections;
using SSDConsole.CMS.Function;
using Microsoft.Xrm.Sdk;
using SSDConsole.CMS.Data.Associable;
using SSDConsole.SSDDisplay;
using SSDConsole.Dataverse.DVConnector.DVConnector;

namespace SSDConsole
{
    internal class Program
    {
        internal const string PREFIX = "ssd_";

        internal const string OPTIONSET_CLINICIANSPECIALTY = $"{PREFIX}clinicianspecialtychoice";

        internal const string TABLE_CLINICIAN = $"{PREFIX}clinician";
        internal const string TABLE_ORGANIZATION = $"{PREFIX}medicalgroup";
        internal const string TABLE_CLINIC = $"{PREFIX}clinic";

        static async Task Main(string[] args)
        {
            Display.BeginDisplay();

            DVConnector.AddOptionSetData(OPTIONSET_CLINICIANSPECIALTY, await Specialty.Specialties());

            var entities = DVConnector.CreateAndUpdateEntities((await CMSConnector.GetAssociables()).ToList());
            
            DVConnector.CreateAndPushRelationships(entities);
        }
    }
}
