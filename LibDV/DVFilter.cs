using LibDV;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using LibCMS.Data.Associable;

namespace LibDV
{
    internal static class DVFilter
    {
        

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

        #region filtergeneration
        internal static FilterExpression EqualExpression(CAssociable a)
        {
            var filter = new FilterExpression(LogicalOperator.And);

            if (a is null) return filter;

            if (a is CClinic)
            {
                var clinic = (CClinic)a;

                // Name && (ID || (Ln1 && (Ln2 || Sprs)))

                var filter_name = FilterAll(Attribute.Name, clinic.Name);
                var filter_id = FilterAll(Attribute.AddressID, clinic.Location.AddressID);
                var filter_ln1 = FilterAll(Attribute.AddressLine1, clinic.Location.AddressLine1);
                var filter_ln2 = FilterAll(Attribute.AddressLine2, clinic.Location.AddressLine2);
                var filter_sprs = FilterAny(Attribute.Line2Suppressed, true);
                if (clinic.Location.Line2Suppressed) filter_sprs.AddEquals(Attribute.Line2Suppressed, false); // if clinic ln 2 is supressed, remote ln 2 sprs does not matter
                
                var filter_addr = new FilterExpression(LogicalOperator.Or);
                var filter_lns_1 = new FilterExpression(LogicalOperator.And);
                var filter_lns_2 = new FilterExpression(LogicalOperator.Or);
                
                // filter_lns_2 = Ln2 || Sprs
                filter_lns_2.AddFilter(filter_ln2);
                filter_lns_2.AddFilter(filter_sprs);
                // filter_lns_1 = Ln1 && (Ln2 || Sprs)
                filter_lns_1.AddFilter(filter_ln1);
                filter_lns_1.AddFilter(filter_lns_2);
                // filter_addr = ID || (Ln1 && (Ln2 || Sprs))
                filter_addr.AddFilter(filter_id);
                filter_addr.AddFilter(filter_lns_1);
                // filter = Name && (ID || (Ln1 && (Ln2 || Sprs)))
                filter.AddFilter(filter_name);
                filter.AddFilter(filter_addr);
            }
            else if (a is CClinician)
            {
                var clinician = (CClinician)a;
                filter.AddCondition(Attribute.Pac, ConditionOperator.Equal, clinician.pacId);
            }
            else if (a is CMedicalGroup)
            {
                var organization = (CMedicalGroup)a;
                filter.AddCondition(Attribute.Pac, ConditionOperator.Equal, organization.pac);
            }
            else throw new Exception($"Unexpected associable type in EqualExpression(): {a.GetType()}");

            return filter;
        }

        internal static void AddEquals(this FilterExpression f, Attribute a, object value)
            => f.AddCondition(a, ConditionOperator.Equal, value);
        internal static void AddEquals(this FilterExpression f, string a, object value)
            => f.AddCondition(a, ConditionOperator.Equal, value);
        internal static void AddNotEquals(this FilterExpression f, Attribute a, object value)
            => f.AddCondition(a, ConditionOperator.NotEqual, value);
        internal static void AddNotEquals(this FilterExpression f, string a, object value)
            => f.AddCondition(a, ConditionOperator.NotEqual, value);
        internal static void AddNull(this FilterExpression f, Attribute a)
            => f.AddCondition(a, ConditionOperator.Null);
        internal static void AddNull(this FilterExpression f, string a)
            => f.AddCondition(a, ConditionOperator.Null);
        internal static void AddNotNull(this FilterExpression f, Attribute a)
            => f.AddCondition(a, ConditionOperator.NotNull);
        internal static void AddNotNull(this FilterExpression f, string a)
            => f.AddCondition(a, ConditionOperator.NotNull);
        internal static void AddCondition(this FilterExpression f, Attribute a, ConditionOperator op, object? value = null)
            => AddCondition(f, a.Attribute(), op, value);
        internal static void AddCondition(this FilterExpression f, string a, ConditionOperator op, object? value = null)
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
                        if (op == ConditionOperator.Equal) f.AddCondition(a, ConditionOperator.Null);
                        if (op == ConditionOperator.NotEqual) f.AddCondition(a, ConditionOperator.NotNull);
                    }
                    else
                    {
                        f.AddCondition(a, op, value);
                    }
                    break;
                case ConditionOperator.Null:
                case ConditionOperator.NotNull:
                    f.AddCondition(a, op);
                    break;
                default:
                    throw new Exception($"In EqualExpression(), attempted to add condition with operator that is not (equal, not equal, null, not null, contains): {op}");

            }
        }


        internal static FilterExpression FilterAny<T>(Attribute a, T value)
            => FilterAny(a.Attribute(), new List<T> { value });
        internal static FilterExpression FilterAny<T>(string attrName, T value)
            => FilterAny(attrName, new List<T> { value });
        internal static FilterExpression FilterAny<T>(Attribute a, params T[] values)
            => FilterAny(a.Attribute(), values.ToList());
        internal static FilterExpression FilterAny<T>(string attrName, params T[] values)
            => FilterAny(attrName, values.ToList());
        internal static FilterExpression FilterAny<T>(Attribute a, List<T> values)
            => FilterMatch(a, LogicalOperator.Or, values, true);
        internal static FilterExpression FilterAny<T>(string attrName, List<T> values)
            => FilterMatch(attrName, LogicalOperator.Or, values, true);

        internal static FilterExpression FilterAll<T>(Attribute a, List<T> values)
            => FilterMatch(a, LogicalOperator.And, values, true);
        internal static FilterExpression FilterAll<T>(Attribute a, T value)
            => FilterAll(a, new List<T> { value });
        internal static FilterExpression FilterAll<T>(Attribute a, params T[] values)
            => FilterAll(a, values.ToList());
        internal static FilterExpression FilterNone<T>(Attribute a, List<T> values)
            => FilterMatch(a, LogicalOperator.And, values, false);
        internal static FilterExpression FilterNone<T>(Attribute a, T value)
            => FilterNone(a, new List<T> { value });
        internal static FilterExpression FilterNone<T>(Attribute a, params T[] values)
            => FilterNone(a, values.ToList());
        private static FilterExpression FilterMatch<T>(Attribute a, LogicalOperator op, List<T> values, bool equals)
            => FilterMatch(a.Attribute(), op, values, equals);
        private static FilterExpression FilterMatch<T>(string a, LogicalOperator op, List<T> values, bool equals)
        {
            FilterExpression f = new FilterExpression(op);
            bool addedNull = false;
            foreach (var v in values)
            {
                if (v is null & !addedNull)
                {
                    addedNull = true;
                    if (equals) f.AddNull(a);
                    else f.AddNotNull(a);
                } else if (v is not null)
                {
                    var vStr = v as string;
                    if (vStr is not null)
                    {
                        if (string.IsNullOrEmpty(vStr) && !addedNull)
                        {
                            addedNull = true;
                            if (equals) f.AddNull(a);
                            else f.AddNotNull(a);
                        } else
                        {
                            if (equals) f.AddEquals(a, v);
                            else f.AddNotEquals(a, v);
                        }
                    } else
                    {
                        
                    }
                }
            }
            return f;
        }

        #endregion filtergeneration

        #region conditionalmatching
        // Returns true if the entity's relevant attributes match the provided associable's EqualExpression().
        internal static bool Matches(CAssociable a, Entity e)
        {
            if (e == null || a == null) return a == null && e == null;
            if (e.LogicalName != a.LogicalName()) return false;
            return Matches(e, a.EqualExpression()); // Returns true if all conditions of EqualExpression() are met
        }
        // Returns true if the associables are functionally equivelant
        internal static bool Matches(CAssociable a1, CAssociable a2)
        {
            if (a1 == null || a2 == null) return a1 == null && a2 == null;
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
                    
                    // We have this because of the niche occurence wherein empty strings are assigned the Null conditionoperator instead of equals ""
                    if (compareTarget is CAssociable && condition.Operator == ConditionOperator.Null)
                    {
                        if (found)
                        {
                            var target_val = AttributeValue(compareTarget, condition.AttributeName);
                            matching = target_val is string ?
                                            string.IsNullOrEmpty(target_val?.ToString())
                                            : target_val is null;
                                           
                        }
                    }
                    
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
                bool? condition_met = null;
                
                switch (op)
                {
                    case ConditionOperator.Equal:
                        original_val = condition.Values.ElementAt(0);
                        condition_met = compare_val?.Equals(original_val);
                        matches = CompoundMatch(matches, condition_met, f);
                        break;
                    case ConditionOperator.NotEqual:
                        original_val = condition.Values.ElementAt(0);
                        condition_met = !compare_val?.Equals(original_val);
                        matches = CompoundMatch(matches, condition_met, f);
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
            if (o is CAssociable a) return a.HasAttribute(attrName);
            return false;
        }
        private static object? AttributeValue(object o, string attrName)
        {
            if (o is null) throw new Exception("DVFilter(): Unable to access attribute: {attrName} from null object!");
            if (o is Entity e) return e.AttributeValue(attrName);
            if (o is CAssociable a) return a.AttributeValue(attrName);
            throw new Exception($"DVFilter(): Attempting to access EAttribute value for object: {o.GetType()} with attrName: {attrName}, but object is invalid type.");
        }

        #endregion attribute


    }
}
