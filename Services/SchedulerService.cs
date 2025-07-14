using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using Schedule.Model;
using OfficeOpenXml;
using System.Drawing;

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
        private readonly int[] Periods = new[] { 1, 2 }; // 1: Morning, 2: Afternoon

        private HashSet<string> usedSlots = new();

        private bool TeacherIsAvailable(Teacher teacher, string day, int period)
        {
            if (teacher.NoMonday && day == "Thứ hai") return false;
            if (teacher.NoTuesday && day == "Thứ ba") return false;
            if (teacher.NoWednesday && day == "Thứ tư") return false;
            if (teacher.NoThursday && day == "Thứ năm") return false;
            if (teacher.NoFriday && day == "Thứ sáu") return false;
            if (teacher.NoSaturday && day == "Thứ bảy") return false;
            if (teacher.NoSunday && day == "Chủ nhật") return false;
            if (teacher.AvoidFriday && day == "Thứ sáu") return false;
            if (teacher.OnlyMorning && period != 1) return false; // Chỉ dạy buổi sáng
            if (teacher.OnlyAfternoon && period != 2) return false; // Chỉ dạy buổi chiều
            return true;
        }

        private string GetTeacherAvailabilityReason(Teacher teacher, string day, int period)
        {
            var reasons = new List<string>();
            if (teacher.NoMonday && day == "Thứ hai")
                reasons.Add("Giáo viên không dạy thứ 2");
            if (teacher.NoTuesday && day == "Thứ ba")
                reasons.Add("Giáo viên không dạy thứ 3");
            if (teacher.NoWednesday && day == "Thứ tư")
                reasons.Add("Giáo viên không dạy thứ 4");
            if (teacher.NoThursday && day == "Thứ năm")
                reasons.Add("Giáo viên không dạy thứ 5");
            if (teacher.NoFriday && day == "Thứ sáu")
                reasons.Add("Giáo viên không dạy thứ 6");
            if (teacher.NoSaturday && day == "Thứ bảy")
                reasons.Add("Giáo viên không dạy thứ 7");
            if (teacher.NoSunday && day == "Chủ nhật")
                reasons.Add("Giáo viên không dạy chủ nhật");
            if (teacher.AvoidFriday && day == "Thứ sáu")
                reasons.Add("Giáo viên tránh thứ 6");
            if (teacher.OnlyMorning && period != 1)
                reasons.Add("Giáo viên chỉ dạy buổi sáng");
            if (teacher.OnlyAfternoon && period != 2)
                reasons.Add("Giáo viên chỉ dạy buổi chiều");
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
                    var availableTeachers = input.GetTeachers()
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

                            // Kiểm tra tất cả các slot để tìm xung đột
                            var conflictDetails = new List<string>();
                            foreach (var day in Days)
                            {
                                foreach (var period in Periods)
                                {
                                    var slotKey = $"{day}_{period}";
                                    var conflictReason = GetSlotConflictReason(slotKey, classItem.Name,
                                        availableTeachers.First().Name, availableRooms.First().Name);
                                    if (!string.IsNullOrEmpty(conflictReason))
                                    {
                                        conflictDetails.Add($"   {day} tiết {period}: {conflictReason}");
                                    }
                                }
                            }

                            // Hiển thị tất cả xung đột tìm được
                            if (conflictDetails.Count > 0)
                            {
                                Console.WriteLine("   Chi tiết xung đột:");
                                foreach (var detail in conflictDetails.Take(5)) // Giới hạn hiển thị 5 xung đột đầu
                                {
                                    Console.WriteLine(detail);
                                }
                                if (conflictDetails.Count > 5)
                                {
                                    Console.WriteLine($"   ... và {conflictDetails.Count - 5} xung đột khác");
                                }
                            }

                            errorLog.Add($"[Lớp: {classItem.Name} | Môn: {subjectName}] {error}");
                            foreach (var detail in conflictDetails)
                            {
                                errorLog.Add(detail);
                            }
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

        public void ExportToExcel(List<ScheduledClass> schedule, string filePath = "schedule.xlsx")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Lịch học");

                // Định dạng header
                worksheet.Cells[1, 1].Value = "Thứ";
                worksheet.Cells[1, 2].Value = "Tiết";
                worksheet.Cells[1, 3].Value = "Lớp";
                worksheet.Cells[1, 4].Value = "Môn học";
                worksheet.Cells[1, 5].Value = "Giáo viên";
                worksheet.Cells[1, 6].Value = "Phòng học";

                // Định dạng header
                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                }

                // Thêm dữ liệu
                int row = 2;
                foreach (var item in schedule.OrderBy(s => s.Day).ThenBy(s => s.Period).ThenBy(s => s.ClassName))
                {
                    worksheet.Cells[row, 1].Value = item.Day;
                    worksheet.Cells[row, 2].Value = item.Period == 1 ? "Sáng" : "Chiều";
                    worksheet.Cells[row, 3].Value = item.ClassName;
                    worksheet.Cells[row, 4].Value = item.Subject;
                    worksheet.Cells[row, 5].Value = item.Teacher;
                    worksheet.Cells[row, 6].Value = item.Room;

                    // Định dạng border cho từng dòng
                    using (var range = worksheet.Cells[row, 1, row, 6])
                    {
                        range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    row++;
                }

                // Tự động điều chỉnh độ rộng cột
                worksheet.Cells.AutoFitColumns();

                // Lưu file
                package.SaveAs(new FileInfo(filePath));
            }

            Console.WriteLine($"📊 Đã xuất lịch học ra file Excel: {filePath}");
        }

        public void ExportTimetableExcel(List<ScheduledClass> schedule, string filePath = "timetable.xlsx")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var days = new[] { "Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy" };
            var periods = new[] { 1, 2 };
            var periodNames = new[] { "Sáng", "Chiều" };

            // Lấy danh sách lớp
            var classNames = schedule.Select(s => s.ClassName).Distinct().OrderBy(x => x).ToList();

            using (var package = new ExcelPackage())
            {
                foreach (var className in classNames)
                {
                    var worksheet = package.Workbook.Worksheets.Add(className);

                    // Header
                    worksheet.Cells[1, 1].Value = "Tiết/Thứ";
                    for (int d = 0; d < days.Length; d++)
                    {
                        worksheet.Cells[1, d + 2].Value = days[d];
                    }
                    worksheet.Row(1).Style.Font.Bold = true;

                    // Ghi từng tiết
                    for (int p = 0; p < periods.Length; p++)
                    {
                        worksheet.Cells[p + 2, 1].Value = periodNames[p];
                        for (int d = 0; d < days.Length; d++)
                        {
                            var slot = schedule.FirstOrDefault(s => s.ClassName == className && s.Day == days[d] && s.Period == periods[p]);
                            if (slot != null)
                            {
                                worksheet.Cells[p + 2, d + 2].Value = $"{slot.Subject}\nGV: {slot.Teacher}\nPhòng: {slot.Room}";
                            }
                            else
                            {
                                worksheet.Cells[p + 2, d + 2].Value = "";
                            }
                        }
                    }

                    worksheet.Cells.AutoFitColumns();
                    worksheet.Cells.Style.WrapText = true;
                }
                package.SaveAs(new FileInfo(filePath));
            }
            Console.WriteLine($"📊 Đã xuất thời khoá biểu dạng bảng ra file: {filePath}");
        }

        public void ExportTeacherTimetableExcel(List<ScheduledClass> schedule, string filePath = "timetable_teachers.xlsx")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var days = new[] { "Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy" };
            var periods = new[] { 1, 2 };
            var periodNames = new[] { "Sáng", "Chiều" };
            var teacherNames = schedule.Select(s => s.Teacher).Distinct().OrderBy(x => x).ToList();
            using (var package = new ExcelPackage())
            {
                foreach (var teacher in teacherNames)
                {
                    var worksheet = package.Workbook.Worksheets.Add(teacher);
                    worksheet.Cells[1, 1].Value = "Tiết/Thứ";
                    for (int d = 0; d < days.Length; d++)
                        worksheet.Cells[1, d + 2].Value = days[d];
                    worksheet.Row(1).Style.Font.Bold = true;
                    for (int p = 0; p < periods.Length; p++)
                    {
                        worksheet.Cells[p + 2, 1].Value = periodNames[p];
                        for (int d = 0; d < days.Length; d++)
                        {
                            var slot = schedule.FirstOrDefault(s => s.Teacher == teacher && s.Day == days[d] && s.Period == periods[p]);
                            if (slot != null)
                                worksheet.Cells[p + 2, d + 2].Value = $"{slot.ClassName}\n{slot.Subject}\nPhòng: {slot.Room}";
                            else
                                worksheet.Cells[p + 2, d + 2].Value = "";
                        }
                    }
                    worksheet.Cells.AutoFitColumns();
                    worksheet.Cells.Style.WrapText = true;
                }
                package.SaveAs(new FileInfo(filePath));
            }
            Console.WriteLine($"📊 Đã xuất thời khoá biểu giáo viên ra file: {filePath}");
        }

        public void ExportRoomTimetableExcel(List<ScheduledClass> schedule, string filePath = "timetable_rooms.xlsx")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var days = new[] { "Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy" };
            var periods = new[] { 1, 2 };
            var periodNames = new[] { "Sáng", "Chiều" };
            var roomNames = schedule.Select(s => s.Room).Distinct().OrderBy(x => x).ToList();
            using (var package = new ExcelPackage())
            {
                foreach (var room in roomNames)
                {
                    var worksheet = package.Workbook.Worksheets.Add(room);
                    worksheet.Cells[1, 1].Value = "Tiết/Thứ";
                    for (int d = 0; d < days.Length; d++)
                        worksheet.Cells[1, d + 2].Value = days[d];
                    worksheet.Row(1).Style.Font.Bold = true;
                    for (int p = 0; p < periods.Length; p++)
                    {
                        worksheet.Cells[p + 2, 1].Value = periodNames[p];
                        for (int d = 0; d < days.Length; d++)
                        {
                            var slot = schedule.FirstOrDefault(s => s.Room == room && s.Day == days[d] && s.Period == periods[p]);
                            if (slot != null)
                                worksheet.Cells[p + 2, d + 2].Value = $"{slot.ClassName}\n{slot.Subject}\nGV: {slot.Teacher}";
                            else
                                worksheet.Cells[p + 2, d + 2].Value = "";
                        }
                    }
                    worksheet.Cells.AutoFitColumns();
                    worksheet.Cells.Style.WrapText = true;
                }
                package.SaveAs(new FileInfo(filePath));
            }
            Console.WriteLine($"📊 Đã xuất thời khoá biểu phòng học ra file: {filePath}");
        }
    }
}
