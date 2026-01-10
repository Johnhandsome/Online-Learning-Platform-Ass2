namespace Online_Learning_Platform_Ass1.Service.Validators;

public static class ValidationExtensions
{
    public static List<string> GetErrors(this ValidationResult result)
    {
        return [.. result.Errors.Select(e => e.ErrorMessage)];
    }
}
