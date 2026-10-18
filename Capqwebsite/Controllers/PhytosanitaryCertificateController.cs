using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

            try
            {
                string storedPath;

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

                // المسار في قاعدة البيانات يبدأ بـ plant/، بينما مجلد plant موجود داخل F:\
                var relativePath = storedPath
                    .Replace('/', Path.DirectorySeparatorChar)
                    .Replace('\\', Path.DirectorySeparatorChar)
                    .TrimStart(Path.DirectorySeparatorChar);

                if (relativePath.StartsWith(
                        "plant" + Path.DirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase))
                {
                    relativePath = relativePath.Substring(
                        ("plant" + Path.DirectorySeparatorChar).Length);
                }

                var filePath = Path.GetFullPath(
                    Path.Combine(@"F:\plant", relativePath));

                if (!System.IO.File.Exists(filePath))
                    return NotFound($"الملف غير موجود: {filePath}");

                var contentType = Path.GetExtension(filePath).ToLowerInvariant() switch
                {
                    ".pdf" => "application/pdf",
                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    _ => "application/octet-stream"
                };

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, contentType);
            }
            catch
            {
                return StatusCode(500, "حدث خطأ أثناء قراءة ملف الشهادة من القرص F.");
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
    }
}