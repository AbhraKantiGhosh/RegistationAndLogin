
using Microsoft.Extensions.Configuration;
using Moq;
using RegistationAndLogin.Domain.DTOs;
using RegistationAndLogin.Domain.Entity;
using RegistationAndLogin.Repositories;
using RegistationAndLogin.Service;
using System;
using Xunit;

namespace RegistationAndLoginTestProject
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepo> _userRepoMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepo>();
            _configMock = new Mock<IConfiguration>();

            // Mock JWT config values
            var sectionMock = new Mock<IConfigurationSection>();
            sectionMock.Setup(x => x["Key"]).Returns("ThisIsASecretKeyForJwtTokenTesting1234");
            sectionMock.Setup(x => x["Issuer"]).Returns("TestIssuer");
            sectionMock.Setup(x => x["Audience"]).Returns("TestAudience");

            _configMock.Setup(x => x.GetSection("Jwt")).Returns(sectionMock.Object);

            _authService = new AuthService(_userRepoMock.Object, _configMock.Object);
        }

        #region RegisterAsync Tests
        [Fact]
        public async Task RegisterAsync_ReturnsAuthResponceDto_OnsuccessfulRegistration()
        {
            // Arrange
            var registerDto = new RegisterDto { FirstName = "John", LastName = "Doe", Email = "test@example.com", Password = "Password123", PhoneNumber = "1234567890" };
            var user = new User { FirstName = registerDto.FirstName, LastName = registerDto.LastName, Email = registerDto.Email, Password = registerDto.Password, PhoneNumber = registerDto.PhoneNumber };

            _userRepoMock.Setup(r => r.EmailExistsAsync(registerDto.Email)).ReturnsAsync(false);

            _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(user);

            //Act

            var result = await _authService.RegisterAsync(registerDto);

            //assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.Equal(registerDto.Email, result.User.Email);


        }
        [Fact]
        public async Task RegisterAsync_ThrowsException_WhenUserAlreadyExists()
        {
            // Arrange
            var registerDto = new RegisterDto { FirstName = "John", LastName = "Doe", Email = "test@example.com", Password = "Password123", PhoneNumber = "1234567890" };
            var user = new User { FirstName = registerDto.FirstName, LastName = registerDto.LastName, Email = registerDto.Email, Password = registerDto.Password, PhoneNumber = registerDto.PhoneNumber };


            _userRepoMock.Setup(r => r.EmailExistsAsync(registerDto.Email)).ReturnsAsync(true);
            //Act
            var exception = await Assert.ThrowsAsync<Exception>(() => _authService.RegisterAsync(registerDto));

            //Assert
            Assert.Equal("User already exists with this email", exception.Message);
        }

        #endregion

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_ReturnsAuthResponceDto_OnSuccessfulLogin()
        {
            // Arrange
            var logInDto = new LogInDto { Email = "test@example.com", Password = "Password123" };

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = logInDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(logInDto.Password),
            };

            _userRepoMock.Setup(r => r.GetByEmailAsync(logInDto.Email.ToLowerInvariant())).ReturnsAsync(user);

            var result = await _authService.LoginAsync(logInDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.Equal(user.Email, result.User.Email);
        }
        [Fact]
        public async Task LoginAsync_ThrowsException_WhenInvalidCredentials()
        {
            // Arrange
            var logInDto = new LogInDto { Email = "test@example.com", Password = "Password123" };

            var user = new User
            {
             
                Email = logInDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword("WrongPassword123"),
            };

            _userRepoMock.Setup(r => r.GetByEmailAsync(logInDto.Email.ToLowerInvariant())).ReturnsAsync(user);

            var exception= await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(logInDto));

            // Assert
            Assert.Equal("Invalid email or password", exception.Message);

        }

        #endregion
    }      
}