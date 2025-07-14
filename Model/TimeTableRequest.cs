using System.Collections.Generic;
using System.Linq;

namespace Schedule.Model
{
    public class TimeTableRequest
    {
        public List<Faculty> Faculties { get; set; } = new();
        public List<ClassInput> Classes { get; set; } = new();
        public List<TeacherInput> Teachers { get; set; } = new();
        public List<Room> Rooms { get; set; } = new();

        public List<Teacher> GetTeachers()
        {
            return Teachers.Select(t => Teacher.FromInput(t)).ToList();
        }
    }
}
