namespace FrontendView.Models
{
    public class LoginResponseModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string IsAdmin { get; set; }
        public string Token { get; set; } // This will hold the JWT Token
    }
}
