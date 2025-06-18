using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule.Model
{
    public class TimeTableRequest
    {
        public List<Subject> Subjects { get; set; }
        public List<ClassInput> Classes { get; set; }
        public List<Teacher> Teachers { get; set; }
        public List<Room> Rooms { get; set; }
    }

}
