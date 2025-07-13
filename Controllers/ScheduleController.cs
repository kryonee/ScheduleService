using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Collections.Generic;
using Schedule.Model;
using Schedule.Services;

namespace Schedule.Controllers
{
    public class ScheduleController
    {
        private readonly SchedulerService _schedulerService;

        public ScheduleController()
        {
            _schedulerService = new SchedulerService();
        }

        public void GenerateSchedule(string inputPath = "input.json", string outputPath = "output.json")
        {
            Console.OutputEncoding = Encoding.UTF8;

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

                var results = _schedulerService.Generate(input);

                var jsonOutput = JsonSerializer.Serialize(results, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                File.WriteAllText(outputPath, jsonOutput, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                Console.WriteLine($"\n📁Đã lưu kết quả vào: {outputPath}");
        
            }
            catch (Exception ex)
            {
                Console.WriteLine("❗ Lỗi khi xử lý: " + ex.Message);
            }
        }

        public List<ScheduledClass> GenerateScheduleFromJson(string jsonContent, string outputPath = "output.json")
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var input = JsonSerializer.Deserialize<TimeTableRequest>(jsonContent, options);
                if (input == null)
                {
                    Console.WriteLine("❌ Dữ liệu JSON không hợp lệ.");
                    return new List<ScheduledClass>();
                }

                var results = _schedulerService.Generate(input);

                var jsonOutput = JsonSerializer.Serialize(results, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                File.WriteAllText(outputPath, jsonOutput, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                Console.WriteLine($"\n📁 Đã lưu kết quả vào: {outputPath}");

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❗ Lỗi khi xử lý: " + ex.Message);
                return new List<ScheduledClass>();
            }
        }
    }
}