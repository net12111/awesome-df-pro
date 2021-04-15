﻿// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff.Tests.TestFramework;
using Duende.Bff.Tests.TestHosts;
using FluentAssertions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Duende.Bff.Tests.Endpoints
{
    public class RemoteEndpointTests : BffIntegrationTestBase
    {
        [Fact]
        public async Task unauthenticated_calls_to_remote_endpoint_should_return_401()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user/test"));
            req.Headers.Add("x-csrf", "1");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task calls_to_remote_endpoint_should_forward_user_to_api()
        {
            await BffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user/test"));
            req.Headers.Add("x-csrf", "1");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.Method.Should().Be("GET");
            apiResult.Path.Should().Be("/test");
            apiResult.Sub.Should().Be("alice");
            apiResult.ClientId.Should().Be("spa");
        }

        [Fact]
        public async Task put_to_remote_endpoint_should_forward_user_to_api()
        {
            await BffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Put, BffHost.Url("/api_user/test"));
            req.Headers.Add("x-csrf", "1");
            req.Content = new StringContent(JsonSerializer.Serialize(new TestPayload("hello test api")), Encoding.UTF8, "application/json");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.Method.Should().Be("PUT");
            apiResult.Path.Should().Be("/test");
            apiResult.Sub.Should().Be("alice");
            apiResult.ClientId.Should().Be("spa");
            var body = JsonSerializer.Deserialize<TestPayload>(apiResult.Body);
            body.message.Should().Be("hello test api");
        }
        
        [Fact]
        public async Task post_to_remote_endpoint_should_forward_user_to_api()
        {
            await BffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Post, BffHost.Url("/api_user/test"));
            req.Headers.Add("x-csrf", "1");
            req.Content = new StringContent(JsonSerializer.Serialize(new TestPayload("hello test api")), Encoding.UTF8, "application/json");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.Method.Should().Be("POST");
            apiResult.Path.Should().Be("/test");
            apiResult.Sub.Should().Be("alice");
            apiResult.ClientId.Should().Be("spa");
            var body = JsonSerializer.Deserialize<TestPayload>(apiResult.Body);
            body.message.Should().Be("hello test api");
        }



        [Fact]
        public async Task calls_to_remote_endpoint_should_forward_user_or_anonymous_to_api()
        {
            {
                var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_or_anon/test"));
                req.Headers.Add("x-csrf", "1");
                var response = await BffHost.BrowserClient.SendAsync(req);

                response.IsSuccessStatusCode.Should().BeTrue();
                response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
                var json = await response.Content.ReadAsStringAsync();
                var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
                apiResult.Method.Should().Be("GET");
                apiResult.Path.Should().Be("/test");
                apiResult.Sub.Should().BeNull();
                apiResult.ClientId.Should().BeNull();
            }

            {
                await BffHost.BffLoginAsync("alice");

                var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_or_anon/test"));
                req.Headers.Add("x-csrf", "1");
                var response = await BffHost.BrowserClient.SendAsync(req);

                response.IsSuccessStatusCode.Should().BeTrue();
                response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
                var json = await response.Content.ReadAsStringAsync();
                var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
                apiResult.Method.Should().Be("GET");
                apiResult.Path.Should().Be("/test");
                apiResult.Sub.Should().Be("alice");
                apiResult.ClientId.Should().Be("spa");
            }
        }

        [Fact]
        public async Task calls_to_remote_endpoint_should_forward_client_token_to_api()
        {
            await BffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_client/test"));
            req.Headers.Add("x-csrf", "1");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.Method.Should().Be("GET");
            apiResult.Path.Should().Be("/test");
            apiResult.Sub.Should().BeNull();
            apiResult.ClientId.Should().Be("spa");
        }

        [Fact]
        public async Task calls_to_remote_endpoint_should_forward_user_or_client_to_api()
        {
            {
                var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_or_client/test"));
                req.Headers.Add("x-csrf", "1");
                var response = await BffHost.BrowserClient.SendAsync(req);

                response.IsSuccessStatusCode.Should().BeTrue();
                response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
                var json = await response.Content.ReadAsStringAsync();
                var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
                apiResult.Method.Should().Be("GET");
                apiResult.Path.Should().Be("/test");
                apiResult.Sub.Should().BeNull();
                apiResult.ClientId.Should().Be("spa");
            }

            {
                await BffHost.BffLoginAsync("alice");

                var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_or_client/test"));
                req.Headers.Add("x-csrf", "1");
                var response = await BffHost.BrowserClient.SendAsync(req);

                response.IsSuccessStatusCode.Should().BeTrue();
                response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
                var json = await response.Content.ReadAsStringAsync();
                var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
                apiResult.Method.Should().Be("GET");
                apiResult.Path.Should().Be("/test");
                apiResult.Sub.Should().Be("alice");
                apiResult.ClientId.Should().Be("spa");
            }
        }

        [Fact]
        public async Task calls_to_remote_endpoint_with_anon_should_be_anon()
        {
            {
                var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
                req.Headers.Add("x-csrf", "1");
                var response = await BffHost.BrowserClient.SendAsync(req);

                response.IsSuccessStatusCode.Should().BeTrue();
                response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
                var json = await response.Content.ReadAsStringAsync();
                var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
                apiResult.Method.Should().Be("GET");
                apiResult.Path.Should().Be("/test");
                apiResult.Sub.Should().BeNull();
                apiResult.ClientId.Should().BeNull();
            }

            {
                await BffHost.BffLoginAsync("alice");

                var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
                req.Headers.Add("x-csrf", "1");
                var response = await BffHost.BrowserClient.SendAsync(req);

                response.IsSuccessStatusCode.Should().BeTrue();
                response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
                var json = await response.Content.ReadAsStringAsync();
                var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
                apiResult.Method.Should().Be("GET");
                apiResult.Path.Should().Be("/test");
                apiResult.Sub.Should().BeNull();
                apiResult.ClientId.Should().BeNull();
            }
        }


        [Fact]
        public async Task calls_to_remote_endpoint_expecting_token_but_without_token_should_fail()
        {
            var client = IdentityServerHost.Clients.Single(x => x.ClientId == "spa");
            client.Enabled = false;

            {
                var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_or_client/test"));
                req.Headers.Add("x-csrf", "1");
                var response = await BffHost.BrowserClient.SendAsync(req);

                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            {
                var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_client/test"));
                req.Headers.Add("x-csrf", "1");
                var response = await BffHost.BrowserClient.SendAsync(req);

                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }


        [Fact]
        public async Task response_status_401_from_remote_endpoint_should_return_401_from_bff()
        {
            await BffHost.BffLoginAsync("alice");
            ApiHost.ApiStatusCodeToReturn = 401;

            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user/test"));
            req.Headers.Add("x-csrf", "1");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task response_status_403_from_remote_endpoint_should_return_403_from_bff()
        {
            await BffHost.BffLoginAsync("alice");
            ApiHost.ApiStatusCodeToReturn = 403;

            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user/test"));
            req.Headers.Add("x-csrf", "1");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }



        [Fact]
        public async Task calls_to_remote_endpoint_should_require_csrf()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_or_client/test"));
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task endpoints_that_disable_csrf_should_not_require_csrf_header()
        {
            await BffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_no_csrf/test"));
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.Method.Should().Be("GET");
            apiResult.Path.Should().Be("/test");
            apiResult.Sub.Should().Be("alice");
            apiResult.ClientId.Should().Be("spa");
        }

        [Fact]
        public void calls_to_endpoint_without_bff_metadata_should_fail()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/not_bff_endpoint"));

            Func<Task> f = () => BffHost.BrowserClient.SendAsync(req);
            f.Should().Throw<Exception>();
        }
        
        [Fact]
        public void calls_to_bff_not_in_endpoint_routing_should_fail()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/invalid_endpoint/test"));

            Func<Task> f = () => BffHost.BrowserClient.SendAsync(req);
            f.Should().Throw<Exception>();
        }
    }
}
