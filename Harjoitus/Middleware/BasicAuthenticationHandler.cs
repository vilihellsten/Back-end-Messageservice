using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Harjoitus.Models;
using Harjoitus.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using NuGet.Protocol;

namespace Harjoitus.Middleware
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserRepository _repository;
        private readonly IUserAuthenticationservice _userAuthenticationService;

        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IUserRepository repository, IUserAuthenticationservice userAuthenticationService) : base(options, logger, encoder, clock)
        {
            _repository = repository;
            _userAuthenticationService = userAuthenticationService;      
        }
        
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string userName = "";
            string password = "";

            User? user;

            var endpoint = Context.GetEndpoint();
            if(endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return AuthenticateResult.NoResult();
            }

            if(!Request.Headers.ContainsKey("Authorization"))//kenttä headerissä postmanissa
            {
                return AuthenticateResult.Fail("Authorization header missing");
            }
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialData = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialData).Split(new[] {':'}, 2);
                userName = credentials[0];
                password = credentials[1];

                // alapuolella tarkistetaan tunnus/salasana tietokannasta

                user = await _userAuthenticationService.Authenticate(userName, password); 

                if (user==null) 
                {
                    return AuthenticateResult.Fail("Unauthorized");
                }
                
            }
            catch
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userName) // Claimtypes.Role voi antaa rooleja 
                
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
