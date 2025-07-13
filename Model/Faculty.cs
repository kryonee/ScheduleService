using System.Collections.Generic;

namespace Schedule.Model
{
    public class Faculty
    {
        public string Name { get; set; }
        public List<string> Subjects { get; set; } = new();
    }
}