namespace DHI.Services.Security.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using DHI.Services.Accounts;
    using DTOs;
    using Polly;
    using Xunit;

    [Collection("Controllers collection")]
    public class AccountsControllerTest
    {
        private readonly HttpClient _client;
        private readonly HttpClient _clientNonAdmin;
        private readonly JsonSerializerOptions _options;
        private readonly ControllersFixture _fixture = new ControllersFixture();

        public AccountsControllerTest(ControllersFixture fixture)
        {
            _client = fixture.Client;
            _clientNonAdmin = fixture.ClientNonAdmin;
            _options = new JsonSerializerOptions(fixture.SerializerOptions);
            _fixture = fixture;
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync("api/accounts/NonExistingAccountId");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = "/api/accounts",
                Body = new AccountDTO("NonExisting", "John Doe")
                {
                    Roles = "User, Editor",
                    Password = "/y4!wg%L[WEg@vPV"
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddExistingReturns400BadRequest()
        {
            _fixture.ResetValues(); // Reset values before the test

            var request = new
            {
                Url = "/api/accounts",
                Body = new AccountDTO("admin", "Administrator")
                {
                    Password = "/y4!wg%L[WEg@vPV",
                    Roles = "Administrator"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("already exists", json);
        }

        [Fact]
        public async Task AddWithPwnedPasswordReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/accounts",
                Body = new AccountDTO("john_doe", "John Doe")
                {
                    Password = "password",
                    Email = "john_doe@acme.com",
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The given password has been breached", json);
        }

        [Fact]
        public async Task AddWithInsecurePasswordReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/accounts",
                Body = new AccountDTO("john_doe", "John Doe")
                {
                    Password = "/y4!",
                    Email = "john_doe@acme.com",
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("PasswordIsTooShort", json);
        }

        [Fact]
        public async Task AddWithMissingPasswordReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/accounts",
                Body = new AccountDTO("john_doe", "John Doe")
                {
                    Email = "john_doe@acme.com",
                    Roles = "Editor"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The Password field is required", json);
        }

        [Fact]
        public async Task UpdateWithPwnedPasswordReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/accounts",
                Body = new AccountUpdateDTO("john_doe", "John Doe")
                {
                    Password = "password",
                    Email = "john_doe@acme.com",
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The given password has been breached", json);
        }

        [Fact]
        public async Task UpdateWithInsecurePasswordReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/accounts",
                Body = new AccountUpdateDTO("john_doe", "John Doe")
                {
                    Password = "/y4!",
                    Email = "john_doe@acme.com",
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("PasswordIsTooShort", json);
        }


        [Fact]
        public async Task UpdateMeWithPwnedPasswordReturns400BadRequest()
        {
            var response = await _client.GetAsync("api/accounts/me");
            var json = await response.Content.ReadAsStringAsync();
            var me = JsonSerializer.Deserialize<AccountDTO>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("admin", me.Id);

            var request = new
            {
                Url = "/api/accounts/me",
                Body = new AccountDTO(me.Id, me.Name)
                {
                    Email = me.Email,
                    Password = "password"
                }
            };

            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The given password has been breached", json);
        }

        [Fact]
        public async Task UpdateMeWithInsecurePasswordReturns400BadRequest()
        {
            var response = await _client.GetAsync("api/accounts/me");
            var json = await response.Content.ReadAsStringAsync();
            var me = JsonSerializer.Deserialize<AccountDTO>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("admin", me.Id);

            var request = new
            {
                Url = "/api/accounts/me",
                Body = new AccountDTO(me.Id, me.Name)
                {
                    Email = me.Email,
                    Password = "/y4!"
                }
            };

            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("PasswordIsTooShort", json);
        }

        [Fact]
        public async Task UpdateMeForOtherUserReturns403Forbidden()
        {
            var request = new
            {
                Url = "/api/accounts/me",
                Body = new AccountDTO("john_doe", "John Doe")
                {
                    Email = "john_doe@acme.com",
                    Roles = "Editor",
                    Password = "/y4!wg%L[WEg@vPV"
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Contains("Cannot update the account of", json);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync("api/accounts/NonExistingAccountId");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync("api/accounts/editor");
            var json = await response.Content.ReadAsStringAsync();
            _options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            var account = JsonSerializer.Deserialize<AccountDTO>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("editor", account.Id);
            Assert.Contains("Editors", account.UserGroups);
        }

        [Fact]
        public async Task GetPasswordPolicyIsOk()
        {
            var response = await _client.GetAsync("api/accounts/passwordpolicy");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var policy = JsonSerializer.Deserialize<PasswordPolicy>(json, _options);

            Assert.NotNull(policy);
            Assert.False(policy.ValidateAsync("password").Result.Success);
            Assert.Equal(10, policy.RequiredLength);
            Assert.Equal(5, policy.RequiredUniqueChars);
            Assert.True(policy.RequireDigit);
            Assert.Equal(1, policy.MinimumDigit);
            Assert.True(policy.RequireNonAlphanumeric);
            Assert.Equal(1, policy.MinimumNonAlphanumeric);
        }

        [Fact]
        public async Task GetMeIsOk()
        {
            var response = await _client.GetAsync("api/accounts/me");
            var json = await response.Content.ReadAsStringAsync();
            var account = JsonSerializer.Deserialize<AccountDTO>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("admin", account.Id);
            Assert.Contains("Administrators", account.UserGroups);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync("api/accounts");
            var json = await response.Content.ReadAsStringAsync();
            var accounts = JsonSerializer.Deserialize<IEnumerable<AccountDTO>>(json, _options).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(accounts.Any());
            Assert.Contains(accounts, account => account.Id == "admin");
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            _fixture.ResetValues(); // Reset values before the test

            var response = await _client.GetAsync("api/accounts/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(4, count);
        }

        [Fact]
        public async Task AddUpdateAndDeleteIsOk()
        {
            var request = new
            {
                Url = "/api/accounts",
                Body = new AccountDTO("john_doe", "John Doe")
                {
                    Email = "john_doe@acme.com",
                    Company = "Acme",
                    Password = "/y4!wg%L[WEg@vPV",
                    UserGroups = new List<string>
                    {
                        "Editors",
                        "Users"
                    }
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var account = JsonSerializer.Deserialize<AccountDTO>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("http://localhost/api/accounts/john_doe", response.Headers.Location?.ToString());
            Assert.Equal(request.Body.Id, account.Id);
            Assert.Contains("Editors", account.UserGroups);
            Assert.Contains("Users", account.UserGroups);
            Assert.DoesNotContain("Administrators", account.UserGroups);
            Assert.Equal(2, account.UserGroups.Count());


            // Update
            request.Body.Company = "Acme Inc.";
            request.Body.UserGroups = new[] { "Editors" };
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            account = JsonSerializer.Deserialize<AccountDTO>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Acme Inc.", account.Company);
            Assert.Contains("Editors", account.UserGroups);
            Assert.DoesNotContain("Users", account.UserGroups);
            Assert.DoesNotContain("Administrators", account.UserGroups);
            Assert.Single(account.UserGroups);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{account.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{account.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateMeIsOk()
        {
            var response = await _client.GetAsync("api/accounts/me");
            var json = await response.Content.ReadAsStringAsync();
            var me = JsonSerializer.Deserialize<AccountDTO>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("admin", me.Id);

            var request = new
            {
                Url = "/api/accounts/me",
                Body = new AccountDTO(me.Id, me.Name)
                {
                    Email = me.Email,
                    Roles = me.Roles,
                    Company = "DHI",
                    UserGroups = new List<string>
                    {
                        "Administrators",
                        "Editors",
                        "Users"
                    }
                }
            };

            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = JsonSerializer.Deserialize<AccountDTO>(json, _options);
            Assert.Equal(request.Body.Company, updated.Company);
            Assert.DoesNotContain("Guests", updated.UserGroups);
            Assert.Equal(3, updated.UserGroups.Count());
        }

        [Fact]
        public async Task UpdateMeWithAccountPrivilegeToAdminReturns403Forbidden()
        {
            var response = await _clientNonAdmin.GetAsync("api/accounts/me");
            var json = await response.Content.ReadAsStringAsync();
            var me = JsonSerializer.Deserialize<AccountDTO>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("user", me.Id);

            var request = new
            {
                Url = "/api/accounts/me",
                Body = new AccountDTO(me.Id, me.Name)
                {
                    Email = me.Email,
                    Roles = me.Roles,
                    Company = "DHI",
                    UserGroups = new List<string>
                    {
                        "Guests",
                        "Users",
                        "Administrators"
                    }
                }
            };

            response = await _clientNonAdmin.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("Cannot add Administrators privilage for the account of", json);
        }

        [Fact]
        public async Task DeleteRemovesAccountFromUserGroups()
        {
            const string userId = "john.doe";
            var request = new
            {
                Url = "/api/accounts",
                Body = new AccountDTO(userId, "John Doe")
                {
                    Email = "john.doe@acme.com",
                    Password = "/y4!wg%L[WEg@vPV"
                }
            };

            // Add account
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var account = JsonSerializer.Deserialize<AccountDTO>(json, _options);

            Assert.Equal($"http://localhost/api/accounts/{userId}", response.Headers.Location?.ToString());
            Assert.Equal(request.Body.Id, account.Id);

            // Add to group(s)
            var addToGroupRequest = new
            {
                Url = $"/api/usergroups/user/{userId}",
                Body = new[] { "Administrators", "Editors" }
            };

            response = await _client.PostAsync(addToGroupRequest.Url, ContentHelper.GetStringContent(addToGroupRequest.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync($"api/usergroups/ids?userId={userId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<IEnumerable<string>>(json).ToArray();
            Assert.Contains("Administrators", ids);
            Assert.Contains("Editors", ids);
            Assert.DoesNotContain("Users", ids);
            Assert.DoesNotContain("Guests", ids);

            // Delete user
            response = await _client.DeleteAsync($"{request.Url}/{account.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{account.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // Assert user is removed from all groups
            response = await _client.GetAsync($"api/usergroups/ids?userId={userId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            json = await response.Content.ReadAsStringAsync();
            ids = JsonSerializer.Deserialize<IEnumerable<string>>(json).ToArray();
            Assert.Empty(ids);
        }

        [Fact]
        public async Task AddUpdateWithPasswordValidationReturnBadRequest()
        {
            var request = new
            {
                Url = "/api/accounts",
                Body = new AccountDTO("john_doe", "John Doe")
                {
                    Email = "john_doe@acme.com",
                    Company = "Acme",
                    Password = "/y4!wg%L[WEg@vPV",
                    UserGroups = new List<string>
                    {
                        "Editors",
                        "Users"
                    }
                }
            };

            // Add account
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            // Update account
            request.Body.Password = "ywgLWEgvPV";
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEnabledAccountStatusByAdministrator()
        {

            var request = new
            {
                Url = "/api/accounts",
                Body = new AccountDTO("john_doe", "John Doe")
                {
                    Email = "john_doe@acme.com",
                    Company = "Acme",
                    Password = "/y4!wg%L[WEg@vPV",
                    UserGroups = new List<string>
                    {
                        "Editors",
                        "Users"
                    }
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var account = JsonSerializer.Deserialize<AccountDTO>(json, _options);

            // Update
            request.Body.Enabled = false;
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            account = JsonSerializer.Deserialize<AccountDTO>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(account.Enabled);
        }

        /// <summary>
        ///     Performs an account locking by Administrator.
        /// </summary>
        /// <remarks>
        ///     Checks the Administrator Account can update other Account lock or unlock status
        /// </remarks>
        [Fact]
        public async Task UpdateAccountLockStatusByAdministrator()
        {
            // Arrange
            var request = new
            {
                Url = "/api/accounts",
                Body = new AccountDTO("marco_poloe", "Marco Poloe")
                {
                    Email = "marco_poloe@acme.com",
                    Company = "Acme",
                    Password = "/y4!wg%L[WEg@vPV",
                    NoOfUnsuccessfulLoginAttempts = 3,
                    LastLoginAttemptedDate = DateTime.Now,
                    Locked = true,
                    LockedDateEnd = Convert.ToDateTime("10/10/2023").AddDays(30),
                    UserGroups = new List<string>
                    {
                        "Editors",
                        "Users"
                    }
                }
            };

            // Act
            /// Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var account = JsonSerializer.Deserialize<AccountDTO>(json, _options);
            /// Update
            request.Body.NoOfUnsuccessfulLoginAttempts = 0;
            request.Body.Locked = false;
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            account = JsonSerializer.Deserialize<AccountDTO>(json, _options);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, account.NoOfUnsuccessfulLoginAttempts);
            Assert.False(account.Locked);
        }

        /// <summary>
        ///     Performs an account locking by Non Administrator.
        /// </summary>
        /// <remarks>
        ///     Checks the Non Administrator can update other Account lock or unlock status
        /// </remarks>
        [Fact]
        public async Task UpdateAccountLockStatusByNonAdministrator()
        {
            // Arrange
            var request = new
            {
                Url = "/api/accounts",
                Body = new AccountDTO("marco_poloe", "Marco Poloe")
                {
                    Email = "marco_poloe@acme.com",
                    Company = "Acme",
                    Password = "/y4!wg%L[WEg@vPV",
                    NoOfUnsuccessfulLoginAttempts = 3,
                    LastLoginAttemptedDate = DateTime.Now,
                    Locked = true,
                    LockedDateEnd = DateTime.Now,
                    UserGroups = new List<string>
                    {
                        "Editors",
                        "Users"
                    }
                }
            };

            // Act
            /// Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var account = JsonSerializer.Deserialize<AccountDTO>(json, _options);
            /// Update
            request.Body.NoOfUnsuccessfulLoginAttempts = 0;
            request.Body.Locked = false;
            response = await _clientNonAdmin.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetLoginAttemptPolicyIsOk()
        {
            // Arrange
            var response = await _client.GetAsync("api/accounts/loginattemptpolicy");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Act
            var json = await response.Content.ReadAsStringAsync();
            var loginattemptpolicy = JsonSerializer.Deserialize<LoginAttemptPolicyDTO>(json, _options);

            // Assert
            Assert.NotNull(loginattemptpolicy);
            Assert.Equal(5, loginattemptpolicy.MaxNumberOfLoginAttempts);
            Assert.Equal(DateTime.Now.AddDays(10).ToString("yyyy-MM-dd"), DateTime.Now.AddDays(loginattemptpolicy.LockedPeriod.TotalDays).ToString("yyyy-MM-dd"));
        }
    }
}