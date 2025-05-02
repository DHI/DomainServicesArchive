namespace DHI.Services;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>
///     Represents the result of a password validation.
/// </summary>
public class PasswordValidationResult
{
    private readonly List<PasswordError> _errors = new();

    /// <summary>
    ///     Gets a value indicating whether a password validation was successful.
    /// </summary>
    /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
    public bool Success { get; private set; }

    /// <summary>
    ///     Gets the possible errors of a password validation.
    /// </summary>
    public IEnumerable<PasswordError> Errors => _errors;

    /// <summary>
    ///     Creates a failed password validation result with the specified errors.
    /// </summary>
    /// <param name="errors">The password validation errors.</param>
    public static PasswordValidationResult Failed(params PasswordError[] errors)
    {
        Guard.Against.NullOrEmpty(errors, nameof(errors));
        var result = new PasswordValidationResult {Success = false};
        result._errors.AddRange(errors);
        return result;
    }

    /// <summary>
    ///     Creates a successful password validation result.
    /// </summary>
    public static PasswordValidationResult Succeeded()
    {
        return new PasswordValidationResult {Success = true};
    }

    /// <summary>
    ///     Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    public override string ToString()
    {
        return Success ? "Success" : string.Format(CultureInfo.InvariantCulture, "{0} : {1}", "Failed", string.Join(",", Errors.Select(e => e.Code).ToList()));
    }
}