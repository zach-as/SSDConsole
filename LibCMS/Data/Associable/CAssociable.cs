using LibUtil.Equality;
using LibUtil.UtilAttribute;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;


namespace LibCMS.Data.Associable
{
    public abstract class CAssociable : IEqualityComparable
    {
        #region association

        [JsonIgnore]
        private List<CAssociable>? associations;
        public List<CAssociable> Associations()
        {
            if (associations is null) associations = new List<CAssociable>();
            return associations;
        }
        internal bool Associate(CAssociable associable)
        {
            if (associable == null) return false;
            if (associable.GetType() == GetType()) return false; // do not associate with associables of the same type
            if (HasAssociation(associable)) return false;
            Associations().Add(associable);
            associable.Associate(this);
            return true;
        }
        internal bool Disassociate(CAssociable associable)
        {
            if (associable == null) return false;
            if (!HasAssociation(associable)) return false;
            Associations().Remove(associable);
            associable.Disassociate(this);
            return true;
        }
        internal bool HasAssociation(CAssociable associable)
        {
            if (associable == null) return false;
            return Associations().Contains(associable);
        }
        #endregion association

        protected CEqualityExpression? eqExpression;
        public abstract CEqualityExpression EqualityExpression();

        public object? AttributeValue(EAttributeName attrName)
        {
            var attrMap = SAttributeMap.AttributeTagMap(this);
            if (!attrMap.HasMapping(attrName))
                return null;
            return attrMap.Mapping(attrName).Value();
        }

        public override bool Equals(object? obj)
        {
            var other = obj as IEqualityComparable;
            return this.Matches(other);
        }
    }

}