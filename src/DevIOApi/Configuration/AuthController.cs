using DevIO.Business.Intefaces;
using DevIOApi.Controllers;
using DevIOApi.Extensions;
using DevIOApi.ViewModel;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DevIOApi.Configuration
{
    [Route("api")]
    [DisableCors]
    public class AuthController : MainController
    {
        private readonly SignInManager<IdentityUser> _signInManeger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSettings;

        public AuthController(INotificador notificador, SignInManager<IdentityUser> signInManeger,
            UserManager<IdentityUser> userManager, 
            IOptions<AppSettings> appSettings, 
            IUser user) : base(notificador, user)
        {
            _signInManeger = signInManeger;
            _userManager = userManager;
            _appSettings = appSettings.Value;
        }

        [HttpPost("nova-conta")]
        [EnableCors("Development")]
        public async Task<IActionResult> Registrar([FromBody]RegisterUserViewModel registrarUser)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = new IdentityUser
            {
                UserName = registrarUser.Email,
                Email = registrarUser.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, registrarUser.Password);
            if (result.Succeeded)
            {
                await _signInManeger.SignInAsync(user, isPersistent: false);
                return CustomResponse(await GerarJwt(user.Email));
            }
            foreach (var erro in result.Errors)
            {
                NotificarErro(erro.Description);
            }
            return CustomResponse(registrarUser);
        }

        [HttpPost("entrar")]
        public async Task<IActionResult> Login([FromBody] LoginUserViewModel loginUser)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var result = await _signInManeger.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

            if (result.Succeeded)
            {
                return CustomResponse(await GerarJwt(loginUser.Email));
            }
            if (result.IsLockedOut)
            {
                NotificarErro("Usuario bloqueado por numeros de tentativas invalidas");
                return CustomResponse(loginUser);
            }

            NotificarErro("Usuario ou senhas incorretos");
            return CustomResponse(loginUser);
        }

        private async Task<LoginResponseViewModel> GerarJwt(string email )
        {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            claims.Add(new Claim(type: JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(type: JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(type: JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(type: JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(type: JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));
            foreach(var userRole in userRoles)
            {
                claims.Add(new Claim(type: "role", value: userRole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            var tokenHandler = new JwtSecurityTokenHandler();
            var Key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Emissor,
                Audience = _appSettings.ValidoEm,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_appSettings.ExpiracaoHoras),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Key), algorithm: SecurityAlgorithms.HmacSha256Signature)
            });

            var encodedToken =  tokenHandler.WriteToken(token);
            var response = new LoginResponseViewModel
            {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(_appSettings.ExpiracaoHoras).TotalSeconds,
                UserToken = new UserTokenViewlModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new ClaimViewModel{Type = c.Type, Value = c.Value})
                }
            };
            return response;
        }

        private object ToUnixEpochDate(DateTime date) 
            =>(long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(year:1970, month:1, day:1, hour:0, minute:0, second:0, offset:TimeSpan.Zero)).TotalSeconds);
        
    }
}
