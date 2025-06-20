using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibCMS.Path
{
    internal enum EPathMisc
    {
        // The base URI for CMS
        [Description("https://data.cms.gov/provider-data")]
        Base,

        // The API extension for the CMS endpoint
        [Description("api/1")]
        Api,

        // For use on the end of any query to CMS
        [Description("0")]
        End,
    }

    internal enum EPathStore
    {
        // References the storage of data in CMS
        [Description("datastore")]
        Datastore,

        // References the storage of metadata in CMS
        [Description("metastore")]
        Metastore,
    }
    internal enum EPathAction
    {
        // Represents the basic query action to be used in CMS
        [Description("query")]
        Query,
    }

    internal enum EPathDatabase
    {
        // References the primary database of providers in CMS
        [Description("mj5m-pzi6")]
        NationalProviderDatabase,
    }

    internal static class SPath
    {
        internal static Uri BuildUri(EPathStore store = EPathStore.Datastore,
                                      EPathAction action = EPathAction.Query,
                                      EPathDatabase database = EPathDatabase.NationalProviderDatabase)
        {
            var sb = new StringBuilder();
            sb.Append(EPathMisc.Base.Description());
            sb.Append("/");
            sb.Append(EPathMisc.Api.Description());
            sb.Append("/");
            sb.Append(store.Description());
            sb.Append("/");
            sb.Append(action.Description());
            sb.Append("/");
            sb.Append(database.Description());
            sb.Append("/");
            sb.Append(EPathMisc.End.Description());
            return new Uri(sb.ToString());
        }

        public static string Description(this Enum e)
        {
            var fieldInfo = e.GetType().GetField(e.ToString());
            var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            return attributes is not null && attributes.Length > 0 ? attributes[0].Description : e.ToString();
        }
    }
}
