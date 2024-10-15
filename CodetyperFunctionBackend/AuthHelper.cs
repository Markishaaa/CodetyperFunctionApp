using CodetyperFunctionBackend.Model;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CodetyperFunctionBackend
{
    public static class AuthHelper
    {
        private static readonly string _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET not set");
        private static readonly string _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new InvalidOperationException("JWT_ISSUER not set");
        private static readonly string _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new InvalidOperationException("JWT_AUDIENCE not set");

        public static string GenerateJwtToken(string username, string roleName, string userId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim("username", username),
            new Claim("role", roleName),
            new Claim("userId", userId)
        };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static ClaimsPrincipal ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!);
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            try
            {
                // validate the token
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
                    ValidateAudience = true,
                    ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                }, out SecurityToken validatedToken);

                return claimsPrincipal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation failed: {ex.Message}");
                // token validation failed
                return null;
            }
        }

        private static string GetToken(HttpRequestData req)
        {
            string authHeader = null;
            if (req.Headers.TryGetValues("Authorization", out var headerValues))
            {
                authHeader = headerValues.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            return authHeader.Substring("Bearer ".Length).Trim(); // extract the token
        }

        public static bool IsUserAuthorized(HttpRequestData req, params Roles[] roles)
        {
            var token = GetToken(req);

            var claimsPrincipal = ValidateJwtToken(token);
            if (claimsPrincipal == null)
            {
                Console.WriteLine("session expired");
                return false; // token validation failed
            }

            var username = claimsPrincipal.FindFirst("username")?.Value;
            var role = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value
                       ?? claimsPrincipal.FindFirst("role")?.Value;
            Console.WriteLine($"{username} {role}");

            if (string.IsNullOrEmpty(role))
            {
                return false;
            }

            if (!Enum.TryParse(typeof(Roles), role, true, out var roleEnumObj) || !(roleEnumObj is Roles roleEnum))
            {
                Console.WriteLine("Invalid role claim in token");
                return false; // invalid role in token
            }

            if (roles.Contains(roleEnum))
            {
                return true;
            }

            return false;
        }
    }
}
