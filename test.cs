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
        // Retrieve the response from the request in JSON.
        var result = await response.Content.ReadFromJsonAsync<Grupprojekt.ProductResponseDto>();

        // Make sure the response is correct.
        // Then
        response.EnsureSuccessStatusCode();
        Assert.NotNull(result); // Ensure a product is returned
        Assert.NotEqual(Guid.Empty, result.Id); // Ensure a product has an id
        Assert.Equal("Coca Cola", result.Title); // Ensure a product has the correct title
        Assert.Equal("A tasty drink", result.Description); // Ensure a product has the correct description
        Assert.Equal(30, result.Price); // Ensure a product has the correct price
        Assert.True(result.InStock); // Ensure a product is in stock when created
    }

    [Fact]
    public async Task AddProductBadRequest() // Test bad request - so the operation doesn´t succeed when it´s suppose to fail.
    {
        // Create a client that can be used to call the API (just like a real HTTP client).
        // Given
        var client = factory.CreateClient();
        var dto = new Grupprojekt.ProductCreateDto("", "A tasty drink", 30);

        // Call an endpoint with new product.
        // It will use the fake user created in the "factory" file for authentication.
        // When
        var response = await client.PostAsJsonAsync<Grupprojekt.ProductCreateDto>("product/new", dto);


        // Retrieve the response from the request in JSON.
        var result = await response.Content.ReadFromJsonAsync<Grupprojekt.ProductResponseDto>();

        // Then
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchForProduct() // Test bad request - so the operation doesn´t succeed when it´s suppose to fail.
    {
        // Create a client that can be used to call the API (just like a real HTTP client).
        // Given
        var client = factory.CreateClient();
        // Create a product
        var product = new Grupprojekt.ProductCreateDto("Coca Cola", "A tasty drink", 30);

        // Call an endpoint with new product.
        var response = await client.PostAsJsonAsync<Grupprojekt.ProductCreateDto>("product/new", product);
        response.EnsureSuccessStatusCode();  // Ensure the product was created

        // Call an endpoint with search-word.
        var searchResponse = await client.GetAsync($"product/search/{product.Title}");
        searchResponse.EnsureSuccessStatusCode();

        // When
        // Retrieve the response from the request in JSON.
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<Grupprojekt.ProductResponseDto>();

        // Then
        response.EnsureSuccessStatusCode();
        Assert.NotNull(searchResult);  // Ensure a product is returned
        Assert.Equal("Coca Cola", searchResult.Title);  // Ensure the product title matches
        Assert.Equal("A tasty drink", searchResult.Description);  // Ensure the description matches
        Assert.Equal(30, searchResult.Price);  // Ensure the price matches
    }
}
