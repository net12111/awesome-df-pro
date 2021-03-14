using System.Net.Http;

namespace Duende.Bff
{
    /// <summary>
    /// Factory for creating message invokes for outgoing remote BFF API calls
    /// </summary>
    public interface IHttpMessageInvokerFactory
    {
        /// <summary>
        /// Creates a message invoker based on the local path
        /// </summary>
        /// <param name="localPath">Local path the remote API is mapped to</param>
        /// <returns></returns>
        HttpMessageInvoker CreateClient(string localPath);
    }
}