using LibCMS.Record;
using static LibUtil.UtilAttribute.EAttributeName;
using LibUtil.UtilAttribute;
using LibUtil.Equality;
using LibCMS.Specialty;
using LibUtil.Reflection;

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

        // This represents all primary specialties held by clinicians affiliated with this clinic
        [AAttributeTag(Attribute_PrimarySpecialties)]
        [AOverrideValue(EFuncName.LibDV_SAssociable_GetSpecialtyCodes)]
        public List<string> PrimarySpecialties()
            => SSpecialty.PrimarySpecialties(this);

        // This represents all secondary specialties held by clinicians affiliated with this clinic
        [AAttributeTag(Attribute_SecondarySpecialties)]
        [AOverrideValue(EFuncName.LibDV_SAssociable_GetSpecialtyCodes)]
        public List<string> SecondarySpecialties()
            => SSpecialty.SecondarySpecialties(this);

        // This is the address of this clinic
        [AAttributeTagNested()]
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
                                    location.addressLine1);
        }

        public static new CEqualityExpression EqualityExpression(IEqualityComparable? comp)
        {
            var clinic = comp as CClinic;

            // If the equality expression has already been created, return it
            // This works because the values of CAssociables do not change over time
            if (clinic?.eqExpression is not null) return clinic.eqExpression;

            // Name && (ID || (Ln1 && (Ln2 || Sprs)))

            var expression = new CEqualityExpression();

            // An expression representing equality to the clinic's address line 2
            var ex_ln2_match = SEqualityExpression.NewAndExpression();
            ex_ln2_match.AddEquals(Attribute_AddressLine2, clinic?.location.addressLine2); // Ln2
            var ex_ln2_sprs = SEqualityExpression.NewAndExpression();
            ex_ln2_sprs.AddNotEquals(Attribute_AddressLine2, clinic?.location.addressLine2); // !Ln2
            ex_ln2_sprs.AddNotEquals(Attribute_Line2Suppressed, clinic?.location.line2Suppressed); //!Sprs

            var ex_addr = SEqualityExpression.NewOrExpression();
            var ex_addr_1 = SEqualityExpression.NewAndExpression();
            var ex_addr_2 = SEqualityExpression.NewOrExpression();

            // Ln2 || (!Ln2 && !Sprs)
            ex_addr_2.AddExpression(ex_ln2_match); // Ln2
            ex_addr_2.AddExpression(ex_ln2_sprs); // !Ln2 && !Sprs == Ln2

            // Ln1 && (Ln2 || Sprs)
            ex_addr_1.AddEquals(Attribute_AddressLine1, clinic?.location.addressLine1); // Ln1
            ex_addr_1.AddExpression(ex_addr_2); // Ln2 || (!Ln2 && !Sprs)

            // ID || (Ln1 && (Ln2 || Sprs))
            ex_addr.AddEquals(Attribute_AddressId, clinic?.location.addressID); // ID
            ex_addr.AddExpression(ex_addr_1); // Ln1 && (Ln2 || (!Ln2 && !Sprs))

            // Name && (ID || (Ln1 && (Ln2 || Sprs)))
            expression.AddEquals(Attribute_Name, clinic?.name); // Name
            expression.AddExpression(ex_addr); // ID || (Ln1 && (Ln2 || (!Ln2 && !Sprs)))

            if (clinic is not null)
                clinic.eqExpression = expression; // save the expression for later

            return expression;
        }
    }
}