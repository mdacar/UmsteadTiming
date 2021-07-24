namespace UltimateTiming.DomainModel.Sorting
{
    public interface ITimeEntrySorter
    {

        TimeEntrySortResponse Sort(Runner runner);

    }
}
