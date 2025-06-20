using Newtonsoft.Json;


namespace LibCMS.Data.Associable
{
    public class Associable
    {
        #region association

        [JsonIgnore]
        private List<Associable>? associations;
        internal List<Associable> Associations()
        {
            if (associations is null) associations = new List<Associable>();
            return associations;
        }
        internal bool Associate(Associable associable)
        {
            if (associable == null) return false;
            if (associable.GetType() == GetType()) return false; // do not associate with associables of the same type
            if (HasAssociation(associable)) return false;
            Associations().Add(associable);
            associable.Associate(this);
            return true;
        }
        internal bool Disassociate(Associable associable)
        {
            if (associable == null) return false;
            if (!HasAssociation(associable)) return false;
            Associations().Remove(associable);
            associable.Disassociate(this);
            return true;
        }
        internal bool HasAssociation(Associable associable)
        {
            if (associable == null) return false;
            return Associations().Contains(associable);
        }
        #endregion association

        // Sorts the provided associables by type.
        internal static Dictionary<Type, List<Associable>> SortAssociables(List<Associable> associables)
        {
            var sorted = new Dictionary<Type, List<Associable>>();

            foreach (var associable in associables)
            {
                var type = associable.GetType();

                if (!sorted.ContainsKey(type))
                {
                    sorted[type] = new List<Associable>();
                }

                sorted[type].Add(associable);
            }

            return sorted;
        }

        // This returns true if the two associables are effectively the same
        // This uses the EqualExpression() for comparison operations
        
    }

}