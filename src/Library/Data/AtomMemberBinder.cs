using System;
using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Data.Types;

namespace Atom.Data
{
    public static class AtomMemberBinder
    {
        public static IEnumerable<AtomModel> BindReferences(IEnumerable<AtomModel> src)
        {
            var atomModels = src.ToList();

            var allAtomsLookup = atomModels.ToDictionary(atom => atom.AdditionalInfo.Schema + "." + atom.Name);

            foreach (var atomModel in atomModels)
            {
                var haveReferences = atomModel.Members.Where(i => i.HasReference);

                foreach (var member in haveReferences)
                {
                    AtomModel relatingRoot;

                    string type = member.Atom.AdditionalInfo.Schema + "." + member.Reference.Name;

                    if (!allAtomsLookup.TryGetValue(type, out relatingRoot))
                    {
                        throw new InvalidOperationException($"Type not found: {type}");
                    }

                    string relatedMemberName = member.Reference.Member ?? member.Name;
                    member.Reference.Member = relatedMemberName;

                    var relatingMember = relatingRoot.Members.FirstOrDefault(i => i.Name == relatedMemberName);

                    if (relatingMember == null)
                    {
                        throw new Exception($"Unable to find related member named '{relatedMemberName}' on {type} from reference {member.Atom.Name}.");
                    }

                    member.Reference.TargetMember = relatingMember;
                    ReferenceMemberType referenceMemberType = (ReferenceMemberType)member.MemberType;
                    referenceMemberType.ReferenceType = relatingMember.MemberType;
                }

                bool? hasItems = atomModel.SearchableBy?.Any();
                if (hasItems.GetValueOrDefault())
                {
                    foreach (var searchBy in atomModel.SearchableBy)
                    {
                        var relation = allAtomsLookup[atomModel.AdditionalInfo.Schema + "." + searchBy.Name];

                        var member = relation.Members.FirstOrDefault(i => i.Name == searchBy.Member);

                        searchBy.TargetMember = member;
                    }
                }

                yield return atomModel;
            }
        }
    }
}
