namespace ChatApp.UserService.Core.RequestDTOs
{
    public class UpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Status { get; set; }
    }
}
