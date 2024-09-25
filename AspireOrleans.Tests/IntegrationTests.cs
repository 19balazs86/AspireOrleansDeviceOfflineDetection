namespace AspireOrleans.Tests;

public sealed class IntegrationTests : IClassFixture<ApplicationFixture>
{
    private readonly ApplicationFixture _fixture;

    public IntegrationTests(ApplicationFixture applicationFixture)
    {
        _fixture = applicationFixture;
    }

    [Fact]
    public async Task Should_OrleansClient_respond_OK_When_HttpCall_occurs()
    {
        // Act
        using HttpResponseMessage httpResponse = await _fixture.OrleansClient_HttpClient.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task Should_OrleansServer_respond_OK_When_HttpCall_occurs()
    {
        // Act
        using HttpResponseMessage httpResponse = await _fixture.OrleansServer_HttpClient.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }
}
