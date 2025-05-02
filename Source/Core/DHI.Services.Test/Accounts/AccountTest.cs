namespace DHI.Services.Test
{
    using System;
    using Accounts;
    using AutoFixture.Xunit2;
    using Xunit;

    public class AccountTest
    {
        [Fact]
        public void CreateWithNullIdThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Account(null, "John Doe"));
        }

        [Fact]
        public void CreateWithNullNameThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Account("John.doe", null));
        }

        [Theory, AutoData]
        public void SetPasswordNullThrows(Account account)
        {
            Assert.Throws<ArgumentNullException>(() => account.SetPassword(null));
        }

        [Theory, AutoData]
        public void SetPasswordEmptyThrows(Account account)
        {
            Assert.Throws<ArgumentException>(() => account.SetPassword(""));
        }

        [Theory]
        [InlineData("CorrectPassword")]
        [Obsolete]
        public void ValidatePasswordIsOk(string password)
        {
            var user = new Account("john.doe", "John Doe") {EncryptedPassword = Account.HashPassword(password)};
            Assert.True(user.ValidatePassword(password));
            Assert.False(user.ValidatePassword("WrongPassword"));
        }

        [Fact]
        [Obsolete]
        public void GetRolesReturnsEmptyIfNoRoles()
        {
            var account = new Account("user", "User");

            Assert.Empty(account.GetRoles());
        }

        [Theory]
        [InlineData("Guest, User, Editor", new[] {"Guest", "User", "Editor"})]
        [InlineData("Administrator", new[] {"Administrator"})]
        [InlineData("Guest, Editor", new[] {"Guest", "Editor"})]
        [Obsolete]
        public void GetRolesIsOk(string roleString, string[] roles)
        {
            var account = new Account("user", "User");
            account.SetPassword("password");
            account.Roles = roleString;

            Assert.Equal(roles, account.GetRoles());
        }

        [Fact]
        public void AllowMePasswordChangeDefaultIsTrue()
        {
            var account = new Account("john.doe", "John Doe");
            Assert.True(account.AllowMePasswordChange);
        }
    }
}