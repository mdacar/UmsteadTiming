using System.Collections.Generic;

namespace UltimateTiming.DomainModel.PaceCalculation
{
    public interface IPaceCalc
    {

        void CalculatePace(Runner runner, Race race);

        IEnumerable<Split> CalculatePace(IEnumerable<Split> splits);
    }
}
