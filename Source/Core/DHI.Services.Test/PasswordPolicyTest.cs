namespace DHI.Services.Test
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class PasswordPolicyTest
    {
        [Fact]
        public async Task ValidateNullOrEmptyThrows()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => new PasswordPolicy().ValidateAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => new PasswordPolicy().ValidateAsync(""));
        }

        [Fact]
        public async Task ValidateIsOk()
        {
            var result = await new PasswordPolicy().ValidateAsync("/y4!wg%L[WEg@vPV");
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.Equal("Success", result.ToString());
        }

        [Fact]
        public async Task ValidateLengthFails()
        {
            var passwordPolicy = new PasswordPolicy();
            var result = await passwordPolicy.ValidateAsync("12345");
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(PasswordErrorTypes.PasswordIsTooShort(passwordPolicy.RequiredLength), result.Errors);
            Assert.Contains(nameof(PasswordErrorTypes.PasswordIsTooShort), result.ToString());
        }

        [Fact]
        public async Task ValidateUniqueCharsFails()
        {
            var result = await new PasswordPolicy {RequiredUniqueChars = 5}.ValidateAsync("1A1b1A");
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(PasswordErrorTypes.PasswordRequiresUniqueChars(5), result.Errors);
        }

        [Fact]
        public async Task ValidateNonAlphanumericFails()
        {
            var result = await new PasswordPolicy { RequiredUniqueChars = 5 }.ValidateAsync("1234Abcd");
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(PasswordErrorTypes.PasswordRequiresNonAlphanumeric(), result.Errors);
        }

        [Fact]
        public async Task ValidateLowercaseFails()
        {
            var result = await new PasswordPolicy().ValidateAsync("ABCDEFGH1&");
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(PasswordErrorTypes.PasswordRequiresLower(), result.Errors);
        }

        [Fact]
        public async Task ValidateUppercaseFails()
        {
            var result = await new PasswordPolicy().ValidateAsync("abcdefgh1&");
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(PasswordErrorTypes.PasswordRequiresUpper(), result.Errors);
        }

        [Fact]
        public async Task ValidateNoDigitFails()
        {
            var result = await new PasswordPolicy().ValidateAsync("abcdefg");
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(PasswordErrorTypes.PasswordRequiresDigit(), result.Errors);
        }

        [Fact]
        public async Task ValidateMinimumNonAlphanumericFails()
        {
            var result = await new PasswordPolicy { RequireNonAlphanumeric = true, MinimumNonAlphanumeric = 3 }.ValidateAsync("12Ab/@");
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(PasswordErrorTypes.PasswordMinimumNonAlphanumeric(3), result.Errors);
        }

        [Fact]
        public async Task ValidateWithoutRequiredMinimumNonAlphanumericsSucceeds()
        {
            var result = await new PasswordPolicy { RequireNonAlphanumeric = false, RequireDigit = false, RequireUppercase = false, RequireLowercase = false }.ValidateAsync("@><?/@@@!");
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.DoesNotContain(PasswordErrorTypes.PasswordMinimumNonAlphanumeric(3), result.Errors);
        }

        [Fact]
        public async Task ValidateMinimumDigitsFails()
        {
            var result = await new PasswordPolicy { RequireDigit = true, MinimumDigit = 3 }.ValidateAsync("12Ab/@");
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(PasswordErrorTypes.PasswordMinimumDigit(3), result.Errors);
        }

        [Fact]
        public async Task ValidateWithoutRequiredDigitsSucceds()
        {
            var result = await new PasswordPolicy { RequireDigit = false }.ValidateAsync("Abcdefg@");
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
            Assert.DoesNotContain(PasswordErrorTypes.PasswordMinimumDigit(3), result.Errors);
        }
    }
}
