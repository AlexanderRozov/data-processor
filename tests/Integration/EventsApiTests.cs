using System.Net.Http.Json;
using Xunit;

namespace DataProcessor.IntegrationTests;

public class EventsApiTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;

    public EventsApiTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_Then_Get_Returns_Created_Event()
    {
        var request = new { value = 123 };
        var createResponse = await _client.PostAsJsonAsync("/events", request);
        createResponse.EnsureSuccessStatusCode();

        var listResponse = await _client.GetFromJsonAsync<List<EventResponse>>("/events");
        Assert.NotNull(listResponse);
        Assert.Contains(listResponse, e => e.Value == 123);
    }

    private record EventResponse(Guid Id, DateTime CreatedAt, int Value);
}

