using Microsoft.AspNetCore.StaticFiles;

namespace Capqwebsite.Infrastructure;

public static class CertificateAttachmentFiles
{
    public static string ResolvePath(string storedPath, string networkShare)
    {
        string path = storedPath.Trim().Replace('/', '\\');
        if (path.StartsWith(@"\\")) return path;

        // Old certificate rows may contain a drive-qualified path such as F:\plant\...
        // The drive is local to the legacy server, so retain only its relative part.
        if (Path.IsPathFullyQualified(path))
            path = path[Path.GetPathRoot(path)!.Length..];

        string server = networkShare.Trim().Trim('\\', '/');
        if (string.IsNullOrWhiteSpace(server))
            throw new InvalidOperationException("Path_NetworkShare is not configured.");
        return $@"\\{server}\{path.TrimStart('\\')}";
    }

    public static string ContentType(string path)
    {
        var provider = new FileExtensionContentTypeProvider();
        return provider.TryGetContentType(path, out string? type)
            ? type
            : "application/octet-stream";
    }
}
