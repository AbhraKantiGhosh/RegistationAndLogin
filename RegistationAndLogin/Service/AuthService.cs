
using CryptSharp.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RegistationAndLogin.Domain.DTOs;
using RegistationAndLogin.Domain.Entity;
using RegistationAndLogin.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace RegistationAndLogin.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepo _userRepo;
        private readonly IConfiguration _config;

        public AuthService(IUserRepo userRepo, IConfiguration config)
        {
            _userRepo = userRepo;
            _config = config;
        }
        public async Task<AuthResponceDto> LoginAsync(LogInDto logInDto)
        {
            var user = await _userRepo.GetByEmailAsync(logInDto.Email.ToLowerInvariant());
            bool varifyPassword = BCrypt.Net.BCrypt.Verify(logInDto.Password, user.Password);

            if(user == null || !varifyPassword)
            {
                throw new Exception("Invalid email or password");
            }

            var token = GenerateToken(user);

            return new AuthResponceDto
            {
                Token = token,
                User = new UserDto
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                }
            };

        }

        public async Task<AuthResponceDto> RegisterAsync(RegisterDto registerDto)
        {
           if(await _userRepo.EmailExistsAsync(registerDto.Email))
            {
                throw new Exception("User already exists with this email");
           }

            var users = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                PhoneNumber = registerDto.PhoneNumber
            };

            var createUser=await _userRepo.CreateAsync(users);

            var token = GenerateToken(createUser);

            return new AuthResponceDto
            {
                Token = token,
                User = new UserDto
                {
                    FirstName = users.FirstName,
                    LastName = users.LastName,
                    Email = users.Email,
                    PhoneNumber = users.PhoneNumber
                }
            };

        }
        public async Task<UserDto?> GetUserDetailsAsync(Guid userId)
        {
            var user =await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return new UserDto
                {
                    
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber

                };
           
        }

        public async Task<UserDto> UpdateUserAsync(Guid userId, UserUpdateDto userUpdateDto)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            if(!string.IsNullOrEmpty(userUpdateDto.FirstName))
            {
                user.FirstName = userUpdateDto.FirstName;
            }
            if (!string.IsNullOrEmpty(userUpdateDto.LastName))
            {
                user.LastName = userUpdateDto.LastName;
            }
            if (!string.IsNullOrEmpty(userUpdateDto.Email))
            {
                if(user.Email.ToLowerInvariant()!=userUpdateDto.Email.ToLowerInvariant() && await _userRepo.EmailExistsAsync(userUpdateDto.Email))
                {
                    throw new Exception("Email already exists");
                }
                user.Email = userUpdateDto.Email.ToLowerInvariant();
            }
            if (!string.IsNullOrEmpty(userUpdateDto.PhoneNumber))
            {
                user.PhoneNumber = userUpdateDto.PhoneNumber;
            }
            if (!string.IsNullOrEmpty(userUpdateDto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(userUpdateDto.Password);
            }
           var updatedUser = await _userRepo.UpdateAsync(user);
            return new UserDto
            {
                FirstName = updatedUser.FirstName,
                LastName = updatedUser.LastName,
                Email = updatedUser.Email,
                PhoneNumber = updatedUser.PhoneNumber
            };
        }
        public string GenerateToken(User user)
        {
            var jwt=_config.GetSection("Jwt");
            var secretKey= jwt["Key"];
            var issuer = jwt["Issuer"];
            var audience = jwt["Audience"];

            var key=new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claim = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name,$"{ user.FirstName }{user.LastName}"),
                new Claim("UserId", user.Id.ToString())

            };

            var token=new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claim,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );



            return  new JwtSecurityTokenHandler().WriteToken(token);
        }

       
    }
}
