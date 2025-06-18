using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Collections.Generic;
using Schedule.Model;
using Schedule.Services;

public class Program
{
    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        string inputPath = "input.json";
        string outputPath = "output.json";

        if (!File.Exists(inputPath))
        {
            Console.WriteLine("❌ File input.json không tồn tại.");
            return;
        }

        try
        {
            string jsonContent = File.ReadAllText(inputPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var input = JsonSerializer.Deserialize<TimeTableRequest>(jsonContent, options);
            if (input == null)
            {
                Console.WriteLine("❌ Dữ liệu input không hợp lệ.");
                return;
            }

            var scheduler = new SchedulerService();
            var results = scheduler.Generate(input);

            var jsonOutput = JsonSerializer.Serialize(results, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            File.WriteAllText(outputPath, jsonOutput, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Console.WriteLine($"\n📁 Đã lưu kết quả vào: {outputPath}");

            Console.WriteLine("\n📅 KẾT QUẢ SẮP XẾP THỜI KHÓA BIỂU:\n");
            foreach (var item in results)
            {
                Console.WriteLine($"Lớp: {item.ClassName,-8} | Môn: {item.Subject,-25} | GV: {item.Teacher,-20} | Phòng: {item.Room,-6} | {item.Day,-10} - Ca {item.Period}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❗ Lỗi khi xử lý: " + ex.Message);
        }
    }
}
