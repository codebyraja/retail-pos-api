using System;
public class EmailTemplates
{
    private readonly string _templateFolderPath;

    public EmailTemplates()
    {
        //_templateFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates");
        _templateFolderPath = Path.Combine(AppContext.BaseDirectory, "EmailTemplates");
    }

    public string GetMessage(string fileName, string otp = "")
    {
        try
        {
            string path = Path.Combine(_templateFolderPath, $"{fileName}.html");

            //string filePath = Path.Combine(basePath, $"{templateName}.html");

            //if (!File.Exists(filePath))
            //    return $"Template {templateName} not found at {filePath}";

            if (!File.Exists(path))
                return $"Template {fileName} not found at {path}";

            string html = File.ReadAllText(path);

            if (!string.IsNullOrEmpty(otp))
                html = html.Replace("{OTP}", otp);

            return html;
        }
        catch (Exception ex)
        {
            return $"Error loading template: {ex.Message}";
        }
    }
}