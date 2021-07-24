
using System;


namespace UltimateTiming.DomainModel.RaceModel
{

    public class Note
    {
        public int ID { get; set; }
        public DateTime DateAdded { get; set; }
        public string NoteText { get; set; }
        public bool Completed { get; set; }

    }
}
