using System;
using Dapper;
using System.Data.SqlClient;
using TimeEntryProcessor.Processor.Models;

namespace TimeEntryProcessor.Processor
{
    internal class RaceRepository
    {
        private readonly string SqlConnectionString;

        public RaceRepository(string connString)
        {
            SqlConnectionString = connString;
        }

        public Race GetCurrentRace()
        {
            using (var conn = new SqlConnection(SqlConnectionString))
            {
                var sql = @"SELECT CONVERT(varchar(36), ID) AS [ID], Name, StartDate, Completed, RaceOrganizationID 
                            FROM Race 
                            WHERE ID = (SELECT [Setting] FROM ApplicationSettings WHERE [Key] = 'CurrentRaceID')";

                

                var race = conn.QuerySingle<Race>(sql);

                //TimeEntrySources
                var timeEntrySources = conn.Query<TimeEntrySource>($"SELECT [ID] ,[Description] ,CONVERT(varchar(36), [RaceID]) AS 'RaceID' FROM TimeEntrySource WHERE RaceID = '{race.ID}'");
                race.AddTimeEntrySources(timeEntrySources);
                
                //RFIDReaders
                var readers = conn.Query<RFIDReader>("SELECT * FROM RFIDReader");
                race.AddRFIDReaders(readers);

                //TimingLocations
                var timingLocations = conn.Query<TimingLocation>($"SELECT CONVERT(varchar(36), ID) AS [ID], CONVERT(varchar(36), RaceID) AS [RaceID],[LocationName] AS [Description],[ReaderName] AS [Code] FROM TimingLocation WHERE RaceID = '{race.ID}'");
                race.AddTimingLocations(timingLocations);

                //Checkpoints
                var checkpointSql = $@"SELECT CONVERT(varchar(36), ID) AS [ID]
                                      ,CONVERT(varchar(36), RaceID) AS [RaceID]
                                      ,[Description]
                                      ,[Sequence]
                                      ,[Distance]
                                      ,[MinimumTimeFromStart]
                                      ,[MaximumTimeFromStart]
                                      ,[Elevation]
                                      ,CONVERT(varchar(36), TimingLocationID) AS [TimingLocationID]
                                      ,[TimingLocationSequence]
                                      ,[ShortName]
                                      ,[SMSNotificationText]
                                      ,[SendNotifications]
                                  FROM [UltimateTiming].[dbo].[RaceCheckPoint] 
                                  WHERE RaceID = '{race.ID}'";

                var checkpoints = conn.Query<Checkpoint>(checkpointSql);
                race.AddCheckPoints(checkpoints);

                //Runners
//                var runnerSql = $@"SELECT CONVERT(varchar(36), x.ID) AS 'ID', x.RunnerNumber, x.State AS 'RunnerState', x.TagID, x.[Stopped], rn.FirstName, rn.LastName, 
//                                    rn.BirthDate, rn.City, rn.State, x.Past100MileFinishes, x.Past50MileFinishes
//	                            FROM Runners rn WITH (NOLOCK) INNER JOIN RaceXRunner x WITH (NOLOCK) ON rn.ID = x.RunnerID
//	                            WHERE x.RaceID = '{race.ID}'
//                                ORDER BY x.RunnerNumber";
//                var runners = conn.Query<UltimateTiming.DomainModel.Runner>(runnerSql);
//                race.AddRunners(runners);

//                //Time entries
//                var timeEntrySql = $@"SELECT CONVERT(varchar(36), rte.ID) AS [ID], CONVERT(varchar(36), rte.RaceXRunnerID) AS [RaceXRunnerID], 
//                                        rte.ElapsedTime, rte.AbsoluteTime, rte.ReaderTimestamp, rr.ReaderName,
//	                                    rte.RFIDReaderID, rte.TimeEntryStatusID AS [Status], rte.StatusReason, rte.TimeEntrySourceID, rte.TagType
//                                    FROm RunnerTimeEntry rte INNER JOIN RaceXRunner rxr ON rte.RaceXRunnerID = rxr.ID
//                                        INNER JOIN RFIDReader rr ON rr.ID = rte.RFIDReaderID
//                                    WHERE rxr.RaceID = '{race.Id}'";
//                var timeEntries = conn.Query<UltimateTiming.DomainModel.TimeEntry>(timeEntrySql).AsList();
//                race.AddTimeEntries(timeEntries);

//#if DEBUG
//                race.SortRunnersIntoPlaces();
//                foreach (var runner in race.Runners)
//                {
//                    if (runner.Splits.Count > 0)
//                    {
//                        Console.WriteLine($"{runner.Place} ({runner.Number} - {runner.FullName}) {runner.LastSplit.CheckPoint.TotalMiles} {runner.LastSplit.ElapsedTime}");
//                    }
//                }
//#endif

                return race;
            }

            
        }

        public Guid SaveTimeEntry(TimeEntry timeEntry)
        {
            var storedProc = "[RunnerTimeEntry_upcert]";

            var parameters = new { 
                ID = timeEntry.ID, 
                RaceXRunnerID = timeEntry.RaceXRunnerID,
                ElapsedTime = timeEntry.ElapsedTime,
                AbsoluteTime = timeEntry.AbsoluteTime,
                ReaderTimestamp = timeEntry.ReaderTimestamp,
                RFIDReaderID = timeEntry.RFIDReaderID,
                TimeEntryStatusID = timeEntry.TimeEntryStatusID,
                StatusReason = string.Empty,
                TimeEntrySourceID = timeEntry.TimeEntrySource,
                TagType = timeEntry.TagType
            };

            using (var conn = new SqlConnection(SqlConnectionString))
            {
                conn.Execute(storedProc, parameters, commandType: System.Data.CommandType.StoredProcedure);
            }

            return timeEntry.ID;
        }

    }
}
