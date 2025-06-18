
## 🚀 Chức năng chính

* 📌 Tự động phân phối thời khóa biểu cho từng lớp, từng môn
* 👨‍🏫 Gán giảng viên phù hợp với điều kiện thời gian
* 🏫 Gán phòng học đúng loại (`LT`, `TH`)
* ❌ Tránh trùng lặp lớp, phòng, giảng viên
* 📤 Xuất kết quả ra `output.json` (UTF-8, giữ tiếng Việt)
* 🖥 Hiển thị kết quả rõ ràng trên console

---

## 📁 Cấu trúc thư mục

```
├── Program.cs                   // Điểm vào chương trình
├── Services/
│   └── SchedulerService.cs      // Xử lý lập lịch chính
├── Model/
│   ├── TimeTableRequest.cs
│   ├── Subject.cs, ClassInput.cs, Teacher.cs, Room.cs
├── input.json                   // Dữ liệu đầu vào
├── output.json                  // Kết quả đầu ra
└── README.md
```

---

## 📦 Logic xử lý trong `SchedulerService`

### 1. Lặp theo lớp học → từng môn học

* Tìm giáo viên dạy được môn đó
* Tìm phòng có `RoomType` khớp với môn
* Duyệt ngẫu nhiên ngày và ca học
* Với mỗi lựa chọn:

  * Kiểm tra giáo viên có `Conditions` phù hợp không
  * Kiểm tra lớp / phòng / giáo viên đã bị chiếm slot chưa
  * Nếu hợp lệ: Gán lịch và ghi nhận slot đã dùng

### 2. Các điều kiện giáo viên được hỗ trợ

| Condition       | Ý nghĩa                 |
| --------------- | ----------------------- |
| `NoMonday`      | Không dạy vào thứ hai   |
| `OnlyAfternoon` | Chỉ dạy ca 4–6          |
| `NoEarlyPeriod` | Không dạy ca 1          |
| `AvoidFriday`   | Hạn chế dạy vào thứ sáu |

### 3. Xuất kết quả

* Dữ liệu được sắp xếp: theo thứ → ca → lớp
* In rõ ràng từng dòng: lớp – môn – GV – phòng – thứ – ca
* Ghi ra `output.json` với định dạng đẹp, không mã hóa Unicode

---

## 🧾 Định dạng `input.json`

```json
{
  "Subjects": [
    { "Name": "Cấu trúc dữ liệu", "Dependency": [], "RoomType": "LT" }
  ],
  "Classes": [
    { "Name": "IT1_K64", "Subjects": ["Cấu trúc dữ liệu"] }
  ],
  "Teachers": [
    { "Name": "ThS. Nguyễn Văn A", "Subjects": ["Cấu trúc dữ liệu"], "Conditions": ["NoMonday"] }
  ],
  "Rooms": [
    { "Name": "B101", "Type": "LT" }
  ]
}
```

---
