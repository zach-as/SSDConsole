using LibCMS.Record;

namespace LibCMS.Data
{
    public class Address
    {
        // This is the first line of the address
        public string AddressLine1 { get; }

        // This is the second line of the address
        public string AddressLine2 { get; }

        // This indicates if AddressLine2 is supressed (may be incomplete)
        public bool Line2Suppressed { get; }

        // This indicates the city of the address
        public string City { get; }

        // This indicates the state of the address
        public string State { get; }

        // This indicates the zip code of address
        public string ZIP { get; }

        /* This is a unique ID used to identify the specific street name,
        * suite, state, zip, and office floor of this address */
        public string AddressID { get; }

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