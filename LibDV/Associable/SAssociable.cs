using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LibCMS.Data.Associable;
using Microsoft.Xrm.Sdk.Query;
using LibDV.DVEntity;
using LibDV.EntityType;
using LibDV.Attribute;
using LibUtil.UtilAttribute;

namespace LibDV.Associable
{
    public static class SAssociable
    {

        #region clinician
        // This accepts a string representing the sex of a clinician ("M" or "F") and returns an OptionSetValue.
        public static OptionSetValue Sex(string sex)
        {
            // code for male = 924040000, code for female = 924040001
            return new OptionSetValue(sex.Contains("M") ? 924040000 : 924040001);
        }
        #endregion clinician

        #region entity
        
        #endregion entity
    }
}
