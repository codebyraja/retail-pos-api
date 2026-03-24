using Microsoft.AspNetCore.Http;

public static class FileUploadHelper
{
    public static async Task<string> SaveFileAsync(IFormFile file, string rootPath, string folderName, string name)
    {
        if (file == null || file.Length == 0)
            return "";

        // 🔹 Clean name
        var cleanName = name?.Replace(" ", "").ToLower() ?? "file";

        // 🔹 Extension
        var extension = Path.GetExtension(file.FileName);

        // 🔹 Unique file name
        var fileName = $"{cleanName}_{Guid.NewGuid()}{extension}";

        // 🔹 Folder path
        var folderPath = Path.Combine(rootPath, "uploads", folderName);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 🔹 Full path
        var fullPath = Path.Combine(folderPath, fileName);

        // 🔹 Save file
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 🔹 Return relative path (DB me save karne ke liye)
        return $"/uploads/{folderName}/{fileName}";
    }
}