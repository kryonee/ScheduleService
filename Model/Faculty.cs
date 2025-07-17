using System.Collections.Generic;

namespace Schedule.Model
{
    public class Faculty
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public List<string> Subjects { get; set; } = new();
    }
}