using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Schedule.Model;

namespace Schedule.Model
{
    public static class InputLoader
    {
        public static List<Teacher> LoadTeachers(string filePath)
        {
            var json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);
            var items = doc.RootElement.GetProperty("result").GetProperty("items");
            var teachers = new List<Teacher>();
            foreach (var item in items.EnumerateArray())
            {
                teachers.Add(new Teacher
                {
                    Id = item.GetProperty("id").GetString(),
                    FacultyId = item.GetProperty("facultyId").GetString(),
                    UserInternalId = item.GetProperty("userInternalId").GetString(),
                    IsActived = item.GetProperty("isActived").GetBoolean(),
                    Name = item.GetProperty("fullName").GetString(),
                    Faculty = item.GetProperty("facultyId").GetString()
                });
            }
            return teachers;
        }

        public static List<Subject> LoadSubjects(string filePath)
        {
            var json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);
            var items = doc.RootElement.GetProperty("result").GetProperty("items");
            var subjects = new List<Subject>();
            foreach (var item in items.EnumerateArray())
            {
                subjects.Add(new Subject
                {
                    Id = item.GetProperty("id").GetString(),
                    SubjectCode = item.GetProperty("subjectCode").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    CreditPoint = item.GetProperty("creditPoint").GetInt32(),
                    FacultyName = item.GetProperty("facultyName").GetString(),
                    FacultyId = item.GetProperty("facultyId").GetString(),
                    TotalHours = item.GetProperty("totalHours").GetInt32(),
                    SubjectType = item.TryGetProperty("subjectType", out var st) && st.ValueKind != JsonValueKind.Null ? st.GetInt32() : (int?)null,
                    Note = item.TryGetProperty("note", out var note) && note.ValueKind != JsonValueKind.Null ? note.GetString() : null
                });
            }
            return subjects;
        }

        public static List<Room> LoadRooms(string filePath)
        {
            var json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);
            var items = doc.RootElement.GetProperty("result");
            var rooms = new List<Room>();
            foreach (var item in items.EnumerateArray())
            {
                rooms.Add(new Room
                {
                    Id = item.GetProperty("id").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    NumberOfSeats = item.GetProperty("numberOfSeats").GetInt32()
                });
            }
            return rooms;
        }

        public static List<Faculty> LoadFaculties(string filePath)
        {
            var json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);
            var items = doc.RootElement.GetProperty("result").GetProperty("items");
            var faculties = new List<Faculty>();
            foreach (var item in items.EnumerateArray())
            {
                faculties.Add(new Faculty
                {
                    Id = item.GetProperty("id").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    Code = item.GetProperty("code").GetString()
                });
            }
            return faculties;
        }
    }
}