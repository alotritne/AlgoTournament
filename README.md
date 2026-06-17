# AlgoTournament

Một nền tảng thi lập trình (ASP.NET Core) dùng để tổ chức giải đấu, đối kháng (duel), nộp bài và đánh giá.

## Tính năng chính
- Quản lý người dùng, đội, mùa giải và bảng xếp hạng
- Tổ chức contest và duel trực tuyến
- Hệ thống nộp bài và chấm tự động
- Bảng tin, thảo luận và thông báo

## Yêu cầu
- .NET 8 SDK
- SQL Server (hoặc cấu hình database trong [appsettings.json](algotournament/appsettings.json#L1))

## Cài đặt nhanh (Windows)
1. Mở terminal trong thư mục gốc dự án (chứa `algotournament.slnx`).
2. Khôi phục và build:

```powershell
dotnet restore
dotnet build
```

3. (Tùy chọn) Cập nhật database nếu cần:

```powershell
dotnet ef database update --project algotournament
```

4. Chạy ứng dụng:

```powershell
dotnet run --project algotournament
```

Ứng dụng sẽ lắng nghe theo cấu hình trong [algotournament/Properties/launchSettings.json](algotournament/Properties/launchSettings.json#L1) và cấu hình môi trường trong [algotournament/appsettings.json](algotournament/appsettings.json#L1).

## Seed dữ liệu
Dự án có các lớp seed trong `Data/SeedData.cs` và `Data/SeedHelloWorld.cs` để chèn dữ liệu mẫu. Chạy chúng nếu cần (thường được gọi trong quá trình khởi tạo DB).

## Phát triển
- Mã nguồn chính tại thư mục `algotournament`.
- Các service liên quan tới duel và judge nằm ở `Services/`.
- Hub SignalR ở `Hubs/DuelHub.cs`.

## Đóng góp
1. Fork repo
2. Tạo branch: `feature/your-feature`
3. Tạo pull request với mô tả rõ ràng

## License
Không có license cụ thể trong repo; thêm file `LICENSE` nếu muốn công khai.

## Liên hệ
Nếu cần trợ giúp, mở issue trên repository hoặc liên hệ trực tiếp với maintainer của dự án.
