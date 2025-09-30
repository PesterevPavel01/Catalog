using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Entities.Configurations;
using Catalog.Domain.Dto.Authorization;
using Catalog.Domain.Entities.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Catalog.Application.Processors.AuthorizationProcessor
{
    public sealed class AuthenticationProcessor
    {
        private readonly IUnitOfWork _unitOfWork = null!;
        private readonly AuthorizationSettings _authorizationSettings;

        public AuthenticationProcessor(IUnitOfWork unitOfWork, IOptions<AuthorizationSettings> authorizationSetts)
        {
            _unitOfWork = unitOfWork;
            _authorizationSettings = authorizationSetts.Value;
        }

        public async Task<String> ProcessAsync(LoginDto model, CancellationToken cancellationToken)
        {
            var user = (await _unitOfWork.GetRepository<ApplicationUser>()
                .GetAllAsync(
                    predicate: x => x.UserName == model.UserName,
                    trackingType: TrackingType.Tracking
                )).FirstOrDefault(user => user.CheckPassword(model.Password));

            if (user is null)
                throw new ArgumentException("User not found");

            var userToken = (await _unitOfWork.GetRepository<UserToken>().GetAllAsync(predicate: x => x.UserId == user.Id)).FirstOrDefault();

            var refreshToken = GenerateRefreshToken();

            if (userToken is null)
            {
                userToken = new UserToken(user.Id)
                {
                    RefreshToken = refreshToken,
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7),
                    User = user
                };

                await _unitOfWork.GetRepository<UserToken>().InsertAsync(userToken, cancellationToken);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                userToken.RefreshToken = refreshToken;
                userToken.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            }

            var accessToken = GenerateAccessToken(user);

            return accessToken;
        }

        private string GenerateAccessToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, "PesterevPavel90@gmail.com"),
                new Claim(ClaimTypes.Role, "Administrator") // Добавляем claim с ролью
            };

            byte[] secretBytes = Encoding.UTF8.GetBytes(_authorizationSettings.SecretKey);
            var key = new SymmetricSecurityKey(secretBytes);
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _authorizationSettings.Issuer,
                _authorizationSettings.Audience,
                claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials
            );

            var value = new JwtSecurityTokenHandler().WriteToken(token);

            return value;
        }

        private string GenerateRefreshToken()
        {
            var randomNumbers = new byte[32];
            using var randomeNumberGenerator = RandomNumberGenerator.Create();
            randomeNumberGenerator.GetBytes(randomNumbers);
            return Convert.ToBase64String(randomNumbers);
        }
    }
}
