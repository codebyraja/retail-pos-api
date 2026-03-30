namespace RetailPos.Licensing;

public class LicenseDto
{
    public bool Allowed { get; set; }
    public string? LockMessage { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

