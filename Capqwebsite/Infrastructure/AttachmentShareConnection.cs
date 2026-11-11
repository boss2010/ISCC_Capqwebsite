using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Capqwebsite.Infrastructure;

public static class AttachmentShareConnection
{
    public static void Connect(string filePath, IConfiguration configuration)
    {
        string? sharePath = GetSharePath(filePath);
        if (sharePath is null)
            return;

        if (!OperatingSystem.IsWindows())
            throw new IOException("الاتصال بالمجلد المشترك يتطلب تشغيل التطبيق على Windows.");

        string configuredServer = (configuration["Path_NetworkShare"] ?? string.Empty)
            .Trim()
            .Trim('\\', '/');

        bool isConfiguredServer = sharePath.StartsWith(
            $@"\\{configuredServer}\",
            StringComparison.OrdinalIgnoreCase);

        string? username = isConfiguredServer
            ? configuration["NetworkShare:Username"]
            : null;
        string? password = isConfiguredServer
            ? configuration["NetworkShare:Password"]
            : null;

        if (string.IsNullOrWhiteSpace(username))
        {
            username = null;
            password = null;
        }

        var resource = new NetResource
        {
            Type = 1,
            RemoteName = sharePath
        };

        int result = WNetUseConnection(
            IntPtr.Zero,
            resource,
            password,
            username,
            0,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

        if (result is 0 or 1219)
            return;

        throw new IOException(
            $"تعذر الاتصال بالمجلد المشترك {sharePath} (رمز {result}): " +
            new Win32Exception(result).Message);
    }

    private static string? GetSharePath(string filePath)
    {
        string path = filePath.Replace('/', '\\');
        if (!path.StartsWith(@"\\", StringComparison.Ordinal))
            return null;

        string[] parts = path[2..]
            .Split('\\', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
            throw new IOException("مسار المجلد المشترك غير صحيح.");

        return $@"\\{parts[0]}\{parts[1]}";
    }

    [DllImport("mpr.dll", EntryPoint = "WNetUseConnectionW", CharSet = CharSet.Unicode)]
    private static extern int WNetUseConnection(
        IntPtr owner,
        NetResource resource,
        string? password,
        string? username,
        int flags,
        IntPtr accessName,
        IntPtr bufferSize,
        IntPtr result);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private sealed class NetResource
    {
        public int Scope;
        public int Type;
        public int DisplayType;
        public int Usage;
        public string? LocalName;
        public string? RemoteName;
        public string? Comment;
        public string? Provider;
    }
}
