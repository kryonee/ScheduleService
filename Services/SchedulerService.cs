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

        private bool TeacherIsAvailable(Teacher teacher, string day, int period)
        {
            foreach (var condition in teacher.Conditions)
            {
                switch (condition)
                {
                    case "NoMonday":
                        if (day == "Thứ hai") return false;
                        break;
                    case "NoTuesday":
                        if (day == "Thứ ba") return false;
                        break;
                    case "NoWednesday":
                        if (day == "Thứ tư") return false;
                        break;
                    case "NoThursday":
                        if (day == "Thứ năm") return false;
                        break;
                    case "NoFriday":
                        if (day == "Thứ sáu") return false;
                        break;
                    case "NoSaturday":
                        if (day == "Thứ bảy") return false;
                        break;
                    case "AvoidFriday":
                        if (day == "Thứ sáu") return false;
                        break;
                    case "NoEarlyPeriod":
                        if (period <= 2) return false;
                        break;
                    case "OnlyAfternoon":
                        if (period <= 3) return false;
                        break;
                }
            }
            return true;
        }

        private string GetTeacherAvailabilityReason(Teacher teacher, string day, int period)
        {
            var reasons = new List<string>();

            foreach (var condition in teacher.Conditions)
            {
                switch (condition)
                {
                    case "NoMonday" when day == "Thứ hai":
                        reasons.Add("Giáo viên không dạy thứ 2");
                        break;
                    case "NoTuesday" when day == "Thứ ba":
                        reasons.Add("Giáo viên không dạy thứ 3");
                        break;
                    case "NoWednesday" when day == "Thứ tư":
                        reasons.Add("Giáo viên không dạy thứ 4");
                        break;
                    case "NoThursday" when day == "Thứ năm":
                        reasons.Add("Giáo viên không dạy thứ 5");
                        break;
                    case "NoFriday" when day == "Thứ sáu":
                        reasons.Add("Giáo viên không dạy thứ 6");
                        break;
                    case "NoSaturday" when day == "Thứ bảy":
                        reasons.Add("Giáo viên không dạy thứ 7");
                        break;
                    case "AvoidFriday" when day == "Thứ sáu":
                        reasons.Add("Giáo viên tránh thứ 6");
                        break;
                    case "NoEarlyPeriod" when period <= 2:
                        reasons.Add("Giáo viên không dạy tiết sớm (tiết 1-2)");
                        break;
                    case "OnlyAfternoon" when period <= 3:
                        reasons.Add("Giáo viên chỉ dạy buổi chiều (từ tiết 4)");
                        break;
                }
            }

            return string.Join(", ", reasons);
        }

        private bool IsSlotAvailable(string slotKey, string className, string teacherName, string roomName)
        {
            // Check if the slot is already used by this class, teacher, or room
            return !usedSlots.Contains($"{slotKey}_{className}") &&
                   !usedSlots.Contains($"{slotKey}_{teacherName}") &&
                   !usedSlots.Contains($"{slotKey}_{roomName}");
        }

        private string GetSlotConflictReason(string slotKey, string className, string teacherName, string roomName)
        {
            var conflicts = new List<string>();

            if (usedSlots.Contains($"{slotKey}_{className}"))
                conflicts.Add($"Lớp {className} đã có lịch học");
            if (usedSlots.Contains($"{slotKey}_{teacherName}"))
                conflicts.Add($"Giáo viên {teacherName} đã có lịch dạy");
            if (usedSlots.Contains($"{slotKey}_{roomName}"))
                conflicts.Add($"Phòng {roomName} đã được sử dụng");

            return string.Join(", ", conflicts);
        }

        private void MarkSlotUsed(string slotKey, string className, string teacherName, string roomName)
        {
            usedSlots.Add($"{slotKey}_{className}");
            usedSlots.Add($"{slotKey}_{teacherName}");
            usedSlots.Add($"{slotKey}_{roomName}");
        }

        public List<ScheduledClass> Generate(TimeTableRequest input)
        {
            var results = new List<ScheduledClass>();
            var failedSchedules = new List<string>();
            var errorLog = new List<string>();
            string errorLogPath = "schedule_errors.log";

            foreach (var classItem in input.Classes)
            {
                foreach (var subjectName in classItem.Subjects)
                {
                    // Kiểm tra môn học có thuộc khoa của lớp không
                    var faculty = input.Faculties.FirstOrDefault(f => f.Name == classItem.Faculty);
                    if (faculty == null)
                    {
                        var error = $"❌ Không tìm thấy khoa {classItem.Faculty} cho lớp {classItem.Name}";
                        Console.WriteLine(error);
                        failedSchedules.Add(error);
                        errorLog.Add($"[Lớp: {classItem.Name} | Môn: {subjectName}] {error}");
                        continue;
                    }

                    if (!faculty.Subjects.Contains(subjectName))
                    {
                        var error = $"❌ Môn {subjectName} không thuộc khoa {classItem.Faculty} của lớp {classItem.Name}";
                        Console.WriteLine(error);
                        failedSchedules.Add(error);
                        errorLog.Add($"[Lớp: {classItem.Name} | Môn: {subjectName}] {error}");
                        continue;
                    }

                    // Tìm giáo viên trong cùng khoa
                    var availableTeachers = input.Teachers
                        .Where(t => t.Faculty == classItem.Faculty)
                        .ToList();

                    if (availableTeachers.Count == 0)
                    {
                        var error = $"❌ Không có giáo viên nào trong khoa {classItem.Faculty} để dạy môn {subjectName} cho lớp {classItem.Name}";
                        Console.WriteLine(error);
                        failedSchedules.Add(error);
                        errorLog.Add($"[Lớp: {classItem.Name} | Môn: {subjectName}] {error}");
                        continue;
                    }

                    // Tất cả phòng đều có thể sử dụng
                    var availableRooms = input.Rooms.ToList();

                    if (availableRooms.Count == 0)
                    {
                        var error = $"❌ Không có phòng học nào để xếp lịch cho môn {subjectName} của lớp {classItem.Name}";
                        Console.WriteLine(error);
                        failedSchedules.Add(error);
                        errorLog.Add($"[Lớp: {classItem.Name} | Môn: {subjectName}] {error}");
                        continue;
                    }

                    bool scheduled = false;
                    bool teacherAvailableAnySlot = false;
                    var teacherAvailabilityDetails = new List<string>();

                    var shuffledDays = Days.OrderBy(_ => Guid.NewGuid()).ToArray();
                    var shuffledPeriods = Periods.OrderBy(_ => Guid.NewGuid()).ToArray();

                    foreach (var day in shuffledDays)
                    {
                        foreach (var period in shuffledPeriods)
                        {
                            foreach (var teacher in availableTeachers)
                            {
                                if (!TeacherIsAvailable(teacher, day, period))
                                {
                                    var reason = GetTeacherAvailabilityReason(teacher, day, period);
                                    teacherAvailabilityDetails.Add($"GV {teacher.Name}: {reason} ({day} tiết {period})");
                                    continue;
                                }
                                teacherAvailableAnySlot = true;

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
                        if (!teacherAvailableAnySlot)
                        {
                            var error = $"❌ Không có giáo viên nào đủ điều kiện thời gian cho lớp {classItem.Name} môn {subjectName}";
                            Console.WriteLine(error);
                            Console.WriteLine("   Chi tiết ràng buộc giáo viên:");
                            errorLog.Add($"[Lớp: {classItem.Name} | Môn: {subjectName}] {error}");
                            errorLog.Add("   Chi tiết ràng buộc giáo viên:");
                            foreach (var detail in teacherAvailabilityDetails)
                            {
                                Console.WriteLine($"   - {detail}");
                                errorLog.Add($"   - {detail}");
                            }
                            failedSchedules.Add(error);
                        }
                        else
                        {
                            var error = $"❌ Tất cả slot đã bị chiếm, không thể xếp lịch cho lớp {classItem.Name} môn {subjectName}";
                            Console.WriteLine(error);
                            var firstDay = Days[0];
                            var firstPeriod = Periods[0];
                            var firstSlotKey = $"{firstDay}_{firstPeriod}";
                            var conflictReason = GetSlotConflictReason(firstSlotKey, classItem.Name,
                                availableTeachers.First().Name, availableRooms.First().Name);
                            var conflictDetail = $"   Ví dụ xung đột tại {firstDay} tiết {firstPeriod}: {conflictReason}";
                            Console.WriteLine(conflictDetail);
                            errorLog.Add($"[Lớp: {classItem.Name} | Môn: {subjectName}] {error}");
                            errorLog.Add(conflictDetail);
                            failedSchedules.Add(error);
                        }
                    }
                }
            }
            if (failedSchedules.Count > 0)
            {
                File.WriteAllLines(errorLogPath, errorLog);
                Console.WriteLine($"\n📄 Đã ghi log lỗi chi tiết vào: {errorLogPath}");
            }

            return results;
        }
    }
}
