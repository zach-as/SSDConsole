using LibCMS.Record;
using static LibUtil.UtilAttribute.EAttributeName;
using LibUtil.UtilAttribute;
using LibUtil.Equality;

namespace LibCMS.Data.Associable
{
    public class CClinic : CAssociable
    {
        // This is the name of this clinic
        [AAttributeTag(Attribute_Name)]
        public string name { get; set; }

        // This indicates the telephone number associated with this clinic
        [AAttributeTag(Attribute_PhoneNumber)]
        public string telephoneNumber { get; set; }

        // This is the number of clinicians that operate at this clinic
        [AAttributeTag(Attribute_ClinicianCount)]
        public int numClinicians { get; set; }

        // This is the address of this clinic
        public CAddress location { get; set; }

        internal CClinic (CRecordItem record)
        {
            name = record.FacilityName;
            telephoneNumber = record.PhoneNumber;
            location = new CAddress(record);
            numClinicians = 1; // Always start at 1 clinician, this number will be incremented as more clinicians are discovered 
        }

        public List<CClinician> Clinicians()
        {
            return Associations().OfType<CClinician>().ToList();
        }

        public List<CMedicalGroup> Organizations()
        {
            return Associations().OfType<CMedicalGroup>().ToList();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name,
                                    location.addressLine1,
                                    location.city,
                                    location.state,
                                    location.zip);
        }

        public override CEqualityExpression EqualityExpression()
        {
            // If the equality expression has already been created, return it
            // This works because the values of CAssociables do not change over time
            if (eqExpression is not null) return eqExpression;

            // Name && (ID || (Ln1 && (Ln2 || Sprs)))

            var expression = new CEqualityExpression();

            // An expression representing equality to the clinic's name
            var ex_name = SEqualityExpression.NewAndExpression();
            ex_name.AddEquals(Attribute_Name, name);

            // An expression representing equality to the clinic's address ID
            var ex_id = SEqualityExpression.NewAndExpression();
            ex_id.AddEquals(Attribute_AddressId, location.addressID);

            // An expression representing equality to the clinic's address line 2
            var ex_ln2 = SEqualityExpression.NewAndExpression();
            ex_ln2.AddEquals(Attribute_AddressLine2, location.addressLine2);

            // An expression representing equality to the clinic's address line 2 suppressed
            var ex_sprs = SEqualityExpression.NewAndExpression();
            ex_sprs.AddEquals(Attribute_Line2Suppressed, location.line2Suppressed);

            var ex_addr = SEqualityExpression.NewOrExpression();
            var ex_addr_1 = SEqualityExpression.NewAndExpression();
            var ex_addr_2 = SEqualityExpression.NewOrExpression();

            // Ln2 || Sprs
            ex_addr_2.AddExpression(ex_ln2);
            ex_addr_2.AddExpression(ex_sprs);

            // Ln1 && (Ln2 || Sprs)
            ex_addr_1.AddEquals(Attribute_AddressLine1, location.addressLine1);
            ex_addr_1.AddExpression(ex_addr_2);

            // ID || (Ln1 && (Ln2 || Sprs))
            ex_addr.AddExpression(ex_id);
            ex_addr.AddExpression(ex_addr_1);

            // Name && (ID || (Ln1 && (Ln2 || Sprs)))
            expression.AddEquals(Attribute_Name, ex_name);
            expression.AddExpression(ex_addr);

            eqExpression = expression; // save the expression for later
            return expression;
        }
    }
}