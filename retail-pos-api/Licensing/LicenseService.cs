namespace RetailPos.Licensing;

public class LicenseService
{
    private readonly IHttpClientFactory _factory;

    public LicenseService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<LicenseDto> ValidateAsync()
    {
        #if DEBUG
                return new LicenseDto { Allowed = true };
        #endif

        var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-LICENSE-KEY",
            Environment.GetEnvironmentVariable("LICENSE_KEY"));

        return await client.GetFromJsonAsync<LicenseDto>(
            "https://license.yourdomain.com/api/license/validate?appId=QSR_ADMIN");
    }
}
