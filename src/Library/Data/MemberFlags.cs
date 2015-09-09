using System;

namespace Atom.Data
{
    [Flags]
    enum MemberFlags
    {
        None = 0,
        Generated = 1,
        SoftDeleteTracking = 2,
        NotUpdateable = 4,
        TemporalTracking = 8,
        CreatedDateTimeTracking = 16 | TemporalTracking | NotUpdateable,
        LastModifiedDateTimetracking = 32 | TemporalTracking | NotUpdateable,
        Hidden = 64,
        PrimaryKey = 128 | NotUpdateable,
        Key = 256 | NotUpdateable,
        HiddenId = PrimaryKey | Hidden | Generated,
    }
}
