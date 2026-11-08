using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace Capqwebsite.Controllers
{
    public class PhytosanitaryCertificateController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public PhytosanitaryCertificateController(
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCertificate(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "أدخلي رقم الشهادة."
                });
            }

            var connectionString =
                _configuration.GetConnectionString("CertificateDb");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "إعداد الاتصال CertificateDb غير موجود."
                });
            }

            try
            {
                await using var connection = new SqlConnection(connectionString);
                await using var command = new SqlCommand(@"
                    SELECT TOP (1) CertificateNumber, CertificateFile
                    FROM dbo.CertificateFile
                    WHERE CertificateNumber = @number;", connection);

                command.Parameters
                    .Add("@number", SqlDbType.NVarChar, 100)
                    .Value = number.Trim();

                await connection.OpenAsync();

                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "لم يتم العثور على الشهادة."
                    });
                }

                var certificateNumber =
                    reader["CertificateNumber"]?.ToString();

                var certificateFile =
                    reader["CertificateFile"]?.ToString();

                if (string.IsNullOrWhiteSpace(certificateFile))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "لا يوجد ملف مرفق بهذه الشهادة."
                    });
                }

                return Json(new
                {
                    success = true,
                    certificateNumber,
                    certificateFile = Url.Action(
                        nameof(OpenCertificate),
                        "PhytosanitaryCertificate",
                        new { number = certificateNumber })
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء البحث عن الشهادة."
                });
            }
        }

       
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> OpenCertificate(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return BadRequest("رقم الشهادة مطلوب.");

            var connectionString =
                _configuration.GetConnectionString("CertificateDb");

            if (string.IsNullOrWhiteSpace(connectionString))
                return StatusCode(500, "إعداد الاتصال CertificateDb غير موجود.");

            string? server = null;
            string? filePath = null;
            string? storedPath = null;

            try
            {
                await using (var connection = new SqlConnection(connectionString))
                await using (var command = new SqlCommand(@"
            SELECT TOP (1) CertificateFile
            FROM dbo.CertificateFile
            WHERE CertificateNumber = @number;", connection))
                {
                    command.Parameters
                        .Add("@number", SqlDbType.NVarChar, 100)
                        .Value = number.Trim();

                    await connection.OpenAsync();

                    var result = await command.ExecuteScalarAsync();

                    if (result == null || result == DBNull.Value)
                        return NotFound("لم يتم العثور على الشهادة.");

                    storedPath = result.ToString()?.Trim();

                    if (string.IsNullOrWhiteSpace(storedPath))
                        return NotFound("مسار ملف الشهادة فارغ.");
                }

                server = ResolveAttachmentServer();
                filePath = ResolveAttachmentPath(server, storedPath);

                var stream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 64 * 1024,
                    options: FileOptions.Asynchronous | FileOptions.SequentialScan);

                try
                {
                    Response.Headers["X-Content-Type-Options"] = "nosniff";
                    Response.Headers.CacheControl = "private, no-store";

                    var contentType = Path.GetExtension(filePath).ToLowerInvariant() switch
                    {
                        ".pdf" => "application/pdf",
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".webp" => "image/webp",
                        ".bmp" => "image/bmp",
                        _ => "application/octet-stream"
                    };

                    return File(
                        stream,
                        contentType,
                        enableRangeProcessing: true);
                }
                catch
                {
                    await stream.DisposeAsync();
                    throw;
                }
            }
            catch (ArgumentException ex)
            {
                return AttachmentError(
                    400, "مسار ملف الشهادة غير صالح.", ex, server, storedPath, filePath);
            }
            catch (FileNotFoundException ex)
            {
                return AttachmentError(
                    404, "ملف الشهادة غير موجود على السيرفر.", ex, server, storedPath, filePath);
            }
            catch (DirectoryNotFoundException ex)
            {
                return AttachmentError(
                    404, "مجلد الشهادة غير موجود أو مشاركة الملفات غير متاحة.",
                    ex, server, storedPath, filePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                return AttachmentError(
                    403, "حساب تشغيل الموقع ليس لديه صلاحية قراءة ملف الشهادة من السيرفر.",
                    ex, server, storedPath, filePath);
            }
            catch (IOException ex)
            {
                return AttachmentError(
                    503, "تعذرت قراءة ملف الشهادة. تحقق من اتصال السيرفر وصلاحية المشاركة.",
                    ex, server, storedPath, filePath);
            }
            catch (ConfigurationErrorsException ex)
            {
                return AttachmentError(
                    500, "إعدادات سيرفر الملفات غير صالحة.",
                    ex, server, storedPath, filePath);
            }
        }
        private static string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }

        private string ResolveAttachmentServer()
        {
            var server = _configuration["Path_NetworkShare"];

            if (string.IsNullOrWhiteSpace(server))
                throw new InvalidOperationException(
                    "الإعداد Path_NetworkShare غير موجود.");

            server = server.Trim().Trim('\\', '/');

            if (server.IndexOfAny(new[] { '\\', '/', ':' }) >= 0)
                throw new InvalidOperationException(
                    "Path_NetworkShare يجب أن يحتوي اسم السيرفر أو عنوان IP فقط.");

            return server;
        }

        private string ResolveAttachmentPath(string server, string storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
                throw new ArgumentException("مسار الشهادة فارغ.");

            const string root = @"\"; // لا تستخدمي هذا السطر؛ السطر الصحيح أدناه
            var shareRoot = @"\\" + server + @"\plant\";

            var path = storedPath.Trim().Replace('/', '\\');

            foreach (var segment in path.Split('\\'))
            {
                if (segment == "." || segment == "..")
                    throw new ArgumentException("مسار الشهادة غير صالح.");
            }

            // يقبل المسار النسبي مثل plant\folder\file.pdf
            // أو مسار UNC كامل على نفس مشاركة plant.
            if (!path.StartsWith(@"\\", StringComparison.Ordinal))
                path = @"\\" + server + @"\" + path.TrimStart('\\');

            path = Path.GetFullPath(path);

            if (!path.StartsWith(shareRoot, StringComparison.OrdinalIgnoreCase)
                || path.Length == shareRoot.Length
                || path.Contains(':'))
            {
                throw new ArgumentException(
                    "ملف الشهادة يجب أن يكون داخل مشاركة plant.");
            }

            return path;
        }   
        private IActionResult AttachmentError(
            int statusCode,
            string message,
            Exception exception,
            string? server,
            string? storedPath,
            string? filePath)
        {
            // سجّلي exception هنا باستخدام ILogger إذا كان متوفرًا في الـController.
            return StatusCode(statusCode, message);
        }


    }
}