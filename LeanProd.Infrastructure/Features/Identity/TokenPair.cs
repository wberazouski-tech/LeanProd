namespace LeanProd.Infrastructure.Features.Identity;

public sealed record TokenPair(string AccessToken, DateTime AccessTokenExpiresAtUtc,
    string RefreshToken, DateTime RefreshTokenExpiresAtUtc);
