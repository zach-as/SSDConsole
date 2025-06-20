using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;
using ExpressiveAnnotations.Attributes;

namespace SSDConsole.CMS.Params
{

    [JsonDerivedType(typeof(Condition))]
    [JsonDerivedType(typeof(ConditionGroup))]
    public interface ICondition{}

    // This model is used to represent an individual condition that
    // must be fulfilled for a particular record to be returned in a query.
    public class Condition : ICondition
    {
        public Condition(string _property, ConditionOperator _operator, string _value)
        {
            Property = _property;
            Operator = _operator.Description();
            Value = _value;
        }

        // The name of the property to example (e.g. "org_pac_id")
        [JsonInclude]
        [Required]
        public string Property { get; }

        // The operator to use (=, <>, <, <=, >, >=, like, between, in, not in, is_empty, not_empty, contains, starts with, match)
        [JsonInclude]
        [Required]
        public string Operator { get; }

        // The value to which the property will be compared
        [JsonInclude]
        [Required]
        public string Value { get; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not Condition) return false;

            Condition? c = obj as Condition;

            if (c?.Property == Property &&
                c?.Operator == Operator &&
                c?.Value == Value) return true;

            return false;
        }
    }

    // This model is used to represent a group of conditions that
    // must be fulfilled for a particular record to be returned in a query.
    public class ConditionGroup : ICondition
    {
        // The operator to determine if all or only some conditions must be met (and, or)
        [JsonInclude]
        [RequiredIf("Conditions.Count() > 1")]
        public string GroupOperator { get; set; } = null;

        // The conditions that must be fulfilled for a record to be returned
        [JsonInclude]
        [Required]
        public IEnumerable<ICondition> Conditions { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is ConditionGroup)) return false;

            ConditionGroup c = (ConditionGroup)obj;

            if (GroupOperator != c.GroupOperator) return false;

            foreach (ICondition condition in Conditions)
            {
                if (!c.Conditions.Contains(condition)) return false;
            }

            return true;
        }
    }

    public enum ConditionOperator
    {
        [Description("=")]
        Equals = 0,

        [Description("<>")]
        NotEquals = 1,

        [Description("<")]
        LessThan = 2,

        [Description("<=")]
        LessThanOrEqualTo = 3,

        [Description(">")]
        GreaterThan = 4,

        [Description(">=")]
        GreaterThanOrEqualTo = 5,

        [Description("like")]
        Like = 6,

        [Description("between")]
        Between = 7,

        [Description("in")]
        In = 8,

        [Description("not in")]
        NotIn = 9,

        [Description("is_empty")]
        IsEmpty = 10,

        [Description("not_empty")]
        NotEmpty = 11,

        [Description("contains")]
        Contains = 12,

        [Description("starts with")]
        StartsWith = 13,

        [Description("match")]
        Match = 14,
    }
    public static class ConditionOperator_Extensions
    {
        public static string Description(this ConditionOperator conditionOperator)
        {
            var fieldInfo = conditionOperator.GetType().GetField(conditionOperator.ToString());
            var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            return attributes is not null && attributes.Length > 0 ? attributes[0].Description : conditionOperator.ToString();
        }
    }
}