namespace DHI.Services.Authentication
{
    using System;
    using System.Threading.Tasks;

    [Obsolete("Will be removed in a future version.")]
    public interface IAccessTokenProvider
    {
        Task<string> GetAccessToken();
    }
}