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
