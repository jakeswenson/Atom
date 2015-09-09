using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Atom.Data;
using Atom.Data.Projections;
using Atom.Data.Serialization;
using Serilog;

namespace Atom.Generation
{
    public class GeneratorArguments<TConfig> where TConfig : TargetConfig
    {        
        private readonly Lazy<ReadOnlyCollection<AtomModel>> _atoms;
        private readonly Lazy<ReadOnlyCollection<ProjectionAtom>> _projections;


        public GeneratorArguments(TConfig config, string atomsFolder, AtomDefaults defaults)
            : this(
                  config, 
                  atomsFolder,
                  defaults,
                  Lazy(() => LoadAtoms(atomsFolder, defaults)),
                  Lazy(() => LoadProjections(atomsFolder)))
        {
        }

        public GeneratorArguments(TConfig config, string atomsFolder, AtomDefaults defaults, ReadOnlyCollection<AtomModel> atoms) 
            : this(
                  config, 
                  atomsFolder,
                  defaults,
                  new Lazy<ReadOnlyCollection<AtomModel>>(() => atoms),
                  Lazy(() => LoadProjections(atomsFolder)))
        {
        }

        private GeneratorArguments(
            TConfig config, 
            string atomsFolder,
            AtomDefaults defaults,
            Lazy<ReadOnlyCollection<AtomModel>> atoms,
            Lazy<ReadOnlyCollection<ProjectionAtom>> projections)
        {
            Config = config;
            AtomsFolder = atomsFolder;
            Defaults = defaults;
            _atoms = atoms;
        }

        private static Lazy<T> Lazy<T>(Func<T> func)
        {
            return new Lazy<T>(func);
        }

        private static ReadOnlyCollection<AtomModel> LoadAtoms(string atomsFolder, AtomDefaults defaults)
        {
            Log.Information("Loading atoms from {AtomFolder}", atomsFolder);

            var atoms = AtomCreator.FromFolder(atomsFolder, defaults).ToList();

            Log.Information("Loaded {LoadedAtomsCount} atoms", atoms.Count);

            return atoms.AsReadOnly();
        }

        private static ReadOnlyCollection<ProjectionAtom> LoadProjections(string atomFolder)
        {
            Log.Information("Loading projection atoms from {AtomFolder}", atomFolder);

            var projectionAtoms = AtomCreator.ProjectionsFromFolder(atomFolder)
                                             .ToList();

            Log.Information("Loaded {ProjectionCount} projections", projectionAtoms.Count);

            return projectionAtoms.AsReadOnly();
        }

        public TConfig Config { get; }

        public string AtomsFolder { get; }

        public AtomDefaults Defaults { get; }

        public ReadOnlyCollection<AtomModel> Atoms => _atoms.Value;

        public ReadOnlyCollection<ProjectionAtom> Projections => _projections.Value;
    }
}
