//***********************************************************************************
//Program: TokenConfig.cs
//Description: Token settings config model
//Date: Sep 29, 2025
//Author: John Nasitem
//***********************************************************************************



namespace takethetab_server.Domain.Entities
{
    public class TokenConfig
    {
        public int AccessTokenLifeSpanHours { get; set; }
        public int RefreshTokenLifeSpanDays { get; set; }
        public string JwtIssuer { get; set; } = null!;
        public string JwtAudience { get; set; } = null!;
        public string JwtEnvironmentVariableName { get; set; } = null!;
    }
}
