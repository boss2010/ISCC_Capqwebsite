using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Capqwebsite.Controllers
{
    public class PhytosanitaryCertificateController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PhytosanitaryCertificateController> _logger;
        private readonly string _attachmentServer;

        public PhytosanitaryCertificateController(
            IConfiguration configuration,
            ILogger<PhytosanitaryCertificateController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _attachmentServer = configuration["Path_NetworkShare"]
                ?? throw new InvalidOperationException("Path_NetworkShare is not configured.");
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

                if (Uri.TryCreate(storedPath, UriKind.Absolute, out var uri)
                    && uri.Scheme is "http" or "https")
                    return Redirect(storedPath);

                string filePath = Capqwebsite.Infrastructure.CertificateAttachmentFiles.ResolvePath(
                    storedPath, _attachmentServer);
                Capqwebsite.Infrastructure.AttachmentShareConnection.Connect(filePath, _configuration);
                if (!System.IO.File.Exists(filePath))
                    return NotFound("ملف الشهادة غير موجود على سيرفر الملفات.");

                string contentType = Capqwebsite.Infrastructure.CertificateAttachmentFiles.ContentType(filePath);
                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath, HttpContext.RequestAborted);
                return contentType == "application/octet-stream"
                    ? File(fileBytes, contentType, Path.GetFileName(filePath))
                    : File(fileBytes, contentType);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogError(ex,
                    "Unable to open public certificate {CertificateNumber} from the configured file server",
                    number);
                return NotFound("تعذر فتح ملف الشهادة من السيرفر. تأكد من وجود الملف وصلاحية الوصول للمجلد المشترك.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load public certificate {CertificateNumber}", number);
                return StatusCode(500, "حدث خطأ أثناء قراءة ملف الشهادة.");
            }
        }
    }
}
