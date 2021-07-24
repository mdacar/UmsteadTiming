namespace UltimateTiming.DomainModel
{
    public class TimeEntrySource
    {
        public static string SOURCE_RFIDReader = "RFID Reader";
        public static string SOURCE_ULTFile = "Ult File";
        public static string SOURCE_SIM = "Simulation File";
        public static string SOURCE_MANUAL = "Manual Entry";
        public static string SOURCE_BACKUP_TAG = "Backup Tag";

        public int Id { get; set; }
        public string Description { get; set; }
        public string RaceId { get; set; }
    }
}
