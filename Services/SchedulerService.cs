using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using Schedule.Model;

namespace Schedule.Services
{
    public class ScheduleSlot
    {
        public string Day { get; set; }
        public int Period { get; set; }
    }

    public class ScheduledClass
    {
        public string ClassName { get; set; }
        public string Subject { get; set; }
        public string Teacher { get; set; }
        public string Room { get; set; }
        public string Day { get; set; }
        public int Period { get; set; }
    }

    public class SchedulerService
    {
        private readonly string[] Days = new[] { "Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy" };
        private readonly int[] Periods = new[] { 1, 2, 3, 4, 5, 6 };

        private HashSet<string> usedSlots = new();

        public List<ScheduledClass> Generate(TimeTableRequest input)
        {
            var results = new List<ScheduledClass>();

            foreach (var classItem in input.Classes)
            {
                foreach (var subjectName in classItem.Subjects)
                {
                    var subject = input.Subjects.FirstOrDefault(s => s.Name == subjectName);
                    if (subject == null) continue;

                    var availableTeachers = input.Teachers
                        .Where(t => t.Subjects.Contains(subjectName))
                        .ToList();

                    var availableRooms = input.Rooms
                        .Where(r => r.Type == subject.RoomType)
                        .ToList();

                    bool scheduled = false;

                    var shuffledDays = Days.OrderBy(_ => Guid.NewGuid()).ToArray();
                    var shuffledPeriods = Periods.OrderBy(_ => Guid.NewGuid()).ToArray();

                    foreach (var day in shuffledDays)
                    {
                        foreach (var period in shuffledPeriods)
                        {
                            foreach (var teacher in availableTeachers)
                            {
                                if (!TeacherIsAvailable(teacher, day, period))
                                    continue;

                                foreach (var room in availableRooms)
                                {
                                    string slotKey = $"{day}_{period}";

                                    if (IsSlotAvailable(slotKey, classItem.Name, teacher.Name, room.Name))
                                    {
                                        MarkSlotUsed(slotKey, classItem.Name, teacher.Name, room.Name);

                                        results.Add(new ScheduledClass
                                        {
                                            ClassName = classItem.Name,
                                            Subject = subjectName,
                                            Teacher = teacher.Name,
                                            Room = room.Name,
                                            Day = day,
                                            Period = period
                                        });

                                        scheduled = true;
                                        goto Scheduled;
                                    }
                                }
                            }
                        }
                    }

                Scheduled:
                    if (!scheduled)
                    {
                        Console.WriteLine($"Không thể xếp lịch cho lớp {classItem.Name} môn {subjectName}");
                    }
                }
            }
          
            return results;
        }

        private bool TeacherIsAvailable(Teacher teacher, string day, int period)
        {
            if (teacher.Conditions == null || teacher.Conditions.Count == 0)
                return true;

            if (teacher.Conditions.Contains("NoMonday") && day == "Thứ hai")
                return false;

            if (teacher.Conditions.Contains("OnlyAfternoon") && period < 4)
                return false;

            if (teacher.Conditions.Contains("AvoidFriday") && day == "Thứ sáu" && period < 6)
                return false;

            if (teacher.Conditions.Contains("NoEarlyPeriod") && period == 1)
                return false;

            return true;
        }


        private bool IsSlotAvailable(string slot, string className, string teacherName, string roomName)
        {
            return !usedSlots.Contains($"{slot}_CLASS_{className}") &&
                   !usedSlots.Contains($"{slot}_TEACHER_{teacherName}") &&
                   !usedSlots.Contains($"{slot}_ROOM_{roomName}");
        }

        private void MarkSlotUsed(string slot, string className, string teacherName, string roomName)
        {
            usedSlots.Add($"{slot}_CLASS_{className}");
            usedSlots.Add($"{slot}_TEACHER_{teacherName}");
            usedSlots.Add($"{slot}_ROOM_{roomName}");
        }
    }
}
