using System;
using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Data.Types;
using Atom.Generation.Data;
using Atom.Generation.Extensions;
using Atom.Generation.Generators.Sql.Types;

namespace Atom.Generation.Generators.Sql.Sprocs
{
    public abstract class BaseStoredProcedureGenerator : ISqlGenerator<StoredProcedureResult>
    {
        protected AtomModel Atom { get; }

        public abstract StoredProcedureResult Generate();

        protected BaseStoredProcedureGenerator(AtomModel atom)
        {
            Atom = atom;
        }

        protected bool IsHiddenTemporal(AtomMemberInfo value)
        {
            if (Atom.AdditionalInfo.Temporal.HasTemporal.GetValueOrDefault() &&
                value.MemberType is MemberDateTime)
            {
                return value.HasFlag(MemberFlags.TemporalTracking);
            }
            return false;
        }

        protected bool MemberIsUseableAsInsertParameter(AtomMemberInfo member)
        {
            if (!member.HasFlag(MemberFlags.NotUpdateable))
            {
                return true;
            }

            if (member.Key &&
                !member.MemberType.CanSupplyDefaultCreationValue)
            {
                return true;
            }

            return member.HasReference;
        }

        protected string GetTypedSprocParam(AtomMemberInfo arg)
        {
            var optional =  Optionality(arg);

            if (arg.HasReference && arg.Reference.IsReferenceToHiddenPrimaryKey)
            {
                arg = arg.Reference.TargetAtomAlternateKey;
            }

            var typedParam = GetTypedVariable(arg);
            return typedParam + optional;
        }

        protected string GetTypedVariable(AtomMemberInfo arg)
        {
            return AtPrefix(arg.Name) + " " + arg.MemberType.Accept(new SqlTypeNameVistor());
        }

        private string Optionality(AtomMemberInfo atomMemberInfo)
        {
            if (!atomMemberInfo.Optional)
            {
                return string.Empty;
            }

            return " = NULL";
        }

        protected string SprocHeader(string schema, string tableName, string sprocSuffix, string sprocParameters)
        {
            return $@"SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
-- =============================================
-- Author:      Atom Generated
-- =============================================

CREATE PROCEDURE [{schema}].[{tableName}_{sprocSuffix}]
    {sprocParameters}
AS
BEGIN	
    SET NOCOUNT ON;";
        }

        protected AtomMemberInfo GetPrimaryKey()
        {
            return Atom.Members.First(i => i.IsPrimary);
        }

        protected AtomMemberInfo GetReturnKey()
        {
            var altKey = Atom.Members.FirstOrDefault(i => i.IsAltKey);

            if (altKey != null && !Atom.IsLookup)
            {
                return altKey;
            }

            return GetPrimaryKey();
        }


        protected bool IsKey(AtomMemberInfo value)
        {
            if (Atom.AdditionalInfo.ShouldHidePrimaryKey() ||
                Atom.Members.Any(i => i.IsAltKey))
            {
                return value.IsAltKey;
            }

            return value.IsPrimary;
        }

        private string AtPrefix(string arg)
        {
            return "@" + arg;
        }

        protected bool HasHiddenReference(AtomMemberInfo member)
        {
            return member.HasReference && member.Reference.IsReferenceToHiddenPrimaryKey;
        }

        protected string GenerateTemporalNowVariable(TemporalInfo temporalInfo)
        {
            return temporalInfo.HasTemporal.GetValueOrDefault() 
                ? "DECLARE @NOW " + (temporalInfo.UseDateTime2.GetValueOrDefault() ? "datetime2" : "datetime") + " = GETUTCDATE()" 
                : string.Empty;
        }

        protected string GetLookupSql(IEnumerable<AtomMemberInfo> members)
        {
            return string.Join(Environment.NewLine, members.Where(HasHiddenReference).Select(GetLookupSqlForMember).IndentAllLines(byTabs: 1));
        }

        private string GetLookupSqlForMember(AtomMemberInfo member)
        {
            var referenceAtom = member.Reference.TargetMember.Atom;
            var referenceKey = referenceAtom.Members.Single(m => m.Key);
            var lookup = $@"
DECLARE {GetTypedVariable(member)} = (
    SELECT {member.Name}
    FROM {referenceAtom.Name}
    WHERE [{referenceAtom.Name}].{referenceKey.Name} = @{referenceKey.Name}
)";

            if (!member.Optional)
            {
                lookup += $@"
IF (@{member.Name} IS NULL)
BEGIN
    RAISERROR('{referenceKey.Name} not found.', 11, 1);
    RETURN;
END";
            }
            return lookup;
        }

        protected string GetInsertFields(AtomMemberInfo arg)
        {
            if (arg.IsPrimary &&
                arg.MemberType is MemberLong)
            {
                return string.Empty;
            }

            if (arg.MemberType.CanSupplyDefaultCreationValue &&
                arg.MemberType is MemberGuid &&
                (arg.IsAltKey || arg.IsPrimary))
            {
                return "(NEWID())";
            }

            if (IsHiddenTemporal(arg))
            {
                return "@NOW";
            }

            if (arg.HasFlag(MemberFlags.SoftDeleteTracking))
            {
                return "0";
            }

            string paramName = "@" + arg.Name;
            return paramName;
        }
    }
}
