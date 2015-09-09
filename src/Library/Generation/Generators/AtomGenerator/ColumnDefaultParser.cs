using Microsoft.SqlServer.Management.Smo;

namespace Atom.Generation.Generators.AtomGenerator
{
    internal class ColumnDefaultParser
    {
        private readonly Column _column;

        public ColumnDefaultParser(Column column)
        {
            _column = column;

            SetDefault();
        }

        public string Default { get; private set; }

        private void SetDefault()
        {
            if (!string.IsNullOrEmpty(_column.Default))
            {
                Default = _column.Default;

                return;
            }

            if (_column.DataType.SqlDataType == SqlDataType.Bit)
            {
                Default = StringParenth(2) == "0" ? "false" : "true";

                return;
            }

            if (_column.DataType.SqlDataType == SqlDataType.UniqueIdentifier)
            {
                Default = StringParenth(1);

                return;
            }

            if (_column.DataType.SqlDataType == SqlDataType.BigInt)
            {
                Default = StringParenth(2);

                return;
            }

            if (_column.DataType.SqlDataType == SqlDataType.Date ||
                _column.DataType.SqlDataType == SqlDataType.DateTime)
            {
                Default = StringParenth(1);

                return;
            }

            Default = _column.DefaultConstraint.Text;
        }

        private string StringParenth(int i)
        {
            var withoutLeadingParenth = _column.DefaultConstraint.Text.Substring(i);

            var withoutTrailingParenth = withoutLeadingParenth.Substring(0, withoutLeadingParenth.Length - i);

            return withoutTrailingParenth;
        }
    }
}
