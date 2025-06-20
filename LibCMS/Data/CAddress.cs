using LibCMS.Record;

namespace LibCMS.Data
{
    public class CAddress
    {
        // This is the first line of the address
        public string addressLine1 { get; }

        // This is the second line of the address
        public string addressLine2 { get; }

        // This indicates if addressLine2 is supressed (may be incomplete)
        public bool line2Suppressed { get; }

        // This indicates the city of the address
        public string city { get; }

        // This indicates the state of the address
        public string state { get; }

        // This indicates the zip code of address
        public string zip { get; }

        /* This is a unique ID used to identify the specific street name,
        * suite, state, zip, and office floor of this address */
        public string addressID { get; }

        internal CAddress(CRecordItem record)
        {
            addressLine1 = record.AddrLine1;
            addressLine2 = record.AddrLine2;
            line2Suppressed = record.Line2Supressed == "Y" ? true : false;
            city = record.City;
            state = record.State;
            zip = record.Zip;
            addressID = record.IdAddr;
        }
    }
}