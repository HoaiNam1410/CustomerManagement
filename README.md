\# Customer Management



Ứng dụng quản lý khách hàng cơ bản, xây dựng bằng ASP.NET Core Web API và Blazor WebAssembly.



\## 1. Chức năng



\- Đăng nhập bằng tài khoản quản trị.

\- Xem danh sách khách hàng và phân trang trên giao diện.

\- Thêm, sửa và xóa khách hàng có xác nhận.

\- Tìm kiếm theo họ tên hoặc số điện thoại.

\- Lọc theo trạng thái hoạt động.

\- Kiểm tra dữ liệu bắt buộc, định dạng email, số điện thoại và ngày sinh.

\- Ngăn trùng mã khách hàng bằng kiểm tra nghiệp vụ và unique index.

\- Bảo vệ API khách hàng bằng JWT và role `Admin`.

\- Hiển thị thông báo thành công, lỗi và trạng thái danh sách trống.



\## 2. Công nghệ



| Thành phần | Công nghệ |

|---|---|

| Backend | ASP.NET Core Web API, .NET 8 |

| Frontend | Blazor WebAssembly, MudBlazor |

| Database | SQL Server / SQL Server LocalDB |

| Truy cập dữ liệu | Entity Framework Core 8, Code First |

| Xác thực | JWT Bearer |

| Tài liệu API | Swagger UI |



\## 3. Cấu trúc solution



| Project | Trách nhiệm |

|---|---|

| `CustomerManagement.Api` | REST API, xác thực JWT, xử lý nghiệp vụ, EF Core và migrations |

| `CustomerManagement.Client` | Giao diện Blazor, gọi API, quản lý phiên đăng nhập |

| `CustomerManagement.Contracts` | DTO dùng chung và quy tắc validation |



Trong API:



\- `Controllers`: tiếp nhận request, trả HTTP response.

\- `Services`: xử lý nghiệp vụ và tạo token.

\- `Entities`: mô hình dữ liệu.

\- `Data`: DbContext và cấu hình database.

\- `Migrations`: lịch sử thay đổi cấu trúc database.



\## 4. Yêu cầu môi trường



\- Windows nếu sử dụng SQL Server LocalDB theo hướng dẫn này.

\- .NET SDK 8.

\- Visual Studio 2022 hỗ trợ .NET 8, workload ASP.NET and web development.

\- SQL Server Express LocalDB hoặc SQL Server.

\- Git.



Kiểm tra môi trường trong PowerShell:



```powershell

dotnet --list-sdks

sqllocaldb info

git --version

```



\## 5. Lấy source



```powershell

git clone https://github.com/HoaiNam1410/CustomerManagement.git

cd CustomerManagement

dotnet restore

```



Nếu repository đang để private, tài khoản GitHub phải được cấp quyền truy cập.



\## 6. Cấu hình database



Môi trường phát triển sử dụng LocalDB instance `CEPProject`.



Nếu máy chưa có instance này:



```powershell

sqllocaldb create CEPProject

sqllocaldb start CEPProject

```



Nếu instance đã tồn tại, chỉ cần chạy lệnh `start`.



Trong `CustomerManagement.Api/appsettings.json`, cấu hình mục sau.

Giữ nguyên các mục cấu hình khác trong file:



```json

{

&#x20; "ConnectionStrings": {

&#x20;   "DefaultConnection": "Server=(localdb)\\\\CEPProject;Database=CustomerManagementDb;Trusted\_Connection=True;TrustServerCertificate=True"

&#x20; }

}

```



Có thể dùng SQL Server hoặc LocalDB instance khác bằng cách đổi chuỗi kết nối.



Nếu chuỗi kết nối chứa mật khẩu, lưu bằng User Secrets thay vì commit vào source.



\## 7. Cấu hình tài khoản admin và JWT



Chạy các lệnh sau trong PowerShell tại thư mục solution:



```powershell

dotnet user-secrets set "Jwt:Issuer" "CustomerManagement.Api" --project CustomerManagement.Api

dotnet user-secrets set "Jwt:Audience" "CustomerManagement.Client" --project CustomerManagement.Api

dotnet user-secrets set "Jwt:ExpiryMinutes" "60" --project CustomerManagement.Api



dotnet user-secrets set "AdminAccount:Username" "admin" --project CustomerManagement.Api

dotnet user-secrets set "AdminAccount:Password" "123456" --project CustomerManagement.Api

```



Tạo khóa ký riêng trên máy chạy ứng dụng:



```powershell

$jwtBytes = New-Object byte\[] 32

$jwtRng = \[System.Security.Cryptography.RandomNumberGenerator]::Create()

$jwtRng.GetBytes($jwtBytes)

$jwtKey = \[Convert]::ToBase64String($jwtBytes)

$jwtRng.Dispose()



dotnet user-secrets set "Jwt:SecretKey" "$jwtKey" --project CustomerManagement.Api

```



Project API đã khai báo `UserSecretsId`.

User Secrets được nạp trong môi trường Development.



Tài khoản sau khi cấu hình theo hướng dẫn:



\- Tên đăng nhập: `admin`

\- Mật khẩu: `123456`



Đây là tài khoản demo. Mật khẩu thực tế có thể thay đổi qua User Secrets.

Khóa ký JWT không được lưu trong repository.



\## 8. Tạo database bằng migration



Mở `CustomerManagement.sln` trong Visual Studio.



Vào:



\*\*Tools → NuGet Package Manager → Package Manager Console\*\*



Chạy:



```powershell

$env:ASPNETCORE\_ENVIRONMENT = "Development"



Update-Database -Project CustomerManagement.Api -StartupProject CustomerManagement.Api

```



Lệnh áp dụng migration `InitialCreate` có sẵn và tạo database cùng bảng khách hàng.



Không cần tạo lại migration ban đầu. Dữ liệu khách hàng không được seed sẵn; có thể thêm từ giao diện sau khi đăng nhập.



\## 9. Chạy ứng dụng



Tin cậy chứng chỉ HTTPS dành cho development:



```powershell

dotnet dev-certs https --trust

```



Build solution:



```powershell

dotnet build

```



Mở hai cửa sổ PowerShell tại thư mục solution.



Cửa sổ thứ nhất chạy API:



```powershell

dotnet run --project CustomerManagement.Api --launch-profile https

```



Cửa sổ thứ hai chạy Client:



```powershell

dotnet run --project CustomerManagement.Client --launch-profile https

```



| Địa chỉ | Chức năng |

|---|---|

| `https://localhost:7035/` | Trang chính |

| `https://localhost:7035/login` | Đăng nhập |

| `https://localhost:7035/customers` | Quản lý khách hàng |

| `https://localhost:7231/swagger` | Thử API bằng Swagger |



Giữ cả hai cửa sổ PowerShell mở trong khi sử dụng.

Nhấn `Ctrl + C` để dừng ứng dụng.



Client đọc địa chỉ API từ:



`CustomerManagement.Client/wwwroot/appsettings.json`



```json

{

&#x20; "ApiBaseUrl": "https://localhost:7231/"

}

```



Nếu đổi cổng chạy, cập nhật `launchSettings.json`, `ApiBaseUrl` và danh sách origin của policy CORS `BlazorClient` trong API cho tương ứng.



\## 10. Thử API bằng Swagger



1\. Mở Swagger.

2\. Gọi `POST /api/auth/login`:



```json

{

&#x20; "username": "admin",

&#x20; "password": "123456"

}

```



3\. Copy giá trị `accessToken` từ response.

4\. Bấm \*\*Authorize\*\*, dán token, không thêm chữ `Bearer` và không lấy dấu ngoặc kép.

5\. Gọi các API khách hàng.



| Phương thức | Đường dẫn | Chức năng |

|---|---|---|

| POST | `/api/auth/login` | Đăng nhập |

| GET | `/api/customers` | Danh sách, tìm kiếm và lọc |

| GET | `/api/customers/{id}` | Xem một khách hàng |

| POST | `/api/customers` | Thêm khách hàng |

| PUT | `/api/customers/{id}` | Cập nhật khách hàng |

| DELETE | `/api/customers/{id}` | Xóa khách hàng |



Ví dụ tìm kiếm và lọc:



```text

/api/customers?search=An\&isActive=true

```



Ví dụ dữ liệu thêm mới:



```json

{

&#x20; "customerCode": "KH001",

&#x20; "fullName": "Nguyễn Văn An",

&#x20; "email": "an@example.com",

&#x20; "phoneNumber": "0901234567",

&#x20; "dateOfBirth": "1995-05-20",

&#x20; "isActive": true

}

```



Các mã phản hồi chính:



\- `200`: truy vấn, cập nhật hoặc đăng nhập thành công.

\- `201`: tạo khách hàng thành công.

\- `204`: xóa thành công.

\- `400`: dữ liệu không hợp lệ.

\- `401`: đăng nhập sai hoặc thiếu/token không hợp lệ.

\- `403`: tài khoản không có quyền yêu cầu.

\- `404`: không tìm thấy khách hàng.

\- `409`: mã khách hàng đã tồn tại.



\## 11. Các luồng đã kiểm tra thủ công



\- Thêm, xem, sửa, xóa khách hàng.

\- Hủy thao tác xóa.

\- Tìm theo họ tên và số điện thoại; lọc trạng thái.

\- Thiếu trường bắt buộc, email sai định dạng, mã khách hàng trùng.

\- Đăng nhập đúng và sai mật khẩu.

\- API khách hàng trả 401 khi không có token và 200 khi có token hợp lệ.

\- Tải lại trang khi đã đăng nhập.

\- Đăng xuất và truy cập lại trang được bảo vệ.

\- Trang chủ `/`, danh sách trống và tìm kiếm không có kết quả.



\## 12. Phạm vi triển khai



\- Một tài khoản admin cố định, cấu hình phía API.

\- JWT hết hạn sau 60 phút theo cấu hình; chưa triển khai refresh token.

\- Client lưu phiên trong `sessionStorage`; API kiểm tra chữ ký, hạn token và quyền.

\- Đăng xuất xóa phiên phía Client; chưa có cơ chế thu hồi token phía server.

\- Phân trang phía Client; API trả toàn bộ kết quả phù hợp bộ lọc.

\- Xóa khách hàng là xóa trực tiếp trong database.



\## 13. Xử lý lỗi thường gặp



\### Build báo DLL đang được sử dụng



Dừng API và Client bằng `Ctrl + C`, sau đó build lại.



\### Không kết nối được API



Kiểm tra API đang chạy, địa chỉ `ApiBaseUrl`, chứng chỉ HTTPS và cấu hình CORS.



\### Thiếu cấu hình JWT hoặc tài khoản admin



Thực hiện bước User Secrets cho đúng project API và chạy profile `https` trong môi trường Development.



\### Không kết nối được database



Kiểm tra SQL Server/LocalDB đã cài và instance đang chạy:



```powershell

sqllocaldb info CEPProject

sqllocaldb start CEPProject

```



Đối chiếu chuỗi kết nối và chạy migration.



\### API trả 401 sau một thời gian sử dụng



Đăng nhập lại để lấy token mới.

