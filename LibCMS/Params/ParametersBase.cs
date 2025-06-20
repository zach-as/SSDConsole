using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Text.Json.Serialization;

namespace LibCMS.Params
{
    public class ParametersBase
    {
        [Description("The conditions to filter the query by.")]
        [JsonInclude]
        public List<ICondition> Conditions { get; set; }

        [Description("The maximum number of records to return. If null, get all records.")]
        [JsonInclude]
        public int? Limit { get; set; } = null;

        [Description("The offset of the current query (used for pagination)")]
        [JsonInclude]
        public int Offset { get; set; } = 0;

        [Description("Should the schema be returned?")]
        [JsonInclude]
        public bool Schema { get; set; } = false;

        public ParametersBase()
        {
            AddCondition(new Condition("state", ConditionOperator.Equals, "AZ"));
            if (Limit > 2000) throw new Exception("Limit cannot be greater than 2000.");
            if (Limit == 0) Limit = null; // Indicate that limit should be limitless
        }

        /// <summary>
        /// This function adds the given condition to the list of conditions if it is not alread present.
        /// </summary>
        /// <param name="condition">The condition to add.</param>
        /// <returns>True if the condition was successfully added, false otherwise.</returns>
        public bool AddCondition(ICondition condition)
        {
            if (Conditions == null) Conditions = new List<ICondition>();
            if (!HasCondition(condition))
            {
                Conditions.Add(condition);
                return true;
            }
            return false;
        }

        public bool HasCondition(ICondition condition)
        {
            if (Conditions == null) return false;
            return Conditions.Contains(condition);
        }
    }
}