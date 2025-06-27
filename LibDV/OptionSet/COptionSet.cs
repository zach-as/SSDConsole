using Microsoft.Xrm.Sdk.Metadata;

namespace LibDV.OptionSet
{
    internal class COptionSetEntry
    {
        private string label;
        private int value;

        internal COptionSetEntry(string label, int value)
        {
            this.label = label;
            this.value = value;
        }

        internal string Label() => label;
        internal int Value() => value;
    }

    
    internal class COptionSet
    {
        private List<COptionSetEntry> entries;

        internal COptionSet(OptionSetMetadata meta)
        {
            entries = meta.Options.Select(
                        o => new COptionSetEntry(
                        o.Label.UserLocalizedLabel.Label,
                        o.Value ?? 0))
                        .ToList();
        }

        internal List<COptionSetEntry> Entries() => entries;

        internal bool HasLabel(string label)
            => Entries().Any(o => o.Equals(label));

        internal List<string> Labels()
            => Entries()
                .Select(e => e.Label())
                .ToList();

        // Identifies labels in the provided list that do not exist in the option set entries
        internal List<string> MissingLabels(List<string> labels)
            => labels.
                Where(l => !HasLabel(l))
                .ToList();
    }
}
