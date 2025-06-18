using System;
using System.Collections.Generic;
using System.Linq;
using Schedule.Model;

namespace Schedule.Service
{
    public class ScheduleSlot
    {
        public string Day { get; set; }     // Monday–Friday
        public int Period { get; set; }     // 1–5
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
        private readonly string[] Days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
        private readonly int[] Periods = new[] { 1, 2, 3, 4, 5 };

        private HashSet<string> usedSlots = new(); // Format: "Day_Period_Class", "Day_Period_Teacher", "Day_Period_Room"

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

                    foreach (var day in Days)
                    {
                        foreach (var period in Periods)
                        {
                            foreach (var teacher in availableTeachers)
                            {
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
                                        goto Scheduled; // break all
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
};
