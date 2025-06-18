using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule.Model
{
    public class Subject
    {
        public string Name { get; set; }
        public List<string> Dependency { get; set; } = new();
        public string RoomType { get; set; }
    }

}
