namespace Capqwebsite.Infrastructure;

public static class CertificateAttachmentFiles
{
    public static string ResolvePath(string storedPath, string attachmentServer)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
            throw new ArgumentException("مسار الشهادة فارغ.", nameof(storedPath));

        string server = attachmentServer.Trim().Trim('\\', '/');
        if (string.IsNullOrWhiteSpace(server)
            || server.IndexOfAny(new[] { '\\', '/', ':' }) >= 0)
        {
            throw new ArgumentException(
                "اسم سيرفر المرفقات أو عنوانه غير صالح.",
                nameof(attachmentServer));
        }

        string path = storedPath.Trim().Replace('/', '\\');
        if (path.Split('\\', StringSplitOptions.RemoveEmptyEntries)
            .Any(segment => segment is "." or ".."))
        {
            throw new ArgumentException("مسار الشهادة غير صالح.", nameof(storedPath));
        }

        string shareRoot = $@"\\{server}\plant";
        string fullPath;

        if (path.StartsWith(@"\\", StringComparison.Ordinal))
        {
            fullPath = Path.GetFullPath(path);
        }
        else
        {
            path = path.TrimStart('\\');
            if (path.Equals("plant", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("مسار الشهادة لا يحتوي اسم ملف.", nameof(storedPath));

            if (path.StartsWith(@"plant\", StringComparison.OrdinalIgnoreCase))
                path = path[6..];

            fullPath = Path.GetFullPath(Path.Combine(shareRoot, path));
        }

        string rootWithSeparator = shareRoot + "\\";
        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "ملف الشهادة يجب أن يكون داخل مشاركة plant.",
                nameof(storedPath));
        }

        return fullPath;
    }

    public static string ContentType(string filePath) =>
        Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".tif" or ".tiff" => "image/tiff",
            _ => "application/octet-stream"
        };
}
