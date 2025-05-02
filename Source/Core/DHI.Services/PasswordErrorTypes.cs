namespace DHI.Services;

/// <summary>
///     Password validation error descriptor
/// </summary>
public record struct PasswordError(string Code, string Description);

/// <summary>
///     Password validation error types
/// </summary>
public static class PasswordErrorTypes
{
    /// <summary>
    ///     Password is too short.
    /// </summary>
    /// <param name="minLength">The required minimum length.</param>
    public static PasswordError PasswordIsTooShort(int minLength)
    {
        return new PasswordError(nameof(PasswordIsTooShort), $"Passwords must be at least {minLength} characters.");
    }

    /// <summary>
    ///     Passwords requires non alphanumeric character.
    /// </summary>
    public static PasswordError PasswordRequiresNonAlphanumeric()
    {
        return new PasswordError(nameof(PasswordRequiresNonAlphanumeric), "Passwords must have at least one non alphanumeric character.");
    }

    /// <summary>
    ///     Passwords requires unique characters.
    /// </summary>
    /// <param name="minUniqueChars">The minimum number of unique characters.</param>
    public static PasswordError PasswordRequiresUniqueChars(int minUniqueChars)
    {
        return new PasswordError(nameof(PasswordRequiresUniqueChars), $"Passwords must use at least {minUniqueChars} different characters.");
    }

    /// <summary>
    ///     Password requires digit.
    /// </summary>
    public static PasswordError PasswordRequiresDigit()
    {
        return new PasswordError(nameof(PasswordRequiresDigit), "Passwords must have at least one digit ('0'-'9').");
    }

    /// <summary>
    ///     Password requires lower case letter.
    /// </summary>
    public static PasswordError PasswordRequiresLower()
    {
        return new PasswordError(nameof(PasswordRequiresLower), "Passwords must have at least one lowercase ('a'-'z') letter.");
    }

    /// <summary>
    ///     Password requires upper case letter.
    /// </summary>
    public static PasswordError PasswordRequiresUpper()
    {
        return new PasswordError(nameof(PasswordRequiresUpper), "Passwords must have at least one uppercase ('A'-'Z') letter.");
    }

    /// <summary>
    ///     Passwords requires NonAlphanumeric characters.
    /// </summary>
    /// <param name="minNonAlphanumericLength">The minimum number of NonAlphanumeric characters.</param>
    public static PasswordError PasswordMinimumNonAlphanumeric(int minNonAlphanumericLength)
    {
        return new PasswordError(nameof(PasswordMinimumNonAlphanumeric), $"Passwords must be at least {minNonAlphanumericLength} NonAlphanumeric characters.");
    }

    /// <summary>
    ///     Passwords requires Digits.
    /// </summary>
    /// <param name="minDigitLength">The minimum number of Digits.</param>
    public static PasswordError PasswordMinimumDigit(int minDigitLength)
    {
        return new PasswordError(nameof(PasswordMinimumDigit), $"Passwords must be at least {minDigitLength} Digits.");
    }
}