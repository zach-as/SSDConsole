using LibUtil.UtilAttribute;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;


namespace LibCMS.Data.Associable
{
    public class CAssociable
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

        public Dictionary<EAttributeName, object?> GetAttributePairs()
            => GetAttributePairs(this);
        // Retrieves all values of fields marked with the AAttributeTag and returns pairs of the logicalName and value.
        private static Dictionary<EAttributeName, object?> GetAttributePairs(object o)
            => SAttributeUtil.AttributeTagMap(o)
                .ToDictionary(
                    mapping => mapping.Attribute().AttributeName(),
                    mapping => mapping.Value()
                );
        /*{
            var results = new Dictionary<string, object?>();
            var props = o.GetType().GetProperties();
            foreach(var prop in props)
            {
                var hasDVIndicator = SUtilAttribute.HasInternalAttribute<ADVIndicatorAttribute>(o, prop.Name);
                if (hasDVIndicator)
                {
                    var indicatorAttr = SUtilAttribute.InternalAttribute<ADVIndicatorAttribute>(o, prop.Name);
                    results[indicatorAttr.Name()] = prop.GetValue(o);
                    continue;
                }

                var hasNestedIndicator = SUtilAttribute.HasInternalAttribute<ADVIndicatorNestedAttribute>(o, prop.Name);
                if (hasNestedIndicator)
                {
                    var val = prop.GetValue(o);
                    if (val is null) continue; // dont try to examine nested attributes if the value is null
                    var nestedResults = GetAttributePairs(val); // use recursion to get nested attributes
                    foreach (var nestedResult in nestedResults)
                    {
                        results[nestedResult.Key] = nestedResult.Value; // add the nested results to the main results
                    }
                }
            }
            return results;
        }*/

        // Sorts the provided associables by type.
        internal static Dictionary<Type, List<CAssociable>> SortAssociables(List<CAssociable> associables)
        {
            var sorted = new Dictionary<Type, List<CAssociable>>();

            foreach (var associable in associables)
            {
                var type = associable.GetType();

                if (!sorted.ContainsKey(type))
                {
                    sorted[type] = new List<CAssociable>();
                }

                sorted[type].Add(associable);
            }

            return sorted;
        }
        
    }

}