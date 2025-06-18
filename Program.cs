using System;
using System.Text.Json;
using System.Collections.Generic;
using Schedule.Model;
using Schedule.Service;

// Giả sử các lớp: Subject, ClassInput, Teacher, Room, TimeTableRequest, ScheduledClass, SchedulerService đã được định nghĩa sẵn

public class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        string filePath = "input.json";

        if (!File.Exists(filePath))
        {
            Console.WriteLine("File input.json không tồn tại.");
            return;
        }

        try
        {
            string jsonContent = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var input = JsonSerializer.Deserialize<TimeTableRequest>(jsonContent, options);

            var scheduler = new SchedulerService();
            var results = scheduler.Generate(input);

            Console.WriteLine("KẾT QUẢ SẮP XẾP THỜI KHÓA BIỂU:");
            Console.WriteLine("--------------------------------");

            foreach (var item in results)
            {
                Console.WriteLine($"Lớp: {item.ClassName} | Môn: {item.Subject} | GV: {item.Teacher} | Phòng: {item.Room} | {item.Day} - Ca {item.Period}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi đọc file: " + ex.Message);
        }
    }
}
