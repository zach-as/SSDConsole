using LibUtil.UtilAttribute;
using LibUtil.Equality;

namespace LibUtil.Equality
{
    public interface IEqualityComparable
    {
        // This should return a CEqualityExpression representing
        // the conditions to be met for some comparable to be equal to this comparable
        public abstract CEqualityExpression EqualityExpression();

        // This should return the value of the attribute with the given name
        // if the attribute is not present, it should return null
        // this is used to get the value of an attribute for comparison purposes
        public abstract object? AttributeValue(EAttributeName attributeName);
    }
}
