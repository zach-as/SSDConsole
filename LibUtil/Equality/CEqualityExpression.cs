using LibUtil.UtilAttribute;

namespace LibUtil.Equality
{
    public class CEqualityExpression
    {
        private List<CEqualityCondition> conditions;
        private List<CEqualityExpression> expressions;
        private EEqualityExpressionOperator op;
        
        public CEqualityExpression(EEqualityExpressionOperator op = EEqualityExpressionOperator.And)
        {
            conditions = new List<CEqualityCondition>();
            expressions = new List<CEqualityExpression>();
            this.op = op;
        }

        #region add_condition
        internal void AddCondition(CEqualityCondition condition)
            => conditions.Add(condition);
        public void AddCondition(EAttributeName attr, EEqualityComparator comp, object? val)
            => AddCondition(SEqualityCondition.NewCondition(attr, comp, val));
        public void AddCondition(EAttributeName attr, EEqualityComparator comp, IEqualityComparable owner, EAttributeName attr2)
            => AddCondition(SEqualityCondition.NewCondition(attr, comp, owner, attr2));
        public void AddCondition(EAttributeName attr, EEqualityComparator comp, Func<object?> valFunc)
            => AddCondition(SEqualityCondition.NewCondition(attr, comp, valFunc));
        public void AddEquals(EAttributeName attr, object? val)
            => AddCondition(attr, EEqualityComparator.Equal, val);
        public void AddEquals(EAttributeName attr, IEqualityComparable owner, EAttributeName attr2)
            => AddCondition(attr, EEqualityComparator.Equal, owner, attr2);
        public void AddEquals(EAttributeName attr, Func<object?> valFunc)
            => AddCondition(attr, EEqualityComparator.Equal, valFunc);
        public void AddNotEquals(EAttributeName attr, object? val)
            => AddCondition(attr, EEqualityComparator.NotEqual, val);
        public void AddNotEquals(EAttributeName attr, IEqualityComparable owner, EAttributeName attr2)
            => AddCondition(attr, EEqualityComparator.NotEqual, owner, attr2);
        public void AddNotEquals(EAttributeName attr, Func<object?> valFunc)
            => AddCondition(attr, EEqualityComparator.NotEqual, valFunc);
        public void AddNull(EAttributeName attr)
            => AddCondition(attr, EEqualityComparator.Equal, null as object);
        #endregion add_condition

        public void AddExpression(CEqualityExpression expression)
            => expressions.Add(expression);

        public List<CEqualityCondition> Conditions() => conditions;
        public List<CEqualityExpression> Expressions() => expressions;
        public EEqualityExpressionOperator Operator() => op;

        #region equalityexpression_display
        private string OperatorToString()
            => op.ToString().ToUpper();
        private string ConditionsToString()
            => string.Join(OperatorToString(), conditions.Select(c => c.ToString()));
        private string ExpressionsToString()
            => string.Join(OperatorToString(), expressions.Select(e => e.ToString()));
        private string ConditionsPrintForm(int layer)
            => OptionalIndentation(layer, false) + string.Join(OperatorToString(), conditions.Select(c => c.ToString()));
        private string ExpressionsPrintForm(int layer)
            => OptionalIndentation(layer, true) + string.Join(OperatorToString(), expressions.Select(e => e.PrintForm(layer)));
        private string OptionalIndentation(int layer, bool isExpression)
            => isExpression ?
                // when displaying expressions, if there are conditions present, display on following line with tabs
                // if there are no conditions present, only add the necessary tabbing
                Conditions().Any() ? "\n" + TabbedIndentation(layer) : TabbedIndentation(layer)
                // when displaying conditions, display on same line with appropriate num of tabs
                : TabbedIndentation(layer);
        private string TabbedIndentation(int layer)
            => new String('\t', layer);

        public override string ToString()
            => $"{ConditionsToString()} {OperatorToString()} {ExpressionsToString()}";
        public string PrintForm(int layer = 0) // layer starts at 0, it is used to indicate indentation level
            => $"{layer+1}. {ConditionsPrintForm(layer)} {OperatorToString()} {ExpressionsPrintForm(layer+1)}";
        // EXAMPLE OUTPUT
        // 1. a = b and c = d and
        // 2.     a = c or d = e or f = g or
        // 3.         c = h and i = j
        #endregion equalityexpression_display

    }

    // Extension methods for CEqualityExpression
    public static partial class SEqualityExpression
    {
        internal static EEqualityResult Evaluate(this CEqualityExpression ex, IEqualityComparable other)
        {
            if (ex == null)
                return EEqualityResult.Invalid;
            if (!ex.Conditions().Any() && !ex.Expressions().Any())
                return EEqualityResult.True; // no conditions and sub-expressions means always true

            var op = ex.Operator();
            bool? runningOutcome = null;

            foreach (var condition in ex.Conditions())
            {
                var evaluation = condition.Evaluate(other);
                if (evaluation == EEqualityResult.Invalid)
                    return EEqualityResult.Invalid;

                var conditionMet = evaluation.IsTrue();

                if (op == EEqualityExpressionOperator.And && conditionMet == false)
                    return EEqualityResult.False; // short-circuit for AND if any condition is false for optimization

                // Compound this evaluation result with all previous ones according to the operator
                runningOutcome = op.Compound(runningOutcome, conditionMet);
            }

            foreach (var expression in ex.Expressions())
            {
                var evaluation = Evaluate(expression, other);
                if (evaluation == EEqualityResult.Invalid)
                    return EEqualityResult.Invalid;

                var expressionMet = evaluation.IsTrue();

                if (op == EEqualityExpressionOperator.And && expressionMet == false)
                    return EEqualityResult.False; // short-circuit for AND if any condition is false for optimization

                // Compound this evaluation result with all previous ones according to the operator
                runningOutcome = op.Compound(runningOutcome, expressionMet);
            }

            return (runningOutcome ?? true) ? EEqualityResult.True : EEqualityResult.False;
        }

        public static CEqualityExpression NewAndExpression()
            => new CEqualityExpression(EEqualityExpressionOperator.And);
        public static CEqualityExpression NewOrExpression()
            => new CEqualityExpression(EEqualityExpressionOperator.Or);
    }
}
