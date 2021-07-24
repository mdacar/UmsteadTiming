using System;

namespace UltimateTiming.DomainModel
{
    public class ReaderLogEntry
    {
        public DateTime EntryDate { get; set; }
        public string Id { get; set; }
        public bool ProcessedForWeb { get; set; }
        public bool ProcessedForFTP { get; set; }
        public string ReaderName { get; set; }
        public string FieldNames { get; set; }
        public string FieldData { get; set; }
        public string FieldDelimiter { get; set; }
        public string RowDelimiter { get; set; }


        public string FTPFileName { get; set; }

        public override string ToString()
        {
            return string.Format("Entry Date: {0}\nId: {1}\nProcessedForWeb: {2}\nProcessedForFTP: {3}\nReaderName: {4}\nFieldNames: {5}\nFieldData: {6}\nFieldDelimiter: {7}\nRowDelimiter: {8}",
                EntryDate.ToString(), Id, ProcessedForWeb, ProcessedForFTP, ReaderName, FieldNames, FieldData, FieldDelimiter, RowDelimiter);
        }
    }
}
