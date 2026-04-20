using System.ComponentModel.DataAnnotations;

namespace Twitter.Application.Models.Requests.Auth.Register
{
    public class RegisterInitAuthRequest
    {

        [Required]
        public string Email { get; set; }
    }
}
