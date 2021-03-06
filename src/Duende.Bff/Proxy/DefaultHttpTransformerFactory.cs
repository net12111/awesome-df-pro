// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Linq;
using Microsoft.Net.Http.Headers;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Duende.Bff
{
    /// <summary>
    /// Default HTTP transformer implementation
    /// </summary>
    public class DefaultHttpTransformerFactory : IHttpTransformerFactory
    {
        /// <summary>
        /// The options
        /// </summary>
        protected readonly BffOptions Options;

        /// <summary>
        /// The YARP transform builder
        /// </summary>
        protected readonly ITransformBuilder TransformBuilder;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="options">The BFF options</param>
        /// <param name="transformBuilder">The YARP transform builder</param>
        public DefaultHttpTransformerFactory(BffOptions options, ITransformBuilder transformBuilder)
        {
            Options = options;
            TransformBuilder = transformBuilder;
        }
        
        /// <inheritdoc />
        public virtual HttpTransformer CreateTransformer(string localPath, string accessToken = null)
        {
            return TransformBuilder.Create(context =>
            {
                context.CopyRequestHeaders = false;

                // todo: should x-forwarded be added by default?
                context.UseDefaultForwarders = false;

                context.RequestTransforms.Add(new PathStringTransform(Options.PathTransformMode, localPath));
                context.RequestTransforms.Add(new ForwardHeadersRequestTransform(new[]
                    { HeaderNames.Accept, HeaderNames.ContentLength, HeaderNames.ContentType }));

                if (Options.ForwardedHeaders.Any())
                {
                    context.RequestTransforms.Add(new ForwardHeadersRequestTransform(Options.ForwardedHeaders));
                }

                if (Options.AddXForwardedHeaders)
                {
                    var action = ForwardedTransformActions.Set;
                    
                    if (Options.ForwardIncomingXForwardedHeaders)
                    {
                        action = ForwardedTransformActions.Append;
                    }
                    
                    context.AddXForwarded(action);
                }

                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    context.RequestTransforms.Add(new AccessTokenRequestTransform(accessToken));
                }
            });
        }
    }
}