using LibCMS.Record;
using static LibUtil.UtilAttribute.EAttributeName;
using LibUtil.UtilAttribute;
using LibUtil.Equality;
using LibCMS.Specialty;
using LibUtil.Reflection;

namespace LibCMS.Data.Associable
{
    public class CMedicalGroup : CAssociable
    {
        // This is the unique ID that is associated with this organization in PECOS
        [AAttributeTag(Attribute_Pac)]
        public string pac { get; set; }

        [AAttributeTag(Attribute_Name)]
        public string name { get; set; }

        // This is the number of clinicians affiliated with this organization
        [AAttributeTag(Attribute_ClinicianCount)]
        public int numClinicians { get; set; }

        // This indicates if this organization accepts medicare payments in full or in part
        [AAttributeTag(Attribute_FullMedicare)]
        public bool acceptsFullMedicare { get; set; }

        // This represents all primary specialties held by clinicians affiliated with this medical group
        [AAttributeTag(Attribute_PrimarySpecialties)]
        [AOverrideValue(EFuncName.LibDV_SAssociable_GetSpecialtyCodes)]
        public List<string> PrimarySpecialties()
            => SSpecialty.PrimarySpecialties(this);

        // This represents all secondary specialties held by clinicians affiliated with this medical group
        [AAttributeTag(Attribute_SecondarySpecialties)]
        [AOverrideValue(EFuncName.LibDV_SAssociable_GetSpecialtyCodes)]
        public List<string> SecondarySpecialties()
            => SSpecialty.SecondarySpecialties(this);

        internal CMedicalGroup (CRecordItem record)
        {
            pac = record.IDPacOrg;
            name = pac;
            acceptsFullMedicare = record.MedicareFullOrg == "Y" ? true : false;
            int numClinicians = 1;
            int.TryParse(record.NumClinicians, out numClinicians);
            this.numClinicians = numClinicians;
        }
        public List<CClinician> Clinicians()
        {
            return Associations().OfType<CClinician>().ToList();
        }

        public List<CClinic> Clinics()
        {
            return Associations().OfType<CClinic>().ToList();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(pac);
        }

        public static new CEqualityExpression EqualityExpression(IEqualityComparable? comp)
        {
            var medicalGroup = comp as CMedicalGroup;

            // If the equality expression has already been created, return it
            // This works because the values of CAssociables do not change over time
            if (medicalGroup?.eqExpression is not null) return medicalGroup.eqExpression;

            var expression = SEqualityExpression.NewAndExpression();
            expression.AddEquals(Attribute_Pac, medicalGroup?.pac);

            if (medicalGroup is not null)
                medicalGroup.eqExpression = expression; // save the expression for later

            return expression;
        }

    }
}