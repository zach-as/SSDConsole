using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.UtilGlobal
{
    public static class CGlobal
    {
        public static string Prefix() => "ssd_";

        public static string OptionSet_ClinicianSpecialtyChoice() => $"{Prefix()}clinicianspecialtychoice";

        public static string Entity_Clinician() => "Clinician";
        public static string Entity_MedicalGroup() => "Medical Group";
        public static string Entity_Clinic() => $"{Prefix()}clinic";

        public static string Entity_ClinicianAtClinic() => $"{Prefix()}clinicianatclinic";
        public static string Entity_ClinicianAtMedicalGroup() => $"{Prefix()}clinicianatmedicalgroup";
        public static string Entity_ClinicAtMedicalGroup() => $"{Prefix()}clinicatmedicalgroup";
    }
}
