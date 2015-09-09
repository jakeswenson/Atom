using System;
using System.Collections.Generic;
using Atom.Data;
using Atom.Generation.Generators.Code;
using Atom.Generation.Generators.Code.CSharp;

namespace Atom.Generation.Data
{
    public class SqlAccessorMetadata
    {
        public IEnumerable<AtomMemberInfo> InputParams { get; set; }

        public ProjectedAtomRoot BaseAtom { get; set; }

        public Type ReturnPrimitive { get; set; }

        public QueryType QueryType { get; set; }

        public string Group { get; set; }

        public string Name { get; set; }

        public AtomMemberInfo QueryKey { get; set; }
    }
}
