using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule.Model
{
    public class Teacher
    {
        public string Name { get; set; }
        public List<string> Subjects { get; set; }
        public List<string> Conditions { get; set; } = new(); // ví dụ: ["NoMonday", "OnlyAfternoon"]
    }

}
