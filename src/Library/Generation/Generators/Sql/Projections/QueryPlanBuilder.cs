using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Atom.Data;
using Atom.Exceptions;
using Atom.Generation.Data;
using Atom.Generation.Extensions;
using Atom.Data.Projections;
using System;

namespace Atom.Generation.Generators.Sql.Projections
{
    public class QueryPlanBuilder
    {
        private readonly Dictionary<string, AtomModel> _allAtoms;

        public ProjectionAtom Projection { get; }

        public QueryPlanBuilder(ProjectionAtom projectionAtom, Dictionary<string, AtomModel> allAtoms)
        {
            Projection = projectionAtom;
            _allAtoms = allAtoms;
        }

        public List<Reference> GetReferences()
        {
            var projectedAtoms = BuildProjectionSet();

            // Build a lookup of atoms that are the target dependency of other atoms
            var dependentAtomMap = projectedAtoms.ToDictionary(
                atom => atom.Name,
                targetAtom => (from relatedAtom in projectedAtoms
                               where relatedAtom.GetDependencies()
                                                .Contains(targetAtom.Name)
                               select relatedAtom).ToSet());

            var mutableAtomDependencyMap = projectedAtoms.ToDictionary(
                atom => atom.Name,
                atom => atom.GetDependencies().ToSet());

            var fullySatisfiedDependencies = SelectStartingAtom(mutableAtomDependencyMap, dependentAtomMap);

            // note: doesn't matter if this is a stack or queue, i think.
            var logicalReferenceOrder = new Queue<AtomModel>();

            // Now we pull elements out of the work stack
            while (fullySatisfiedDependencies.Any())
            {
                // anything on this stack should be resolved
                var reference = fullySatisfiedDependencies.Pop();
                logicalReferenceOrder.Enqueue(reference);

                foreach (var dependentAtom in dependentAtomMap[reference.Name])
                {
                    var remainingDependencies = mutableAtomDependencyMap[dependentAtom.Name];

                    // the remove check makes sure we make progress (don't infinte loop)
                    // also make sure there are no more remaining dependencies that MUST be resolved (they ended up in the projection set)
                    if (remainingDependencies.Remove(reference.Name) &&
                        !remainingDependencies.Any(dep => projectedAtoms.Contains(_allAtoms[dep])))
                    {
                        fullySatisfiedDependencies.Push(dependentAtom);
                    }
                }
            }

            if (mutableAtomDependencyMap.Any(kvp => kvp.Value.Any()))
            {
                // if there is anything with unresolved dependencies... how did that happen?!
                throw new Exception("Some how we couldn't resolve the depedency graph");
            }

            var references = ResolveReferences(logicalReferenceOrder, projectedAtoms);

            return references;
        }

        private static List<Reference> ResolveReferences(Queue<AtomModel> sortedReferences, HashSet<AtomModel> projectedAtoms)
        {
            // Pick the first ref to be the atomModel
            var primaryRef = new SimpleReference(sortedReferences.Dequeue());

            var references = new List<Reference>
            {
                primaryRef
            };
            var referenceMap = new Dictionary<string, Reference>
            {
                { primaryRef.Name, primaryRef }
            };

            var atomDependencyMap = projectedAtoms.ToDictionary(atom => atom.Name, atom => atom.GetDependencies());

            foreach (var atomRelation in sortedReferences)
            {
                var posibleReferences = atomDependencyMap[atomRelation.Name];

                if (!posibleReferences.Any())
                {
                    var r = new SimpleReference(atomRelation);
                    references.Add(r);
                    referenceMap.Add(r.Name, r);
                }
                else
                {
                    var referenceName = posibleReferences.First(referenceMap.ContainsKey);
                    var reference = referenceMap[referenceName];

                    var resolvedDependencies = posibleReferences.Where(referenceMap.ContainsKey)
                                                                .Select(p => referenceMap[p]);

                    var r = new ResolvedReference(atomRelation, resolvedDependencies);
                    references.Add(r);
                    referenceMap.Add(r.Name, r);
                }
            }
            return references;
        }

        public QueryPlan Build()
        {
            var references = GetReferences();

            var columns = GetProjectionColumns()
                .ToList();

            if (Projection.IgnoreConflicts)
            {
                columns = columns.DistinctBy(i => i.Name)
                                 .ToList();
            }
            QueryPlan plan = new QueryPlan
            {
                References = references,
                QueryMembers = columns,
                Filters = GetProjectionFilters(references).ToList()
            };

            return plan;
        }

        public IEnumerable<AliasedAtomMemberInfo> GetProjectionColumns()
        {
            return Projection.Tables.SelectMany(
                kv =>
                    {
                        List<AliasedAtomMemberInfo> projectionMembers = kv.Value.SelectMembers.Select(prop => _allAtoms[kv.Key].Members[prop])
                                                                            .Select(AliasedAtomMemberInfo.FromAtomMemberInfo)
                                                                            .ToList();
                        if (projectionMembers?.Count == 0)
                        {
                            if (!_allAtoms.ContainsKey(kv.Key))
                            {
                                throw new UnknownProjectionAtomException($"{kv.Key} relates to an unknown atom. Has it been defined?");
                            }
                            projectionMembers = _allAtoms[kv.Key].Members.Where(m => !m.HasFlag(MemberFlags.Generated))
                                                                 .Select(AliasedAtomMemberInfo.FromAtomMemberInfo)
                                                                 .ToList();
                        }

                        if (kv.Value.Aliases != null)
                        {
                            foreach (var alias in kv.Value.Aliases)
                            {
                                var requiresAliasMembmer = projectionMembers.FirstOrDefault(i => i.Name == alias.Value);

                                if (requiresAliasMembmer != null)
                                {
                                    requiresAliasMembmer.Name = alias.Key;
                                }
                            }
                        }
                        return projectionMembers;
                    });
        }

        public IEnumerable<AtomFilter> GetProjectionFilters(IEnumerable<Reference> references)
        {
            if (!Projection.NonDeleted)
            {
                yield break;
            }

            foreach (AtomMemberInfo filterMember in references.SelectMany(r => r.ReferenceTarget.Members.Where(m => m.HasFlag(MemberFlags.SoftDeleteTracking))))
            {
                yield return new AtomFilter
                {
                    FilterValue = "0",
                    Member = filterMember
                };
            }
        }

        private HashSet<AtomModel> BuildProjectionSet()
        {
            // ========= Build Graph =============
            // First get all the requested tables

            var projectedAtoms = _allAtoms.Values.Where(NeededByProjection)
                                          .ToSet();

            // and join the transitive references.
            // - this is so that if a projection requests A & C
            // - but the dep chain is:
            //      A -> B, B -> C
            // - then we grab B as a transitive reference

            IncludeTransitiveReferences(projectedAtoms);
            return projectedAtoms;
        }

        private void IncludeTransitiveReferences(HashSet<AtomModel> projectedAtoms)
        {
            var projectedAtomsCopy = projectedAtoms.ToArray();

            var transitiveClosures = from atom in projectedAtomsCopy
                                     from target in projectedAtomsCopy
                                     let path = DepthFirstSearch(atom, target)
                                     where path != null
                                     select path;

            foreach (var closure in transitiveClosures)
            {
                projectedAtoms.UnionWith(closure);
            }
        }

        private Stack<AtomModel> SelectStartingAtom(Dictionary<string, HashSet<string>> atomDependencyMap, Dictionary<string, HashSet<AtomModel>> dependentAtomMap)
        {
            var atomSet = atomDependencyMap.Where(kv => kv.Value.Count == 0)
                                           .Select(kv => _allAtoms[kv.Key])
                                           .ToStack();

            if (!atomSet.Any())
            {
                var atom = atomDependencyMap.OrderBy(kv => kv.Value.Count)
                                            .Select(kv => _allAtoms[kv.Key])
                                            .First();
                atomSet.Push(atom);
                atomDependencyMap[atom.Name].Clear();
                foreach (var upstreamAtom in dependentAtomMap.Values)
                {
                    upstreamAtom.Remove(atom);
                }
            }
            return atomSet;
        }

        private HashSet<AtomModel> DepthFirstSearch(AtomModel atom, AtomModel target)
        {
            return DepthFirstSearch(atom, target, new HashSet<string>());
        }

        private HashSet<AtomModel> DepthFirstSearch(AtomModel atom, AtomModel target, HashSet<string> allreadyChecked)
        {
            if (atom == target)
            {
                return new HashSet<AtomModel>
                {
                    atom
                };
            }

            // WTF allreadyChecked isn't used? 
            // this looks like it could be a perf issue at sompoint, 
            // should build up a set of checked deps... 
            foreach (var dep in atom.GetDependencies()
                                    .Where(d => !allreadyChecked.Contains(d))
                                    .Select(d => _allAtoms[d]))
            {
                var result = DepthFirstSearch(dep, target, allreadyChecked);
                if (result != null)
                {
                    result.Add(atom);
                    return result;
                }
            }

            return null;
        }

        private bool NeededByProjection(AtomModel atom)
        {
            return Projection.Tables.ContainsKey(atom.Name);
        }
    }
}
