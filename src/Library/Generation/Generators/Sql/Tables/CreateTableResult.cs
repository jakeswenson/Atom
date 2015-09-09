namespace Atom.Generation.Generators.Sql.Tables
{
    public class CreateTableResult
    {
        private readonly string _name;

        private readonly string _tableSql;

        public CreateTableResult(string name, string tableSql)
        {
            _name = name;
            _tableSql = tableSql;
        }

        public string TableSql
        {
            get { return _tableSql; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
