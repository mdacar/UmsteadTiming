using System.Linq;

namespace UltimateTiming.DomainModel.PlaceCalculation
{
    public class PlaceCalculator
    {

        public void CalculatePlaces(Race race)
        {
            foreach (var checkPoint in race.CheckPoints)
            {
                //Get all runners at each checkpoint.
                var runners = from r in race.Runners
                              where r.GetSplit(checkPoint.Id) != null
                              orderby r.GetSplit(checkPoint.Id).ElapsedMilliseconds
                              select r;

                int place = 1;
                foreach (var runner in runners)
                {
                    runner.GetSplit(checkPoint.Id).PlaceAtCheckPoint = place++;
                }
            }
        }


    }
}
