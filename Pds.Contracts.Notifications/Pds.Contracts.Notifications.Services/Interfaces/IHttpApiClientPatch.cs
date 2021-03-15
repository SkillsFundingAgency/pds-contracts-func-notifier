using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces
{
    /// <summary>
    /// Interface to allow extensions of the Base API Client to allow HTTP PATCH method.
    /// </summary>
    public interface IHttpApiClientPatch
    {
        /// <summary>
        /// Performs a HTTP PATCH request to given URI, with the payload of type <typeparamref name="TRequest"/>.
        /// </summary>
        /// <typeparam name="TRequest">The payload type of this request.</typeparam>
        /// <param name="requestUri">The request URI to call.</param>
        /// <param name="requestBody">The payload of this request.</param>
        /// <param name="setAccessTokenAction">The delegate to set the access token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Patch<TRequest>(string requestUri, TRequest requestBody, Func<Task> setAccessTokenAction = null);

        /// <summary>
        /// Performs an authenticated HTTP PATCH request to the given URI, with a payload of type <typeparamref name="TRequest"></typeparamref>.
        /// </summary>
        /// <typeparam name="TRequest">The payload type of this request.</typeparam>
        /// <param name="requestUri">The request URI to call.</param>
        /// <param name="requestBody">The payload of this request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task PatchWithAADAuth<TRequest>(string requestUri, TRequest requestBody);
    }
}