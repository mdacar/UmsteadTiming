using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using TimeEntryProcessor.Processor.Models;
using TimeEntryProcessor.Processor.Services;

namespace TimeEntryProcessor.Tests
{
    [TestClass]
    public class TimeEntrySorterTests
    {
        [TestMethod]
        public void SortRunnerTimeEntries_HasDupes_MarksAsDupe()
        {
            TimeEntrySorter sorter = new TimeEntrySorter(GetStandardTimingLocations(), GetStandardCheckpoints());
            var runner = new Runner();

            var duplicateEntryGuid = new Guid();
            runner.TimeEntries.Add(new TimeEntry() { RFIDReaderID = 1, ReaderTimestamp = 123456789, ID = new Guid() });
            runner.TimeEntries.Add(new TimeEntry() { RFIDReaderID = 1, ReaderTimestamp = 123456789, ID = duplicateEntryGuid });

            //Check the pre-sort condition
            Assert.AreEqual(2, runner.TimeEntries.Count);

            sorter.Sort(runner);

            //Should have 1 invalid time entry
            Assert.AreEqual(1, runner.TimeEntries.Where(t => (TimeEntryStatus)t.TimeEntryStatusID == TimeEntryStatus.Invalid).Count());
            //Entry should be marked correctly
            var invalidEntry = runner.TimeEntries.Where(t => (TimeEntryStatus)t.TimeEntryStatusID == TimeEntryStatus.Invalid).FirstOrDefault();
            Assert.AreEqual("Duplicate", invalidEntry.StatusReason);
            Assert.AreEqual(duplicateEntryGuid, invalidEntry.ID);
           
        }


        [TestMethod]
        public void SortRunnerTimeEntries_HasEntryTooClose_MarksAsTooClose()
        {
            TimeEntrySorter sorter = GetStandardTimeEntrySorter();
            var runner = new Runner();

            var tooCloseEntryGuid = new Guid();
            runner.TimeEntries.Add(new TimeEntry() { RFIDReaderID = 1, ReaderTimestamp = 1616878662011279, ID = new Guid() });
            runner.TimeEntries.Add(new TimeEntry() { RFIDReaderID = 1, ReaderTimestamp = 1616879105395954, ID = tooCloseEntryGuid });

            //Check the pre-sort condition
            Assert.AreEqual(2, runner.TimeEntries.Count);

            sorter.Sort(runner);

            //Should have 1 invalid time entry
            Assert.AreEqual(1, runner.TimeEntries.Where(t => (TimeEntryStatus)t.TimeEntryStatusID == TimeEntryStatus.Invalid).Count());
            //Entry should be marked correctly
            var invalidEntry = runner.TimeEntries.Where(t => (TimeEntryStatus)t.TimeEntryStatusID == TimeEntryStatus.Invalid).FirstOrDefault();
            Assert.AreEqual("Too close to previous entry", invalidEntry.StatusReason);
            Assert.AreEqual(tooCloseEntryGuid, invalidEntry.ID);
        }

        private TimeEntrySorter GetStandardTimeEntrySorter()
        {
           return new TimeEntrySorter(GetStandardTimingLocations(), GetStandardCheckpoints(), new DateTime(2021, 3, 27, 6, 0, 0, DateTimeKind.Local));
        }

        #region Setup

        private IList<TimingLocation> GetStandardTimingLocations()
        {
            var timingLocations = new List<TimingLocation>();
            timingLocations.Add(
                new TimingLocation()
                {
                    ID = "C20B4523-76C0-491A-BB5A-1BB7ABEA5F20",
                    LocationName = "Headquarters",
                    ReaderName = "HQ_Reader"
                });
            timingLocations.Add(
                new TimingLocation()
                {
                    ID = "60F1DD35-BB20-4323-B2B1-60D84FEC3891",
                    LocationName = "Airport Spur",
                    ReaderName = "Airport_Reader"
                });
            timingLocations.Add(
                new TimingLocation()
                {
                    ID = "B6BC594E-E73E-47F6-B309-620E30741D60",
                    LocationName = "Aid Station",
                    ReaderName = "Aid_Reader"
                });

            return timingLocations;
        }

        private IList<Checkpoint> GetStandardCheckpoints()
        {
            var checkpoints = new List<Checkpoint>();

            #region Lap 1
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "B1837420-C961-4F9A-B7A3-A3FE983AA10B",
                    Description = "Race Start",
                    Sequence = 0,
                    Distance = 0,
                    TimingLocationID = "C20B4523-76C0-491A-BB5A-1BB7ABEA5F20",
                    TiminingLocationSequence = 0,
                    IsFinalLap = false,
                    SendNotifications = false,
                    ShortName = "STAR"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "4FE8C040-4F6B-4276-80ED-5AA209EDE39E",
                    Description = "Airport 1",
                    Sequence = 1,
                    Distance = 2,
                    TimingLocationID = "60F1DD35-BB20-4323-B2B1-60D84FEC3891",
                    TiminingLocationSequence = 0,
                    IsFinalLap = false,
                    SendNotifications = false,
                    ShortName = "AIR1"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "30F26963-9325-4CA7-8108-79F2E7AE889A",
                    Description = "AID1",
                    Sequence = 2,
                    Distance = 6.85m,
                    TimingLocationID = "B6BC594E-E73E-47F6-B309-620E30741D60",
                    TiminingLocationSequence = 0,
                    IsFinalLap = false,
                    SendNotifications = false,
                    ShortName = "AID1"
                });
            #endregion

            #region Lap 2
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "903CD5A3-2CE1-4AA9-9F1F-49C1391998EC",
                    Description = "Race Start",
                    Sequence = 3,
                    Distance = 12.50m,
                    TimingLocationID = "C20B4523-76C0-491A-BB5A-1BB7ABEA5F20",
                    TiminingLocationSequence = 1,
                    IsFinalLap = false,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} finished {{CheckPointName}} ({{Distance}} miles) in {{SplitTime}}",
                    ShortName = "LAP1"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "B9245F15-CC4E-426C-8A99-5341422D2685",
                    Description = "Airport 2",
                    Sequence = 4,
                    Distance = 14.5m,
                    TimingLocationID = "60F1DD35-BB20-4323-B2B1-60D84FEC3891",
                    TiminingLocationSequence = 1,
                    IsFinalLap = false,
                    SendNotifications = false,
                    ShortName = "AIR2"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "46FF8842-3EE3-4097-8511-D9505F8EC885",
                    Description = "AID2",
                    Sequence = 5,
                    Distance = 19.35m,
                    TimingLocationID = "B6BC594E-E73E-47F6-B309-620E30741D60",
                    TiminingLocationSequence = 1,
                    IsFinalLap = false,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} passed {{CheckPointName}} ({{Distance}} miles) in {{SplitTime}}",
                    ShortName = "AID2"
                });
            #endregion

            #region Lap 3
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "529EA805-4448-475F-AFE9-6FBC6F0A9A74",
                    Description = "LAP2",
                    Sequence = 6,
                    Distance = 25m,
                    TimingLocationID = "C20B4523-76C0-491A-BB5A-1BB7ABEA5F20",
                    TiminingLocationSequence = 2,
                    IsFinalLap = false,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} finished {{CheckPointName}} ({{Distance}} miles) in {{SplitTime}}",
                    ShortName = "LAP2"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "5666EAB8-70D7-46DD-9656-750AB33DAD6B",
                    Description = "Airport 3",
                    Sequence = 7,
                    Distance = 27m,
                    TimingLocationID = "60F1DD35-BB20-4323-B2B1-60D84FEC3891",
                    TiminingLocationSequence = 2,
                    IsFinalLap = false,
                    SendNotifications = false,
                    ShortName = "AIR3"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "FB9FE026-C887-4393-B81B-DDFCA7128639",
                    Description = "AID3",
                    Sequence = 8,
                    Distance = 31.85m,
                    TimingLocationID = "B6BC594E-E73E-47F6-B309-620E30741D60",
                    TiminingLocationSequence = 2,
                    IsFinalLap = false,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} passed {{CheckPointName}} ({{Distance}} miles) in {{SplitTime}}",
                    ShortName = "AID3"
                });
            #endregion

            #region Lap 4
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "78128335-EB99-4D0A-B754-F396EC162228",
                    Description = "Lap 3",
                    Sequence = 9,
                    Distance = 37.5m,
                    TimingLocationID = "C20B4523-76C0-491A-BB5A-1BB7ABEA5F20",
                    TiminingLocationSequence = 3,
                    IsFinalLap = false,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} finished {{CheckPointName}} ({{Distance}} miles) in {{SplitTime}}",
                    ShortName = "LAP3"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "62F067AE-91FD-41CD-91B0-9F3EE6CE67D7",
                    Description = "Airport 4",
                    Sequence = 10,
                    Distance = 39.5m,
                    TimingLocationID = "60F1DD35-BB20-4323-B2B1-60D84FEC3891",
                    TiminingLocationSequence = 3,
                    IsFinalLap = false,
                    SendNotifications = false,
                    ShortName = "AIR4"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "4EEA6615-0AD5-472B-9693-CF726348A6E0",
                    Description = "AID4",
                    Sequence = 11,
                    Distance = 44.35m,
                    TimingLocationID = "B6BC594E-E73E-47F6-B309-620E30741D60",
                    TiminingLocationSequence = 1,
                    IsFinalLap = false,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} passed {{CheckPointName}} ({{Distance}} miles) in {{SplitTime}}",
                    ShortName = "AID4"
                });
            #endregion

            #region Lap 5
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "85EF6A0A-22FB-4E29-9442-971E7B41815F",
                    Description = "Lap 4",
                    Sequence = 12,
                    Distance = 50m,
                    TimingLocationID = "C20B4523-76C0-491A-BB5A-1BB7ABEA5F20",
                    TiminingLocationSequence = 4,
                    IsFinalLap = false,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} finished {{CheckPointName}} ({{Distance}} miles) in {{SplitTime}}",
                    ShortName = "LAP4"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "722B72B1-E3BF-4AB0-AB10-A3C8BB8F5983",
                    Description = "Airport 5",
                    Sequence = 13,
                    Distance = 52m,
                    TimingLocationID = "60F1DD35-BB20-4323-B2B1-60D84FEC3891",
                    TiminingLocationSequence = 4,
                    IsFinalLap = false,
                    SendNotifications = false,
                    ShortName = "AIR5"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "91B77294-6CD9-464A-96DA-17D0E91CE384",
                    Description = "AID5",
                    Sequence = 14,
                    Distance = 56.85m,
                    TimingLocationID = "B6BC594E-E73E-47F6-B309-620E30741D60",
                    TiminingLocationSequence = 4,
                    IsFinalLap = false,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} passed {{CheckPointName}} ({{Distance}} miles) in {{SplitTime}}",
                    ShortName = "AID5"
                });
            #endregion

            #region Lap 6
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "59B38E7B-BA35-4CB0-A816-F9BDF7FB76C7",
                    Description = "Lap 5",
                    Sequence = 15,
                    Distance = 62.5m,
                    TimingLocationID = "C20B4523-76C0-491A-BB5A-1BB7ABEA5F20",
                    TiminingLocationSequence = 5,
                    IsFinalLap = false,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} finished {{CheckPointName}} ({{Distance}} miles) in {{SplitTime}}",
                    ShortName = "LAP5"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "B6272467-511D-4F76-9C0C-DA8AFC0BF2E0",
                    Description = "Airport 6",
                    Sequence = 16,
                    Distance = 65.5m,
                    TimingLocationID = "60F1DD35-BB20-4323-B2B1-60D84FEC3891",
                    TiminingLocationSequence = 5,
                    IsFinalLap = false,
                    SendNotifications = false,
                    ShortName = "AIR6"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "4600DD35-E915-47D6-950E-19EE1B5EA024",
                    Description = "AID6",
                    Sequence = 17,
                    Distance = 69.35m,
                    TimingLocationID = "B6BC594E-E73E-47F6-B309-620E30741D60",
                    TiminingLocationSequence = 5,
                    IsFinalLap = false,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} passed {{CheckPointName}} ({{Distance}} miles) in {{SplitTime}}",
                    ShortName = "AID6"
                });
            #endregion

            #region Lap 7
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "FEAD85EB-C1D3-4742-B4F0-CF266222C20F",
                    Description = "Lap 6",
                    Sequence = 18,
                    Distance = 75m,
                    TimingLocationID = "C20B4523-76C0-491A-BB5A-1BB7ABEA5F20",
                    TiminingLocationSequence = 6,
                    IsFinalLap = false,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} finished {{CheckPointName}} ({{Distance}} miles) in {{SplitTime}}",
                    ShortName = "LAP6"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "045FA181-31B2-4BF6-BB7D-6AFB6F477512",
                    Description = "Airport 7",
                    Sequence = 19,
                    Distance = 77m,
                    TimingLocationID = "60F1DD35-BB20-4323-B2B1-60D84FEC3891",
                    TiminingLocationSequence = 6,
                    IsFinalLap = false,
                    SendNotifications = false,
                    ShortName = "AIR7"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "9EA977E4-6158-4D6F-8A38-405526227E0F",
                    Description = "AID7",
                    Sequence = 20,
                    Distance = 81.85m,
                    TimingLocationID = "B6BC594E-E73E-47F6-B309-620E30741D60",
                    TiminingLocationSequence = 6,
                    IsFinalLap = false,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} passed {{CheckPointName}} ({{Distance}} miles) in {{SplitTime}}",
                    ShortName = "AID7"
                });
            #endregion

            #region Lap 8
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "F62C41BC-0BC8-4BDF-A990-354BF6C70B7B",
                    Description = "Lap 7",
                    Sequence = 21,
                    Distance = 87.5m,
                    TimingLocationID = "C20B4523-76C0-491A-BB5A-1BB7ABEA5F20",
                    TiminingLocationSequence = 7,
                    IsFinalLap = false,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} finished {{CheckPointName}} ({{Distance}} miles) in {{SplitTime}}",
                    ShortName = "LAP7"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "2438E441-0AA5-45E2-8067-55594F0F222C",
                    Description = "Airport 8",
                    Sequence = 22,
                    Distance = 89.5m,
                    TimingLocationID = "60F1DD35-BB20-4323-B2B1-60D84FEC3891",
                    TiminingLocationSequence = 7,
                    IsFinalLap = false,
                    SendNotifications = false,
                    ShortName = "AIR8"
                });
            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "5605A64E-F898-46FD-A0AD-FD2E598A542D",
                    Description = "AID8",
                    Sequence = 23,
                    Distance = 94.35m,
                    TimingLocationID = "B6BC594E-E73E-47F6-B309-620E30741D60",
                    TiminingLocationSequence = 7,
                    IsFinalLap = false,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} passed {{CheckPointName}} ({{Distance}} miles) in {{SplitTime}}",
                    ShortName = "AID7"
                });
            #endregion

            checkpoints.Add(
                new Checkpoint()
                {
                    ID = "71C2A930-8F31-4942-975B-CE779E0F58B5",
                    Description = "Finish",
                    Sequence = 24,
                    Distance = 100m,
                    TimingLocationID = "C20B4523-76C0-491A-BB5A-1BB7ABEA5F20",
                    TiminingLocationSequence = 7,
                    IsFinalLap = true,
                    SendNotifications = true,
                    SMSNotificationText = "{{RunnerName}} finished the Umstead 100 in {{SplitTime}}!",
                    ShortName = "FINS"
                });

            return checkpoints;
        }

        #endregion

    }
}
