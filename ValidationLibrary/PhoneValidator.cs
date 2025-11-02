using System.Text.RegularExpressions;

namespace ValidationLibrary
{
    public static class PhoneValidator
    {
        // US phone number pattern: (123) 456-7890, 123-456-7890, 123.456.7890, 1234567890, +1 123-456-7890
        // Regex pattern explanation:
        // ^(\+1\s?)?   : Optional country code +1 followed by optional space
        // (\(?\d{3}\)? : Area code with optional parentheses
        // [\s.-]?      : Optional separator (space, dot, or dash)
        // \d{3}        : First 3 digits
        // [\s.-]?      : Optional separator (space, dot, or dash)
        // \d{4}$       : Last 4 digits
        // This regex allows for various common US phone number formats.
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