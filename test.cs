using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

// "Program" is the class that starts the real application to be tested.
// "IClassFixture" will automatically create a "factory" for each test,
// to create a test database and fake authentication.
public class ProductTests : IClassFixture<ApplicationFactory<Grupprojekt.Program>>
{
    private readonly ApplicationFactory<Grupprojekt.Program> factory;

    public ProductTests(ApplicationFactory<Grupprojekt.Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task AddProduct()
    {

        // Create a client that can be used to call the API (just like a real HTTP client).
        // Given
        var client = factory.CreateClient();
        var dto = new Grupprojekt.ProductCreateDto("Coca Cola", "A tasty drink", 30);

        // Call an endpoint with some data.
        // It will use the fake user created in the "factory" file for authentication.
        // When
        var response = await client.PostAsJsonAsync<Grupprojekt.ProductCreateDto>("product/new", dto);
        // Hämta ut svaret från anroppet i JSON.
        var result = await response.Content.ReadFromJsonAsync<Grupprojekt.ProductResponseDto>();

        // Make sure the response is correct.
        // Then
        response.EnsureSuccessStatusCode();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Coca Cola", result.Title);
        Assert.Equal("A tasty drink", result.Description);
        Assert.Equal(30, result.Price);
        Assert.True(result.InStock);
    }
}
