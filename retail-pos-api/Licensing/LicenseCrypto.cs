using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RetailPos.Licensing;

public static class LicenseCrypto
{
    private static readonly byte[] Key =
        Encoding.UTF8.GetBytes("CHANGE_THIS_32_CHAR_SECRET_KEY!!");

    private static readonly string FilePath = "App_Data/license.dat";

    public static void Save(LicenseDto license)
    {
        Directory.CreateDirectory("App_Data");

        var json = JsonSerializer.Serialize(license);

        using var aes = Aes.Create();
        aes.Key = Key;
        aes.GenerateIV();

        var encrypted = aes.CreateEncryptor()
            .TransformFinalBlock(Encoding.UTF8.GetBytes(json), 0, json.Length);

        File.WriteAllBytes(FilePath, aes.IV.Concat(encrypted).ToArray());
    }

    public static LicenseDto? Load()
    {
        if (!File.Exists(FilePath))
            return null;

        var data = File.ReadAllBytes(FilePath);

        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = data.Take(16).ToArray();

        var decrypted = aes.CreateDecryptor()
            .TransformFinalBlock(data, 16, data.Length - 16);

        return JsonSerializer.Deserialize<LicenseDto>(
            Encoding.UTF8.GetString(decrypted));
    }
}
