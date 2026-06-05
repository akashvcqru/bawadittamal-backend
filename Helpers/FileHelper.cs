using System;
using System.IO;

namespace BawaDittaMal.Api.Helpers
{
    public static class FileHelper
    {
        public static string SaveBase64Image(string base64String, string rootPath, string subFolder = "")
        {
            if (string.IsNullOrWhiteSpace(base64String))
                return string.Empty;

            // Check if it's base64 data URL
            if (!base64String.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) || 
                !base64String.Contains(";base64,"))
            {
                return base64String;
            }

            try
            {
                var parts = base64String.Split(new[] { ";base64," }, StringSplitOptions.None);
                if (parts.Length != 2)
                    return base64String;

                var metadata = parts[0];
                var base64Data = parts[1];

                // Determine file extension
                string extension = ".jpg"; // default
                if (metadata.Contains("image/png", StringComparison.OrdinalIgnoreCase))
                    extension = ".png";
                else if (metadata.Contains("image/gif", StringComparison.OrdinalIgnoreCase))
                    extension = ".gif";
                else if (metadata.Contains("image/webp", StringComparison.OrdinalIgnoreCase))
                    extension = ".webp";
                else if (metadata.Contains("image/svg+xml", StringComparison.OrdinalIgnoreCase))
                    extension = ".svg";

                byte[] imageBytes = Convert.FromBase64String(base64Data);

                // If rootPath is null or empty, use fallback current directory
                if (string.IsNullOrWhiteSpace(rootPath))
                {
                    rootPath = Directory.GetCurrentDirectory();
                }

                // Ensure directory exists
                string uploadsFolder = string.IsNullOrWhiteSpace(subFolder) 
                    ? Path.Combine(rootPath, "uploads")
                    : Path.Combine(rootPath, "uploads", subFolder);

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fileName = $"{Guid.NewGuid():N}{extension}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                File.WriteAllBytes(filePath, imageBytes);

                return string.IsNullOrWhiteSpace(subFolder)
                    ? $"/uploads/{fileName}"
                    : $"/uploads/{subFolder}/{fileName}";
            }
            catch (Exception)
            {
                // If any error occurs, fall back to returning original string
                return base64String;
            }
        }
    }
}
