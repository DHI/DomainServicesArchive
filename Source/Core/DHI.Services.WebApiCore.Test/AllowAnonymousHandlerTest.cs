namespace DHI.Services.WebApiCore.Test
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Infrastructure;
    using Xunit;

    public class AllowAnonymousHandlerTest
    {

        [Fact]
        public void ImplementsIAuthenticationHandler()
        {
            Assert.IsAssignableFrom<IAuthorizationHandler>(new AllowAnonymousHandler());
        }

        [Fact]
        public async void GetIsOk()
        {
            var requirement = new NameAuthorizationRequirement("name");
            var context = new AuthorizationHandlerContext(new[] {requirement}, new ClaimsPrincipal(), null);
            var handler = new AllowAnonymousHandler();
            await handler.HandleAsync(context);
            Assert.True(context.HasSucceeded);
        }
    }
}
