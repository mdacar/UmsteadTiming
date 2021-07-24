using System;

namespace UltimateTiming.DomainModel
{
    public class RunnerNotFoundException : Exception
    {
        public RunnerNotFoundException(string tagId) : base(string.Format("Unable to locate a runner with Tag ID {0}", tagId))
        {
        }
    }
}
