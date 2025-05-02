namespace DHI.Services;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
///     Password policy options.
/// </summary>
public class PasswordPolicy
{
    /// <summary>
    ///     Gets or sets the minimum length a password must be. Defaults to 6.
    /// </summary>
    public int RequiredLength { get; set; } = 6;

    /// <summary>
    ///     Gets or sets the minimum number of unique characters which a password must contain. Defaults to 1.
    /// </summary>
    public int RequiredUniqueChars { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the minimum number of Digits which a password must contain. Defaults to 1.
    /// </summary>
    public int MinimumDigit { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the minimum number of NonAlphanumeric Character which a password must contain. Defaults to 1.
    /// </summary>
    public int MinimumNonAlphanumeric { get; set; } = 1;

    /// <summary>
    ///     Gets or sets a flag indicating if passwords must contain a non-alphanumeric character. Defaults to true.
    /// </summary>
    /// <value>True if passwords must contain a non-alphanumeric character, otherwise false.</value>
    public bool RequireNonAlphanumeric { get; set; } = true;

    /// <summary>
    ///     Gets or sets a flag indicating if passwords must contain a lower case ASCII character. Defaults to true.
    /// </summary>
    /// <value>True if passwords must contain a lower case ASCII character.</value>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>
    ///     Gets or sets a flag indicating if passwords must contain a upper case ASCII character. Defaults to true.
    /// </summary>
    /// <value>True if passwords must contain a upper case ASCII character.</value>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>
    ///     Gets or sets a flag indicating if passwords must contain a digit. Defaults to true.
    /// </summary>
    /// <value>True if passwords must contain a digit.</value>
    public bool RequireDigit { get; set; } = true;


    /// <summary>
    ///     Validates the specified password against the configured policy.
    /// </summary>
    /// <param name="password">The password.</param>
    public Task<PasswordValidationResult> ValidateAsync(string password)
    {
        Guard.Against.NullOrEmpty(password, nameof(password));
        var errors = new List<PasswordError>();

        var digitCount = CountDigit(password);
        var nonAlphanumericCount = CountNonAlphanumeric(password);

        if (password.Length < RequiredLength)
        {
            errors.Add(PasswordErrorTypes.PasswordIsTooShort(RequiredLength));
        }

        if (RequiredUniqueChars >= 1 && password.Distinct().Count() < RequiredUniqueChars)
        {
            errors.Add(PasswordErrorTypes.PasswordRequiresUniqueChars(RequiredUniqueChars));
        }

        if (RequireNonAlphanumeric && password.All(IsLetterOrDigit))
        {
            errors.Add(PasswordErrorTypes.PasswordRequiresNonAlphanumeric());
        }

        if (RequireNonAlphanumeric && nonAlphanumericCount < MinimumNonAlphanumeric)
        {
            errors.Add(PasswordErrorTypes.PasswordMinimumNonAlphanumeric(MinimumNonAlphanumeric));
        }

        if (RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add(PasswordErrorTypes.PasswordRequiresLower());
        }

        if (RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add(PasswordErrorTypes.PasswordRequiresUpper());
        }

        if (RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add(PasswordErrorTypes.PasswordRequiresDigit());
        }

        if (RequireDigit && digitCount < MinimumDigit)
        {
            errors.Add(PasswordErrorTypes.PasswordMinimumDigit(MinimumDigit));
        }

        return Task.FromResult(errors.Count == 0 ? PasswordValidationResult.Succeeded() : PasswordValidationResult.Failed(errors.ToArray()));
    }

    private static bool IsLetterOrDigit(char c)
    {
        return char.IsUpper(c) || char.IsLower(c) || char.IsDigit(c);
    }

    private static int CountNonAlphanumeric(string str)
    {
        var nonAlphanumericTotal = 0;
        foreach (char c in str)
        {
            if (!char.IsLetterOrDigit(c))
            {
                nonAlphanumericTotal += 1;
            }
        }

        return nonAlphanumericTotal;
    }

    private static int CountDigit(string input)
    {
        int numericCount = 0;

        foreach (char c in input)
        {
            if (char.IsDigit(c))
            {
                numericCount++;
            }
        }

        return numericCount;
    }


}