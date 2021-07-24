using System;
using System.Text;

namespace UltimateTiming.DomainModel
{
    public class ReaderEvent
    {

        public DateTime EntryDate { get; set; }

        public string ReaderName { get; set; }

        public string FieldDelimiter { get; set; }

        public string LineDelimiter { get; set; }

        public string FieldNames { get; set; }

        public string FieldValues { get; set; }

        public bool ProcessedForFTP { get; set; }

        public bool ProcessedForWeb { get; set; }

        public override string ToString()
        {

            StringBuilder ret = new StringBuilder();
            try
            {

                ret.AppendLine("reader_name=\"" + ReaderName + "\"");
                ret.AppendLine("line_ending=\"" + LineDelimiter + "\"");
                ret.AppendLine("field_delim=\"" + FieldDelimiter + "\"");
                ret.AppendLine("field_names=\"" + FieldNames + "\"");
                ret.AppendLine("field_values=\"" + FieldValues + "\"");
            }
            catch { }

            return ret.ToString();
        }

        public string RawData { get; set; }
    }
}
