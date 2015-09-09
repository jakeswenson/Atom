using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Atom.Data.Projections;

namespace Atom.Data.Serialization
{
    public static class AtomCreator
    {
        private const string AtomDefaultsFileName = "_defaults.atom";

        internal static AtomModel FromString(string atomString, AtomDefaults defaults = null)
        {
            return JsonConvert.DeserializeObject<AtomModel>(atomString, new AtomRootConverter(defaults?.AdditionalInfo ?? AtomAdditionalInfo.Default()));
        }

        public static AtomDefaults LoadDefaults(string path)
        {
            var defaultPath = Path.Combine(path, AtomDefaultsFileName);

            if (File.Exists(defaultPath))
            {
                return JsonConvert.DeserializeObject<AtomDefaults>(File.ReadAllText(defaultPath));
            }

            return null;
        }

        public static ReadOnlyCollection<AtomModel> FromFolder(string path, AtomDefaults defaults = null)
        {
            var allAtoms =
                Directory.EnumerateFiles(path, "*.atom")
                            .Where(i => !IsProjection(i) && !IsDefaultAtom(i))
                            .Select(
                                file =>
                                {
                                    var atom = FromString(File.ReadAllText(file), defaults);

                                    return atom;
                                });

            return AtomMemberBinder.BindReferences(allAtoms).ToList().AsReadOnly();
        }

        public static ReadOnlyCollection<AtomModel> FromFolder(string path)
        {
            return FromFolder(path, LoadDefaults(path));
        }

        private static bool IsProjection(string arg)
        {
            return arg.EndsWith(".proj.atom");
        }

        private static bool IsDefaultAtom(string atom)
        {
            return Path.GetFileName(atom) == AtomDefaultsFileName;
        }

        public static IEnumerable<ProjectionAtom> ProjectionsFromFolder(string path)
        {
            return Directory.EnumerateFiles(path, "*.query.atom")
                            .Select(file => JsonConvert.DeserializeObject<QueryProjectionAtom>(File.ReadAllText(file)));
        }
    }
}
