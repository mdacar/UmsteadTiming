using System;

namespace UltimateTiming.DomainModel
{
    [Flags]
    public enum TimeEntryStatus
    {
        Unknown = 5,
        Valid = 1,
        Error = 2,
        Invalid = 3,
        ModifiedValid = 4,
        Secondary = 6
    }
}
