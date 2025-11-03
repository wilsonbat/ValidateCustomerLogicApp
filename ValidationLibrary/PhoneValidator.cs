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
        public static ValidationResults Validate(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return new ValidationResults { IsValid = false, Message = "Phone number is required." };
            }
            // Validate a US phone number. We provide clearer error messages used by tests:
            // - returns "Phone number is required." for null/empty
            // - returns "Invalid phone number format." when letters are present
            // - returns "Phone number must be 10 digits." for incorrect digit counts
            // - accepts an optional leading '1' (country code) and common separators
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return new ValidationResults { IsValid = false, Message = "Phone number is required." };
            }

            // If the input contains alphabetic characters, treat as invalid format
            if (Regex.IsMatch(phoneNumber, "[A-Za-z]"))
            {
                return new ValidationResults { IsValid = false, Message = "Invalid phone number format." };
            }

            // Strip non-digits to analyze the numeric content
            var digitsOnly = Regex.Replace(phoneNumber, "\\D", string.Empty);

            // Check if the length is valid (10 digits, or 11 digits with leading '1')
            if (digitsOnly.Length == 11)
            {
                if (!digitsOnly.StartsWith("1"))
                {
                    return new ValidationResults { IsValid = false, Message = "Phone number must be 10 digits." };
                }
                // Remove the leading '1' for validation
                digitsOnly = digitsOnly.Substring(1);
            }
            else if (digitsOnly.Length != 10)
            {
                return new ValidationResults { IsValid = false, Message = "Phone number must be 10 digits." };
            }

            return new ValidationResults { IsValid = true, Message = "Phone number is valid." };
        }
    }
}