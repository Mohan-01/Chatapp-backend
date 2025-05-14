using Shared.Enums.User;

namespace AuthService.Core.Constants
{
    public static class RoleDescriptions
    {
        public static readonly Dictionary<UserRole, string> Descriptions = new()
        {
            { UserRole.Admin, "Has full access to manage the system." },
            { UserRole.Member, "User with limited access to the system." }
        };

        public static string GetDescription(UserRole role)
        {
            return Descriptions.TryGetValue(role, out var description)
                ? description
                : "No description available.";
        }
    }
}
