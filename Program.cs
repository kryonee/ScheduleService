using Schedule.Controllers;
using Schedule.Model;
using System;
using System.Text.Json;
using System.Linq;
using System.IO;
using Schedule.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var teachers = InputLoader.LoadTeachers("API_Demo/Teacher.json");
        var subjects = InputLoader.LoadSubjects("API_Demo/Subject.json");
        var rooms = InputLoader.LoadRooms("API_Demo/Room.json");
        var faculties = InputLoader.LoadFaculties("API_Demo/Faculty.json");

        var request = new TimeTableRequest
        {
            Faculties = faculties,
            Rooms = rooms,
        
            Teachers = teachers.Select(t => new TeacherInput { Name = t.Name, Faculty = t.Faculty }).ToList(),
            Classes = faculties.Select(f => new ClassInput { Name = $"Lop_{f.Code}", Faculty = f.Name, Subjects = subjects.Where(s => s.FacultyId == f.Id).Select(s => s.Name).ToList() }).ToList()
        };

        var scheduler = new SchedulerService();
        var schedule = scheduler.Generate(request);

        var weekJson = new
        {
            weeks = new[]
            {
                new {
                    weekNumber = 1,
                    startDate = "2024-09-02",
                    endDate = "2024-09-08",
                    schedules = schedule.Select(s => new {
                        date = s.Day,
                        period = s.Period,
                        subject = s.Subject,
                        teacher = s.Teacher,
                        room = s.Room,
                        @class = s.ClassName
                    }).ToList()
                }
            }
        };
        File.WriteAllText("weekly_timetable.json", JsonSerializer.Serialize(weekJson, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Đã xuất thời khoá biểu tuần ra file: weekly_timetable.json");

        var monthJson = new
        {
            months = new[]
            {
                new {
                    month = 9,
                    year = 2024,
                    schedules = schedule.Select(s => new {
                        date = s.Day,
                        period = s.Period,
                        subject = s.Subject,
                        teacher = s.Teacher,
                        room = s.Room,
                        @class = s.ClassName
                    }).ToList()
                }
            }
        };
        File.WriteAllText("monthly_timetable.json", JsonSerializer.Serialize(monthJson, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Đã xuất thời khoá biểu tháng ra file: monthly_timetable.json");

        var semesterJson = new
        {
            semester = "2024-2025 HK1",
            schedules = schedule.Select(s => new
            {
                date = s.Day,
                period = s.Period,
                subject = s.Subject,
                teacher = s.Teacher,
                room = s.Room,
                @class = s.ClassName
            }).ToList()
        };
        File.WriteAllText("semester_timetable.json", JsonSerializer.Serialize(semesterJson, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Đã xuất thời khoá biểu học kỳ ra file: semester_timetable.json");
    }
}
