using System.Text.RegularExpressions;

namespace ValidationLibrary
{
    public static class PhoneValidator
    {
        private static readonly Regex PhoneRegex = new(
            @"^(\+1\s?)?(\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4})$",
            RegexOptions.Compiled);

        public static ValidationResults Validate(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return new ValidationResults { IsValid = false, Message = "Phone number is required." };
            }

            bool isValid = PhoneRegex.IsMatch(phoneNumber);
            return new ValidationResults
            {
                IsValid = isValid,
                Message = isValid ? "Phone number is valid." : "Invalid phone number format (US expected)."
            };
        }
    }
}