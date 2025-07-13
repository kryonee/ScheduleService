using System.Collections.Generic;

namespace Schedule.Model
{
    public class TimeTableRequest
    {
        public List<Faculty> Faculties { get; set; } = new();
        public List<ClassInput> Classes { get; set; } = new();
        public List<Teacher> Teachers { get; set; } = new();
        public List<Room> Rooms { get; set; } = new();
    }
}
