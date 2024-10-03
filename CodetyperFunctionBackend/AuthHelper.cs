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

            try
            {
                // validate the token
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return claimsPrincipal;
            }
            catch
            {
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

        public static bool IsUserAuthorized(HttpRequestData req, params string[] roles)
        {
            var token = GetToken(req);

            var claimsPrincipal = ValidateJwtToken(token);
            if (claimsPrincipal == null)
            {
                Console.WriteLine("session expired");
                // token validation failed, handle the error
                return false;
            }

            var username = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            var role = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;
            Console.WriteLine("role: " + role);

            foreach (var roleClaim in roles)
            {
                Console.WriteLine("role claim" + roleClaim);
                if (roleClaim.Equals(role))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
