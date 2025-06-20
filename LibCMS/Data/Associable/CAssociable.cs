using Newtonsoft.Json;


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

        // This returns true if the two associables are effectively the same
        // This uses the EqualExpression() for comparison operations
        
    }

}