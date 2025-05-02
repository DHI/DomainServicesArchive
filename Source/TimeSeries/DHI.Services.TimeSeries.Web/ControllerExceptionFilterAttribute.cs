namespace DHI.Services.TimeSeries.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Filters;

    /// <summary>
    ///     Class ControllerExceptionFilterAttribute.
    /// </summary>
    /// <seealso cref="System.Web.Http.Filters.ExceptionFilterAttribute" />
    public class ControllerExceptionFilterAttribute : ExceptionFilterAttribute
    {
        /// <summary>
        ///     Raises the exception event.
        /// </summary>
        /// <param name="actionExecutedContext">The context for the action.</param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var e = actionExecutedContext.Exception;
            if (e is KeyNotFoundException || e is ArgumentOutOfRangeException)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, e);
            }
            else if (e is ArgumentException)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, e);
            }
            else
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}