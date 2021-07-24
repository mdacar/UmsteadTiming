﻿using System.Runtime.Caching;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Data.SqlClient;
using MetricsCollection;

namespace TimeEntryFactory
{

    
    internal class Race
    {

         

        public const string SqlConnectionString = "Server=tcp:bfoe9kr5v7.database.windows.net,1433;Database=UltimateTiming;User ID=umstead@bfoe9kr5v7;Password=TrailRun100;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";

        private const string sqlRace = "SELECT * FROM Race WHERE ID = (SELECT [Setting] FROM ApplicationSettings WHERE [Key] = 'CurrentRaceID')";
        private const string sqlRunners = @"SELECT RaceID, RunnerID, RunnerNumber, State, ID, Stopped, TagId
                                            FROM RaceXRunner 
                                            WHERE RaceID = (SELECT [Setting] FROm ApplicationSettings WHERE [Key] = 'CurrentRaceID')";
        private const string sqlTimeEntrySource = "SELECT ID, Description FROM TimeEntrySource WHERE RaceID = (SELECT [Setting] FROm ApplicationSettings WHERE [Key] = 'CurrentRaceID')";


        public static Race GetCurrentRace()
        {
            InfluxClient _influxClient = new InfluxClient();
            var startDate = DateTime.Now;
            Race race = MemoryCache.Default.Get("Current") as Race;
            if (race == null)
            {
                //Get it from the DB and load it into the cache
                using (var conn = new SqlConnection(SqlConnectionString))
                {
                    var races = (List<Race>)conn.Query<Race>(sqlRace);
                    race = races[0];

                    race.Runners = (List<Runner>)conn.Query<Runner>(sqlRunners);
                    race.TimeEntrySources = (List<TimeEntrySource>)conn.Query<TimeEntrySource>(sqlTimeEntrySource);
                    //Add it to the cache and have it expire after an hour
                    MemoryCache.Default.Add("Current", race, new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.AddHours(1) });
                }
            }
            var endDate = DateTime.Now;
            _influxClient.SendMetric($"{Worker.METRIC_PREFIX}_get_current_race_timing", (int)endDate.Subtract(startDate).TotalMilliseconds);

            return race;
        }

        public Guid ID { get; set; }
        public int RaceID { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        

        public List<Runner> Runners { get; set; }
        public List<TimeEntrySource> TimeEntrySources { get; set; }

    }
}
