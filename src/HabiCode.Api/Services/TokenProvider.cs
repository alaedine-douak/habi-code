﻿using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HabiCode.Api.DTOs.Auth;
using HabiCode.Api.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace HabiCode.Api.Services;

public sealed class TokenProvider(IOptions<JwtAuthOptions> options)
{
    private readonly JwtAuthOptions _jwtAuthOptions = options.Value;

    public AccessTokensDto Create(TokenRequest tokenRequest)
    {
        return new AccessTokensDto(
            GenerateAccessToken(tokenRequest),
            GenerateRefreshToken());
    }

    private string GenerateAccessToken(TokenRequest tokenRequest)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtAuthOptions.Key));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, tokenRequest.UserId),
            new(JwtRegisteredClaimNames.Email, tokenRequest.Email),
            ..tokenRequest.Roles.Select(role => new Claim(ClaimTypes.Role, role)),
        ];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtAuthOptions.ExpirationInMinutes),
            Issuer = _jwtAuthOptions.Issuer,
            Audience = _jwtAuthOptions.Audience,
            SigningCredentials = credentials,
        };

        var handler = new JsonWebTokenHandler();

        string accessToken = handler.CreateToken(tokenDescriptor);

        return accessToken;
    }

    private static string GenerateRefreshToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(randomBytes);
    }
}
