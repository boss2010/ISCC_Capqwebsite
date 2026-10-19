using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Capqwebsite.Controllers
{
    public class PhytosanitaryCertificateCountryController : Controller
    {
        private readonly IConfiguration _configuration;

        public PhytosanitaryCertificateCountryController(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetCertificatesByCountry(
            string userName,
            string password,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page = 1,
            int pageSize = 12)
        {
            if (string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "أدخلي اسم المستخدم وكلمة مرور الدولة."
                });
            }

            if (dateFrom.HasValue &&
                dateTo.HasValue &&
                dateFrom.Value.Date > dateTo.Value.Date)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "تاريخ البداية يجب أن يكون قبل تاريخ النهاية."
                });
            }

            page = Math.Max(page, 1);
            pageSize = Math.Max(1, Math.Min(pageSize, 50));

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
                await using var connection =
                    new SqlConnection(connectionString);

                await connection.OpenAsync();

                int countryId;

                await using (var credentialCommand = new SqlCommand(@"
                    SELECT TOP (1) CountryId
                    FROM dbo.CountryPassword
                    WHERE UserName = @userName
                      AND Password = @password;",
                    connection))
                {
                    credentialCommand.Parameters
                        .Add("@userName", SqlDbType.NVarChar, 256)
                        .Value = userName.Trim();

                    credentialCommand.Parameters
                        .Add("@password", SqlDbType.NVarChar, 256)
                        .Value = password;

                    var credentialResult =
                        await credentialCommand.ExecuteScalarAsync();

                    if (credentialResult == null ||
                        credentialResult == DBNull.Value)
                    {
                        return Unauthorized(new
                        {
                            success = false,
                            message = "اسم المستخدم أو كلمة المرور غير صحيحة."
                        });
                    }

                    countryId = Convert.ToInt32(credentialResult);
                }

                int totalCount;

                await using (var countCommand = new SqlCommand(@"
                    SELECT COUNT(*)
                    FROM dbo.CertificateFile
                    WHERE CountryId = @countryId
                      AND (@dateFrom IS NULL
                           OR User_Creation_Date >= @dateFrom)
                      AND (@dateTo IS NULL
                           OR User_Creation_Date < DATEADD(DAY, 1, @dateTo));",
                    connection))
                {
                    countCommand.Parameters
                        .Add("@countryId", SqlDbType.Int)
                        .Value = countryId;

                    countCommand.Parameters
                        .Add("@dateFrom", SqlDbType.Date)
                        .Value = dateFrom.HasValue
                            ? dateFrom.Value.Date
                            : DBNull.Value;

                    countCommand.Parameters
                        .Add("@dateTo", SqlDbType.Date)
                        .Value = dateTo.HasValue
                            ? dateTo.Value.Date
                            : DBNull.Value;

                    totalCount = Convert.ToInt32(
                        await countCommand.ExecuteScalarAsync());
                }

                var totalPages = (int)Math.Ceiling(
                    (double)totalCount / pageSize);

                if (totalPages > 0 && page > totalPages)
                    page = totalPages;

                var offset = (page - 1) * pageSize;
                var certificates = new List<object>();

                await using (var certificatesCommand = new SqlCommand(@"
                    SELECT Id, CertificateNumber, User_Creation_Date
                    FROM dbo.CertificateFile
                    WHERE CountryId = @countryId
                      AND (@dateFrom IS NULL
                           OR User_Creation_Date >= @dateFrom)
                      AND (@dateTo IS NULL
                           OR User_Creation_Date < DATEADD(DAY, 1, @dateTo))
                    ORDER BY Id DESC
                    OFFSET @offset ROWS
                    FETCH NEXT @pageSize ROWS ONLY;",
                    connection))
                {
                    certificatesCommand.Parameters
                        .Add("@countryId", SqlDbType.Int)
                        .Value = countryId;

                    certificatesCommand.Parameters
                        .Add("@dateFrom", SqlDbType.Date)
                        .Value = dateFrom.HasValue
                            ? dateFrom.Value.Date
                            : DBNull.Value;

                    certificatesCommand.Parameters
                        .Add("@dateTo", SqlDbType.Date)
                        .Value = dateTo.HasValue
                            ? dateTo.Value.Date
                            : DBNull.Value;

                    certificatesCommand.Parameters
                        .Add("@offset", SqlDbType.Int)
                        .Value = offset;

                    certificatesCommand.Parameters
                        .Add("@pageSize", SqlDbType.Int)
                        .Value = pageSize;

                    await using var reader =
                        await certificatesCommand.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        var certificateNumber =
                            reader["CertificateNumber"]?.ToString();

                        if (string.IsNullOrWhiteSpace(certificateNumber))
                            continue;

                        string certificateDate = null;

                        if (reader["User_Creation_Date"] != DBNull.Value)
                        {
                            var creationDate =
                                Convert.ToDateTime(reader["User_Creation_Date"]);

                            certificateDate =
                                creationDate.ToString(
                                    "yyyy-MM-dd",
                                    CultureInfo.InvariantCulture);
                        }

                        certificates.Add(new
                        {
                            certificateNumber,
                            certificateDate,
                            certificateFile = Url.Action(
                                nameof(OpenCertificate),
                                "PhytosanitaryCertificateCountry",
                                new { number = certificateNumber })
                        });
                    }
                }

                return Json(new
                {
                    success = true,
                    certificates,
                    page,
                    pageSize,
                    totalCount,
                    totalPages
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء البحث عن شهادات الدولة."
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

                await using (var connection =
                    new SqlConnection(connectionString))
                await using (var command = new SqlCommand(@"
                    SELECT TOP (1) CertificateFile
                    FROM dbo.CertificateFile
                    WHERE CertificateNumber = @number;",
                    connection))
                {
                    command.Parameters
                        .Add("@number", SqlDbType.NVarChar, 100)
                        .Value = number.Trim();

                    await connection.OpenAsync();

                    var result = await command.ExecuteScalarAsync();

                    if (result == null || result == DBNull.Value)
                        return NotFound("لم يتم العثور على ملف الشهادة.");

                    storedPath = result.ToString()?.Trim();

                    if (string.IsNullOrWhiteSpace(storedPath))
                        return NotFound("مسار ملف الشهادة فارغ.");
                }

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
                    return NotFound("ملف الشهادة غير موجود على هذا الجهاز.");

                var extension =
                    Path.GetExtension(filePath).ToLowerInvariant();

                var contentType = extension switch
                {
                    ".pdf" => "application/pdf",
                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    _ => "application/octet-stream"
                };

                var fileBytes =
                    await System.IO.File.ReadAllBytesAsync(filePath);

                return File(fileBytes, contentType);
            }
            catch
            {
                return StatusCode(
                    500,
                    "حدث خطأ أثناء قراءة ملف الشهادة من القرص F.");
            }
        }
    }
}