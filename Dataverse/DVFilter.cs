using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using SSDConsole.CMS.Data.Associable;

namespace SSDConsole.Dataverse
{
    internal static class DVFilter
    {

        #region filtersandconditions

        // Counts the cumulative number of conditions within the provided filter and all subfilters
        internal static int CountConditions(this FilterExpression f)
        {
            int count = f.Conditions.Count();
            foreach (FilterExpression subfilter in f.Filters)
            {
                count += subfilter.CountConditions();
            }
            return count;
        }

        internal static FilterExpression EqualExpression(Associable a)
        {
            var filter = new FilterExpression(LogicalOperator.And);

            if (a is null) return filter;

            if (a is Clinic)
            {
                var clinic = (Clinic)a;

                filter.AddCondition(Attribute.Name, ConditionOperator.Equal, clinic.Name);

                var filter_addr = new FilterExpression(LogicalOperator.Or);
                filter_addr.AddCondition(Attribute.AddressID, ConditionOperator.Equal, clinic.Location.AddressID);

                var filter_addr_lns = new FilterExpression(LogicalOperator.And);
                filter_addr_lns.AddCondition(Attribute.AddressLine1, ConditionOperator.Equal, clinic.Location.AddressLine1);

                var filter_addr_ln2 = new FilterExpression(LogicalOperator.Or);

                var filter_addr_ln2_1 = new FilterExpression(LogicalOperator.And); // Check if the line 2 matches
                filter_addr_ln2_1.AddCondition(Attribute.AddressLine2, ConditionOperator.Equal, clinic.Location.AddressLine2);

                var filter_addr_ln2_2 = new FilterExpression(LogicalOperator.And); // Check if the line 2 does not match but the address is still the same thanks to supression
                filter_addr_ln2_2.AddCondition(Attribute.AddressLine2, ConditionOperator.NotEqual, clinic.Location.AddressLine2); // is the line 2 different?
                filter_addr_ln2_2.AddCondition(Attribute.Line2Suppressed, ConditionOperator.NotEqual, clinic.Location.Line2Suppressed); // is the line 2 supression status different?

                // Check if the addresses are the same
                filter_addr_ln2.AddFilter(filter_addr_ln2_1); // are addr ln 2 the same?
                filter_addr_ln2.AddFilter(filter_addr_ln2_2); // are addr ln 2 not the same but the address is still the same thanks to supression?
                filter_addr_lns.AddFilter(filter_addr_ln2); // is addr ln 1 the same and the are the other conditions met?
                filter_addr.AddFilter(filter_addr_lns); // is addr id the same or are the address lines the same?
                filter.AddFilter(filter_addr); // are the addresses the same and the name the same?
            }
            else if (a is Clinician)
            {
                var clinician = (Clinician)a;
                filter.AddCondition(Attribute.Pac, ConditionOperator.Equal, clinician.PacID);
            }
            else if (a is Organization)
            {
                var organization = (Organization)a;
                filter.AddCondition(Attribute.Pac, ConditionOperator.Equal, organization.PacID);
            }
            else throw new Exception($"Unexpected associable type in EqualExpression(): {a.GetType()}");

            return filter;
        }

        private static void AddCondition(this FilterExpression f, Attribute a, ConditionOperator op, object? value = null)
        {
            switch (op)
            {
                case ConditionOperator.Equal:
                case ConditionOperator.NotEqual:
                    bool isNull = value is null ||
                                    value is string ? string.IsNullOrEmpty(value?.ToString())
                                    : false;
                    if (isNull)
                    {
                        if (op == ConditionOperator.Equal) f.AddCondition(a.Attribute(), ConditionOperator.Null);
                        if (op == ConditionOperator.NotEqual) f.AddCondition(a.Attribute(), ConditionOperator.NotNull);
                    }
                    else
                    {
                        f.AddCondition(a.Attribute(), op, value);
                    }
                    break;
                case ConditionOperator.Null:
                case ConditionOperator.NotNull:
                    f.AddCondition(a.Attribute(), op);
                    break;
                default:
                    throw new Exception($"In EqualExpression(), attempted to add condition with operator that is not (equal, not equal, null, not null, contains): {op}");

            }
        }

        #endregion filtersandconditions

        #region conditionalmatching
        // Returns true if the entity's relevant attributes match the provided associable's EqualExpression().
        internal static bool Matches(Associable a, Entity e)
        {
            if (e == null || a == null) return (a == null && e == null);
            if (e.LogicalName != a.LogicalName()) return false;
            return Matches(e, a.EqualExpression()); // Returns true if all conditions of EqualExpression() are met
        }
        // Returns true if the associables are functionally equivelant
        internal static bool Matches(Associable a1, Associable a2)
        {
            if (a1 == null || a2 == null) return (a1 == null && a2 == null);
            if (a1.LogicalName() != a2.LogicalName()) return false;
            return Matches(a2, a1.EqualExpression()); // Returns true if all conditions of EqualExpression() are met
        }

        // Checks if the provided entity meets all conditions
        // This checks subfilters and more
        private static bool Matches(object compareTarget, FilterExpression f)
        {
            // The accumulated matches boolean
            // This will be true under one of the following two conditions
            //      1. f.FilterOperator is "And" and ALL conditions are met
            //      2. f.FilterOperator is "Or" and AT LEAST ONE condition is met
            bool? matches = null;

            if (f.Conditions.Count() > 0)
            {
                matches = CompoundMatch(matches, MatchesConditions(compareTarget, f), f);
            }

            foreach (var filter in f.Filters)
            {
                if (filter.Conditions.Count > 0)
                {
                    matches = CompoundMatch(matches, MatchesConditions(compareTarget, filter), f); // Check if the conditions match on this subfilter
                }
                if (filter.Filters.Count() > 0) // check if this subfilter has subfilters
                {
                    matches = CompoundMatch(matches, Matches(compareTarget, filter), f); // Check if the conditions of the subfilter's subfilters match
                }
            }

            return matches ?? true;
        }

        // Checks if the provided entity meets all conditions within the provided filter
        // Does not check subfilters
        private static bool MatchesConditions(object compareTarget, FilterExpression f)
        {
            // The accumulated matches boolean
            // This will be true under one of the following two conditions
            //      1. f.FilterOperator is "And" and ALL conditions are met
            //      2. f.FilterOperator is "Or" and AT LEAST ONE condition is met
            bool? matches = null;

            foreach (var condition in f.Conditions)
            {
                var found = HasAttribute(compareTarget, condition.AttributeName);
                if (condition.Operator == ConditionOperator.Null || condition.Operator == ConditionOperator.NotNull)
                {
                    bool matching = condition.Operator == ConditionOperator.Null
                                    ? !found // Null means the attribute is not present
                                    : found; // NotNull means the attribute is present
                    // Required attribute not found, which in this case is desired if null is expected for this condition
                    matches = CompoundMatch(matches, matching, f);
                    continue;
                }

                if (!found)
                {
                    // Required attribute not found
                    matches = CompoundMatch(matches, false, f);
                    continue;
                }

                var op = condition.Operator;
                object? compare_val = AttributeValue(compareTarget, condition.AttributeName);
                object? original_val = null;

                switch (op)
                {
                    case ConditionOperator.Equal:
                        original_val = condition.Values.ElementAt(0);
                        matches = CompoundMatch(matches, compare_val?.Equals(original_val), f);
                        break;
                    case ConditionOperator.NotEqual:
                        original_val = condition.Values.ElementAt(0);
                        matches = CompoundMatch(matches, !compare_val?.Equals(original_val), f);
                        break;
                    default:
                        throw new Exception($"Unexpected condition operator in MatchesCondition(): {op}");
                }
            }

            // True if
            //  1. no conditions present
            //  2. all conditions match and filter requires all conditions met
            //  3. at least one condition matches and filter requires only one condition met
            return matches ?? true;
        }

        // This function is used to compound multiple boolean values together
        // depending on the logical operator stored within the filterexpression
        private static bool CompoundMatch(bool? p, bool? c, FilterExpression f)
        {
            // p = the sum of previous match attemps (a || b || c || d) OR (a && b && c && d)
            // c = the result of the current match attempt
            // if c is null, treat it as false
            switch (f.FilterOperator)
            {
                case LogicalOperator.And:
                    return (p ?? true) && (c ?? false); // if p is null, only check c
                case LogicalOperator.Or:
                    return (p ?? false) || (c ?? false); // if p is null, only check c
                default:
                    throw new Exception($"Unexpected filter operator in MatchesConditions(): {f.FilterOperator}");
            }
        }
        #endregion conditionalmatching

        #region attribute

        private static bool HasAttribute(object o, string attrName)
        {
            if (o is null) throw new Exception("DVFilter(): Unable to access attribute: {attrName} from null object!");
            if (o is Entity e) return e.HasAttribute(attrName);
            if (o is Associable a) return a.HasAttribute(attrName);
            return false;
        }
        private static object? AttributeValue(object o, string attrName)
        {
            if (o is null) throw new Exception("DVFilter(): Unable to access attribute: {attrName} from null object!");
            if (o is Entity e) return e.AttributeValue(attrName);
            if (o is Associable a) return a.AttributeValue(attrName);
            throw new Exception($"DVFilter(): Attempting to access Attribute value for object: {o.GetType()} with attrName: {attrName}, but object is invalid type.");
        }

        #endregion attribute

        #region filtergeneration
        internal static FilterExpression FilterAny<T>(Attribute a, List<T> values)
            => FilterMatch(a, LogicalOperator.Or, values);
        internal static FilterExpression FilterAll<T>(Attribute a, List<T> values)
            => FilterMatch(a, LogicalOperator.And, values);
        private static FilterExpression FilterMatch<T>(Attribute a, LogicalOperator op, List<T> values)
        {
            FilterExpression f = new FilterExpression(op);
            values.ForEach(v => f.AddCondition(a.Attribute(), ConditionOperator.Equal, v));
            return f;
        }
        
        #endregion filtergeneration
    }
}
