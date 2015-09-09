using System;
using System.Collections.Generic;
using System.IO;
using Atom.Data;
using Atom.Generation.Generators.Sql.LookupData;
using JetBrains.Annotations;

namespace Atom.Generation.Data
{
    public class SqlResult : GeneratorResult
    {
        private readonly List<GeneratorOutput> _outputs = new List<GeneratorOutput>();
        private const string _lookupOutputFolder = "Data";

        public List<SqlAccessorMetadata> SqlAccessors { get; set; }

        public SqlResult()
        {
            SqlAccessors = new List<SqlAccessorMetadata>();
            DataFiles = new List<string>();
        }

        public override IEnumerable<GeneratorOutput> Outputs
        {
            get { return _outputs; }
        }

        public List<string> DataFiles { get; set; }

        public void AddOutput([NotNull] string folderName, [NotNull] string name, [NotNull] string sql)
        {
            if (folderName == null)
            {
                throw new ArgumentNullException("folderName");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }

            _outputs.Add(new GeneratorOutput(Path.Combine(folderName, name + ".generated.sql"), sql));
        }

        public void AddQueryOutput([NotNull] string name, [NotNull] string sql)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }

            _outputs.Add(new GeneratorOutput(Path.Combine("Stored Procedures", name + ".query.generated.sql"), sql));
        }

        public void AddLookupOutput([NotNull] string name, [NotNull] string sql)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (sql == null)
            {
                throw new ArgumentNullException("sql");
            }

            var fileName = $"{name}.sql";

            _outputs.Add(new GeneratorOutput(Path.Combine(_lookupOutputFolder, fileName), sql));
            DataFiles.Add(fileName);
        }
    }
}
