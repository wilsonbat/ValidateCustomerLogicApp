namespace ValidationLibrary
{

    // Represents the results of a validation operation.
    // Contains a boolean indicating if the validation was successful and a message providing additional information. 
    public class ValidationResults
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;

    }
}