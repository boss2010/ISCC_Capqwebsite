using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

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

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchResults(
            string userName,
            string password)
        {
            if (string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(password))
            {
                TempData["SearchError"] =
                    "من فضلك أدخلي اسم المستخدم وكلمة المرور.";

                return RedirectToAction(nameof(Index));
            }

            var connectionString =
                _configuration.GetConnectionString("CertificateDb");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return StatusCode(
                    500,
                    "Connection string باسم CertificateDb غير موجود في الإعدادات.");
            }

            const string sql = @"
                SELECT TOP (1) CountryId
                FROM dbo.CountryPassword
                WHERE UserName = @UserName
                  AND Password = @Password;";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@UserName", SqlDbType.NVarChar, 256)
                .Value = userName.Trim();

            command.Parameters.Add("@Password", SqlDbType.NVarChar, 256)
                .Value = password;

            var result = await command.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value)
            {
                TempData["SearchError"] =
                    "اسم المستخدم أو كلمة المرور غير صحيحة.";

                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetInt32(
                "CertificateCountryId",
                Convert.ToInt32(result));

            return RedirectToAction(nameof(Results));
        }

        // عند فتح صفحة النتائج لأول مرة، اعرض شهادات السنة الحالية.
        [HttpGet]
        public async Task<IActionResult> Results()
        {
            var dateFrom = new DateTime(DateTime.Today.Year, 1, 1);
            var dateTo = DateTime.Today;

            return await LoadResultsAsync(
                dateFrom,
                dateTo,
                certificateNumber: null,
                page: 1,
                pageSize: 12);
        }

        // يستقبل فلتر التاريخ، رقم الشهادة، والتنقل بين الصفحات.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FilterResults(
            DateTime? dateFrom,
            DateTime? dateTo,
            string? certificateNumber,
            int page = 1,
            int pageSize = 12)
        {
            return await LoadResultsAsync(
                dateFrom,
                dateTo,
                certificateNumber,
                page,
                pageSize);
        }

        private async Task<IActionResult> LoadResultsAsync(
            DateTime? dateFrom,
            DateTime? dateTo,
            string? certificateNumber,
            int page,
            int pageSize)
        {
            var countryId =
                HttpContext.Session.GetInt32("CertificateCountryId");

            if (countryId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            dateFrom = dateFrom?.Date;
            dateTo = dateTo?.Date;

            // إزالة عبارة "شهادة رقم:" إذا كانت ضمن النص المرسل.
            certificateNumber = certificateNumber?.Trim();

            const string certificateLabel = "شهادة رقم:";

            if (!string.IsNullOrWhiteSpace(certificateNumber) &&
                certificateNumber.StartsWith(
                    certificateLabel,
                    StringComparison.Ordinal))
            {
                certificateNumber =
                    certificateNumber[certificateLabel.Length..].Trim();
            }

            if (string.IsNullOrWhiteSpace(certificateNumber))
            {
                certificateNumber = null;
            }

            var model = new CertificateResultsViewModel
            {
                Certificates = new List<CertificateCardViewModel>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 1,
                DateFrom = dateFrom,
                DateTo = dateTo,
                CertificateNumber = certificateNumber
            };

            if (dateFrom.HasValue &&
                dateTo.HasValue &&
                dateFrom.Value > dateTo.Value &&
                certificateNumber == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "تاريخ البداية يجب أن يكون قبل تاريخ النهاية.");

                return View("Results", model);
            }

            var connectionString =
                _configuration.GetConnectionString("CertificateDb");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return StatusCode(
                    500,
                    "Connection string باسم CertificateDb غير موجود في الإعدادات.");
            }

            const string whereClause = @"
                WHERE CountryId = @CountryId
                  AND (
                        @CertificateNumber IS NOT NULL
                        OR (
                            (@DateFrom IS NULL OR User_Creation_Date >= @DateFrom)
                            AND
                            (@DateTo IS NULL OR User_Creation_Date < DATEADD(DAY, 1, @DateTo))
                        )
                      )
                  AND (
                        @CertificateNumber IS NULL
                        OR CONVERT(NVARCHAR(100), CertificateNumber)
                            LIKE N'%' + @CertificateNumber + N'%'
                      )";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // عند البحث برقم الشهادة، اجلب أقدم تاريخ من كل الشهادات المطابقة.
            if (!string.IsNullOrWhiteSpace(certificateNumber))
            {
                const string oldestDateSql = @"
                    SELECT MIN(User_Creation_Date)
                    FROM dbo.CertificateFile
                    WHERE CountryId = @CountryId
                      AND CONVERT(NVARCHAR(100), CertificateNumber)
                          LIKE N'%' + @CertificateNumber + N'%';";

                await using var oldestDateCommand =
                    new SqlCommand(oldestDateSql, connection);

                oldestDateCommand.Parameters.Add(
                    "@CountryId",
                    SqlDbType.Int).Value = countryId.Value;

                oldestDateCommand.Parameters.Add(
                    "@CertificateNumber",
                    SqlDbType.NVarChar,
                    100).Value = certificateNumber;

                var oldestDateResult =
                    await oldestDateCommand.ExecuteScalarAsync();

                if (oldestDateResult != null &&
                    oldestDateResult != DBNull.Value)
                {
                    dateFrom = Convert.ToDateTime(oldestDateResult).Date;
                    model.DateFrom = dateFrom;
                }
            }

            var countSql = $@"
                SELECT COUNT(*)
                FROM dbo.CertificateFile
                {whereClause};";

            await using (var countCommand = new SqlCommand(countSql, connection))
            {
                AddResultsParameters(
                    countCommand,
                    countryId.Value,
                    dateFrom,
                    dateTo,
                    certificateNumber);

                model.TotalCount = Convert.ToInt32(
                    await countCommand.ExecuteScalarAsync());
            }

            model.TotalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    model.TotalCount / (double)pageSize));

            model.Page = Math.Min(page, model.TotalPages);

            var offset = (model.Page - 1) * pageSize;

            var resultsSql = $@"
                SELECT Id, CertificateNumber, CertificateFile, User_Creation_Date
                FROM dbo.CertificateFile
                {whereClause}
                ORDER BY User_Creation_Date DESC, Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            await using (var command = new SqlCommand(resultsSql, connection))
            {
                AddResultsParameters(
                    command,
                    countryId.Value,
                    dateFrom,
                    dateTo,
                    certificateNumber);

                command.Parameters.Add("@Offset", SqlDbType.Int).Value = offset;
                command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

                await using var reader = await command.ExecuteReaderAsync();

                var idOrdinal = reader.GetOrdinal("Id");
                var numberOrdinal = reader.GetOrdinal("CertificateNumber");
                var fileOrdinal = reader.GetOrdinal("CertificateFile");
                var dateOrdinal = reader.GetOrdinal("User_Creation_Date");

                while (await reader.ReadAsync())
                {
                    model.Certificates.Add(new CertificateCardViewModel
                    {
                        Id = Convert.ToInt32(reader.GetValue(idOrdinal)),

                        CertificateNumber = reader.IsDBNull(numberOrdinal)
                            ? string.Empty
                            : reader.GetValue(numberOrdinal).ToString()
                                ?? string.Empty,

                        CertificateFile = reader.IsDBNull(fileOrdinal)
                            ? string.Empty
                            : reader.GetValue(fileOrdinal).ToString()
                                ?? string.Empty,

                        CertificateDate = reader.IsDBNull(dateOrdinal)
                            ? null
                            : reader.GetDateTime(dateOrdinal)
                    });
                }
            }

            return View("Results", model);
        }

        [HttpGet]
        public async Task<IActionResult> OpenCertificate(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return BadRequest("رقم الشهادة غير صالح.");
            }

            var countryId =
                HttpContext.Session.GetInt32("CertificateCountryId");

            if (countryId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var connectionString =
                _configuration.GetConnectionString("CertificateDb");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return StatusCode(
                    500,
                    "Connection string باسم CertificateDb غير موجود في الإعدادات.");
            }

            const string sql = @"
                SELECT TOP (1) CertificateFile
                FROM dbo.CertificateFile
                WHERE CONVERT(NVARCHAR(100), CertificateNumber) = @CertificateNumber
                  AND CountryId = @CountryId;";

            string? storedPath;

            await using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                await using var command = new SqlCommand(sql, connection);

                command.Parameters.Add(
                    "@CertificateNumber",
                    SqlDbType.NVarChar,
                    100).Value = number.Trim();

                command.Parameters.Add(
                    "@CountryId",
                    SqlDbType.Int).Value = countryId.Value;

                var result = await command.ExecuteScalarAsync();

                storedPath = result == null || result == DBNull.Value
                    ? null
                    : result.ToString();
            }

            if (string.IsNullOrWhiteSpace(storedPath))
            {
                return NotFound(
                    "مسار ملف الشهادة غير موجود في قاعدة البيانات.");
            }

            var relativePath = storedPath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);

            var fullPath = Path.GetFullPath(
                Path.Combine(@"F:\", relativePath));

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(
                    $"ملف الشهادة غير موجود على هذا المسار: {fullPath}");
            }

            var extension = Path.GetExtension(fullPath).ToLowerInvariant();

            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            var fileBytes =
                await System.IO.File.ReadAllBytesAsync(fullPath);

            if (contentType == "application/octet-stream")
            {
                return File(
                    fileBytes,
                    contentType,
                    Path.GetFileName(fullPath));
            }

            return File(fileBytes, contentType);
        }

        private static void AddResultsParameters(
            SqlCommand command,
            int countryId,
            DateTime? dateFrom,
            DateTime? dateTo,
            string? certificateNumber)
        {
            command.Parameters.Add("@CountryId", SqlDbType.Int)
                .Value = countryId;

            command.Parameters.Add("@DateFrom", SqlDbType.Date)
                .Value = dateFrom.HasValue
                    ? (object)dateFrom.Value.Date
                    : DBNull.Value;

            command.Parameters.Add("@DateTo", SqlDbType.Date)
                .Value = dateTo.HasValue
                    ? (object)dateTo.Value.Date
                    : DBNull.Value;

            command.Parameters.Add(
                "@CertificateNumber",
                SqlDbType.NVarChar,
                100).Value = string.IsNullOrWhiteSpace(certificateNumber)
                    ? DBNull.Value
                    : certificateNumber.Trim();
        }
    }

    public class CertificateResultsViewModel
    {
        public List<CertificateCardViewModel> Certificates { get; set; } = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public string? CertificateNumber { get; set; }
    }

    public class CertificateCardViewModel
    {
        public int Id { get; set; }

        public string CertificateNumber { get; set; } = string.Empty;

        public string CertificateFile { get; set; } = string.Empty;

        public DateTime? CertificateDate { get; set; }
    }
}