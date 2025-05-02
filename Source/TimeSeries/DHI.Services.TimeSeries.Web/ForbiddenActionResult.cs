namespace DHI.Services.TimeSeries.Web
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Task = System.Threading.Tasks.Task;

    internal class ForbiddenActionResult : IHttpActionResult
    {
        private readonly string _reason;
        private readonly HttpRequestMessage _request;

        public ForbiddenActionResult(HttpRequestMessage request, string reason)
        {
            _request = request;
            _reason = reason;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = _request.CreateResponse(HttpStatusCode.Forbidden, _reason);
            return Task.FromResult(response);
        }
    }
}