using System.Text.RegularExpressions;

namespace ValidationLibrary
{
    public static class EmailValidator
    {
        // Simple regex for email validation
        // Note: For production use, consider more robust validation or libraries.
        // This regex checks for a basic email structure: local-part@domain
        // Compiled for performance; ignore-case so domain/local-case differences aren't considered
        private static readonly Regex EmailRegex = new(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static ValidationResults Validate(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new ValidationResults { IsValid = false, Message = "Email is required." };
            }

            bool isValid = EmailRegex.IsMatch(email);
            return new ValidationResults
            {
                IsValid = isValid,
                Message = isValid ? "Email is valid." : "Invalid email format."
            };
        }
    }
}