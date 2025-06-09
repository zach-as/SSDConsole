using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSDConsole.CMS.Record;

namespace SSDConsole.CMS.Data
{
    internal class Address
    {
        // This is the first line of the address
        internal string AddressLine1 { get; set; }

        // This is the second line of the address
        internal string AddressLine2 { get; set; }

        // This indicates if AddressLine2 is supressed (may be incomplete)
        internal bool Line2Suppressed { get; set; }

        // This indicates the city of the address
        internal string City { get; set; }

        // This indicates the state of the address
        internal string State { get; set; }

        // This indicates the zip code of address
        internal string ZIP { get; set; }

        /* This is a unique ID used to identify the specific street name,
        * suite, state, zip, and office floor of this address */
        internal string AddressID { get; set; }

        internal Address(RecordItem record)
        {
            AddressLine1 = record.AddrLine1;
            AddressLine2 = record.AddrLine2;
            Line2Suppressed = record.Line2Supressed == "Y" ? true : false;
            City = record.City;
            State = record.State;
            ZIP = record.Zip;
            AddressID = record.IdAddr;
        }
    }
}