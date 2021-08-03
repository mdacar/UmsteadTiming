using System;
using System.Collections.Generic;
using System.Text;
using UltimateTiming.DomainModel;
using Dapper;
using System.Data.SqlClient;

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
                var sql = @"SELECT ID, Name, StartDate, Completed, RaceOrganizationID 
                            FROM Race 
                            WHERE ID = (SELECT [Setting] FROM ApplicationSettings WHERE [Key] = 'CurrentRaceID')";

                var race = conn.QuerySingle<Race>(sql);
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
