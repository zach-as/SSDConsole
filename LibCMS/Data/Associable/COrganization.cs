using LibCMS.Record;
using static LibUtil.UtilAttribute.EAttributeName;
using LibUtil.UtilAttribute;
using LibUtil.Equality;

namespace LibCMS.Data.Associable
{
    public class CMedicalGroup : CAssociable
    {
        // This is the unique ID that is associated with this organization in PECOS
        [AAttributeTag(Attribute_Pac)]
        public string pac { get; set; }

        // This is the number of clinicians affiliated with this organization
        [AAttributeTag(Attribute_ClinicianCount)]
        public int numClinicians { get; set; }

        // This indicates if this organization accepts medicare payments in full or in part
        [AAttributeTag(Attribute_FullMedicare)]
        public bool acceptsFullMedicare { get; set; }

        internal CMedicalGroup (CRecordItem record)
        {
            pac = record.IDPacOrg;
            acceptsFullMedicare = record.MedicareFullOrg == "Y" ? true : false;
            int numClinicians = 1;
            int.TryParse(record.NumClinicians, out numClinicians);
            this.numClinicians = numClinicians;
        }
        public IEnumerable<CClinician> Clinicians()
        {
            return Associations().OfType<CClinician>();
        }

        public IEnumerable<CClinic> Clinics()
        {
            return Associations().OfType<CClinic>();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(pac);
        }

        public override CEqualityExpression EqualityExpression()
        {
            var expression = SEqualityExpression.NewAndExpression();
            expression.AddEquals(Attribute_Pac, pac);
            return expression;
        }

    }
}