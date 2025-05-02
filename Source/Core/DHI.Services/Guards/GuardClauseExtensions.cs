#pragma warning disable IDE0060 // Remove unused parameter

namespace DHI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     A collection of common guard clauses, implemented as extensions.
    /// </summary>
    /// <example>
    ///     Guard.Against.Null(input, nameof(input));
    /// </example>
    public static class GuardClauseExtensions
    {
        /// <summary>
        ///     Throws an <see cref="ArgumentNullException" /> if <paramref name="input" /> is null.
        /// </summary>
        /// <param name="guardClause">The guard clause.</param>
        /// <param name="input">The input.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Null(this IGuardClause guardClause, [ValidatedNotNull] object input, string parameterName)
        {
            if (input is null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentNullException" /> if <paramref name="input" /> is null.
        /// </summary>
        /// <param name="guardClause">The guard clause.</param>
        /// <param name="input">The input.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns><paramref name="input" /> if the value is not null.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T Null<T>(this IGuardClause guardClause, [ValidatedNotNull] T input, string parameterName)
        {
            if (input is null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return input;
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentNullException" /> if <paramref name="input" /> is null.
        ///     Throws an <see cref="ArgumentException" /> if <paramref name="input" /> is an empty string.
        /// </summary>
        /// <param name="guardClause">The guard clause.</param>
        /// <param name="input">The input.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void NullOrEmpty(this IGuardClause guardClause, [ValidatedNotNull] string input, string parameterName)
        {
            Guard.Against.Null(input, parameterName);
            if (input.Length == 0)
            {
                throw new ArgumentException($"Required input `{parameterName}` is empty.", parameterName);
            }
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentNullException" /> if <paramref name="input" /> is null.
        ///     Throws an <see cref="ArgumentException" /> if <paramref name="input" /> is an empty enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="guardClause">The guard clause.</param>
        /// <param name="input">The input.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void NullOrEmpty<T>(this IGuardClause guardClause, [ValidatedNotNull] IEnumerable<T> input, string parameterName)

        {
            Guard.Against.Null(input, parameterName);
            if (!input.Any())
            {
                throw new ArgumentException($"Required input `{parameterName}` is empty.", parameterName);
            }
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentNullException" /> if <paramref name="input" /> is null.
        ///     Throws an <see cref="ArgumentException" /> if <paramref name="input" /> is an empty or white space string.
        /// </summary>
        /// <param name="guardClause">The guard clause.</param>
        /// <param name="input">The input.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void NullOrWhiteSpace(this IGuardClause guardClause, [ValidatedNotNull] string input, string parameterName)
        {
            Guard.Against.NullOrEmpty(input, parameterName);
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException($"Required input `{parameterName}` consists only of white-space characters.", parameterName);
            }
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentNullException" /> if <paramref name="input" /> is null.
        ///     Throws an <see cref="ArgumentException" /> if <paramref name="input" /> is an empty string or contains any spaces.
        /// </summary>
        /// <param name="guardClause">The guard clause.</param>
        /// <param name="input">The input.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void NullOrAnySpace(this IGuardClause guardClause, [ValidatedNotNull] string input, string parameterName)
        {
            Guard.Against.NullOrEmpty(input, parameterName);
            if (input.Contains(" "))
            {
                throw new ArgumentException($"Required input `{parameterName}` contains spaces.", parameterName);
            }
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentException" /> if <paramref name="input" /> is negative or zero.
        /// </summary>
        /// <param name="guardClause">The guard clause.</param>
        /// <param name="input">The input.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns><paramref name="input" /> if the value is not negative or zero.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static int NegativeOrZero(this IGuardClause guardClause, int input, string parameterName)
        {
            return NegativeOrZero<int>(guardClause, input, parameterName);
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentException" /> if <paramref name="input" /> is negative or zero.
        /// </summary>
        /// <param name="guardClause">The guard clause.</param>
        /// <param name="input">The input.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns><paramref name="input" /> if the value is not negative or zero.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static float NegativeOrZero(this IGuardClause guardClause, float input, string parameterName)
        {
            return NegativeOrZero<float>(guardClause, input, parameterName);
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentException" /> if <paramref name="input" /> is negative or zero.
        /// </summary>
        /// <param name="guardClause">The guard clause.</param>
        /// <param name="input">The input.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns><paramref name="input" /> if the value is not negative or zero.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static double NegativeOrZero(this IGuardClause guardClause, double input, string parameterName)
        {
            return NegativeOrZero<double>(guardClause, input, parameterName);
        }

        /// <summary>
        ///     Throws an <see cref="ArgumentException" /> if <paramref name="input" /> is negative or zero.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="guardClause">The guard clause.</param>
        /// <param name="input">The input.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns><paramref name="input" /> if the value is not negative or zero.</returns>
        /// <exception cref="ArgumentException"></exception>
        private static T NegativeOrZero<T>(this IGuardClause guardClause, T input, string parameterName) where T : struct, IComparable
        {
            if (input.CompareTo(default(T)) <= 0)
            {
                throw new ArgumentException($"Required input {parameterName} cannot be zero or negative.", parameterName);
            }

            return input;
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter