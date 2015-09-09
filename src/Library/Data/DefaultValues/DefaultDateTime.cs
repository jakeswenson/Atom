namespace Atom.Data
{
    public class DefaultDateTime : SqlDefaultValue
    {
        public override string SqlRepresentation()
        {
            return "getutcdate()";
        }
    }

    public class BoolDefault : SqlDefaultValue
    {
        private readonly bool _defaultValue;

        public BoolDefault(bool defaultValue)
        {
            _defaultValue = defaultValue;
        }

        public override string SqlRepresentation()
        {
            return _defaultValue.ToString();
        }
    }
}
