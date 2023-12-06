using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MindustryLauncher.Avalonia;

public static class Utils
{
    public static async Task DownloadFile(this HttpClient client, string url, string destinationPath,
        IProgress<float>? progress = null)
    {
        using HttpResponseMessage response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;
        response.EnsureSuccessStatusCode();

        await using Stream contentStream = await response.Content.ReadAsStreamAsync(),
            fileStream = File.Create(destinationPath);
        
        long totalRead = 0L;
        long totalReads = 0L;
        byte[] buffer = new byte[8192];
        bool isMoreToRead = true;

        float contentLength = (float) response.Content.Headers.ContentLength!;

        do
        {
            int read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
            if (read == 0)
            {
                isMoreToRead = false;
            }
            else
            {
                await fileStream.WriteAsync(buffer, 0, read);

                totalRead += read;
                totalReads += 1;

                progress?.Report(totalRead / contentLength * 100);
            }
        } while (isMoreToRead);

        fileStream.Close();
    }

    public static string GetSha256OfFile(string path)
    {
        using SHA256 sha256 = SHA256.Create();
        using FileStream fileStream = File.OpenRead(path);
        return Convert.ToBase64String(sha256.ComputeHash(fileStream));
    }

    public static bool VerifyIntegrity(this ILocalInstance instance)
    {
        string jarPath = instance.JarPath;
        string jarHash = GetSha256OfFile(jarPath);
        return jarHash == instance.JarHash;
    }
}