using System.ComponentModel.DataAnnotations;
namespace WeatherAggregator.Api.Models.Dtos
{
    public class UserRegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}