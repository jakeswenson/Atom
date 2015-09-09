using System;
using JetBrains.Annotations;

namespace Atom.Generation.Data
{
    public class GeneratorOutput
    {
        private readonly string _fileName;

        private readonly string _output;

        public GeneratorOutput([NotNull] string fileName, [NotNull] string output)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            _fileName = fileName;
            _output = output;
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }
        }

        public string Output
        {
            get
            {
                return _output;
            }
        }
    }
}
