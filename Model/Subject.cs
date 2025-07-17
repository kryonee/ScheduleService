using System;

namespace Schedule.Model
{
    public class Subject
    {
        public string Id { get; set; }
        public string SubjectCode { get; set; }
        public string Name { get; set; }
        public int CreditPoint { get; set; }
        public string FacultyName { get; set; }
        public string FacultyId { get; set; }
        public int TotalHours { get; set; }
        public int? SubjectType { get; set; }
        public string Note { get; set; }
    }
}