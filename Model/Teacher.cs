using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule.Model
{
    public class TeacherInput
    {
        public string Name { get; set; }
        public string Faculty { get; set; }
        public List<string> Conditions { get; set; } = new();
    }

    public class Teacher
    {
        public string Id { get; set; }
        public string FacultyId { get; set; }
        public string UserInternalId { get; set; }
        public bool IsActived { get; set; }
        public string Name { get; set; }
        public string Faculty { get; set; }
        public bool NoMonday { get; set; }
        public bool NoTuesday { get; set; }
        public bool NoWednesday { get; set; }
        public bool NoThursday { get; set; }
        public bool NoFriday { get; set; }
        public bool NoSaturday { get; set; }
        public bool NoSunday { get; set; }
        public bool AvoidFriday { get; set; }
        public bool OnlyMorning { get; set; }
        public bool OnlyAfternoon { get; set; }

        public static Teacher FromInput(TeacherInput input)
        {
            var teacher = new Teacher
            {
                Name = input.Name,
                Faculty = input.Faculty
            };

            foreach (var condition in input.Conditions)
            {
                switch (condition)
                {
                    case "NoMonday":
                        teacher.NoMonday = true;
                        break;
                    case "NoTuesday":
                        teacher.NoTuesday = true;
                        break;
                    case "NoWednesday":
                        teacher.NoWednesday = true;
                        break;
                    case "NoThursday":
                        teacher.NoThursday = true;
                        break;
                    case "NoFriday":
                        teacher.NoFriday = true;
                        break;
                    case "NoSaturday":
                        teacher.NoSaturday = true;
                        break;
                    case "NoSunday":
                        teacher.NoSunday = true;
                        break;
                    case "AvoidFriday":
                        teacher.AvoidFriday = true;
                        break;
                    case "OnlyMorning":
                        teacher.OnlyMorning = true;
                        break;
                    case "OnlyAfternoon":
                        teacher.OnlyAfternoon = true;
                        break;
                }
            }

            return teacher;
        }
    }
}
