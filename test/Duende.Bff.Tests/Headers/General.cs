using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Duende.Bff.Tests.TestFramework;
using Duende.Bff.Tests.TestHosts;
using FluentAssertions;
using Xunit;

namespace Duende.Bff.Tests.Headers
{
    public class General : BffIntegrationTestBase
    {
        [Fact]
        public async Task local_endpoint_should_receive_standard_headers()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/local_anon"));
            req.Headers.Add("x-csrf", "1");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);

            apiResult.RequestHeaders.Count.Should().Be(2);
            apiResult.RequestHeaders["Host"].Single().Should().Be("app");
            apiResult.RequestHeaders["x-csrf"].Single().Should().Be("1");
        }
        
        [Fact]
        public async Task custom_header_should_not_be_forwarded_by_default()
        {
            BffHost.BffOptions.AddXForwardedHeaders = false;
            
            await BffHost.InitializeAsync();

            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
            req.Headers.Add("x-csrf", "1");
            req.Headers.Add("x-custom", "custom");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);

            apiResult.RequestHeaders.Count.Should().Be(1);
            
            apiResult.RequestHeaders["Host"].Single().Should().Be("api");
        }
        
        [Fact]
        public async Task custom_header_should_be_forwarded_when_configured()
        {
            BffHost.BffOptions.AddXForwardedHeaders = false;
            BffHost.BffOptions.ForwardedHeaders.Add("x-custom");
            
            await BffHost.InitializeAsync();

            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
            req.Headers.Add("x-csrf", "1");
            req.Headers.Add("x-custom", "custom");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);

            apiResult.RequestHeaders.Count.Should().Be(2);
            
            apiResult.RequestHeaders["Host"].Single().Should().Be("api");
            apiResult.RequestHeaders["x-custom"].Single().Should().Be("custom");
        }
        
        [Fact]
        public async Task custom_header_should_be_forwarded_and_xforwarded_headers_should_be_created()
        {
            BffHost.BffOptions.AddXForwardedHeaders = true;
            BffHost.BffOptions.ForwardedHeaders.Add("x-custom");
            
            await BffHost.InitializeAsync();

            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
            req.Headers.Add("x-csrf", "1");
            req.Headers.Add("x-custom", "custom");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);

            apiResult.RequestHeaders.Count.Should().Be(4);

            apiResult.RequestHeaders["X-Forwarded-Host"].Single().Should().Be("app");
            apiResult.RequestHeaders["X-Forwarded-Proto"].Single().Should().Be("https");
            apiResult.RequestHeaders["Host"].Single().Should().Be("api");
            apiResult.RequestHeaders["x-custom"].Single().Should().Be("custom");
        }
        
        [Fact]
        public async Task custom_and_xforwarded_header_should_be_forwarded_and_xforwarded_headers_should_be_created()
        {
            BffHost.BffOptions.AddXForwardedHeaders = true;
            BffHost.BffOptions.ForwardIncomingXForwardedHeaders = true;
            BffHost.BffOptions.ForwardedHeaders.Add("x-custom");
            
            await BffHost.InitializeAsync();

            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
            req.Headers.Add("x-csrf", "1");
            req.Headers.Add("x-custom", "custom");
            req.Headers.Add("X-Forwarded-Host", "forwarded.host");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);

            apiResult.RequestHeaders.Count.Should().Be(4);

            apiResult.RequestHeaders["X-Forwarded-Proto"].Single().Should().Be("https");
            apiResult.RequestHeaders["Host"].Single().Should().Be("api");
            apiResult.RequestHeaders["x-custom"].Single().Should().Be("custom");

            var forwardedHosts = apiResult.RequestHeaders["X-Forwarded-Host"];
            forwardedHosts.Count.Should().Be(2);
            forwardedHosts.Should().Contain("app");
            forwardedHosts.Should().Contain("forwarded.host");
        }
    }
}