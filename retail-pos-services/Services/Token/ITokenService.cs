using QSRAPIServices.Models;

namespace RetailPosToken.Services.Token
{
    public interface ITokenService
    {
        string GenerateToken(dynamic obj);
        string GenerateRefreshToken();
        bool ValidateRefreshToken(string token);
    }
}
