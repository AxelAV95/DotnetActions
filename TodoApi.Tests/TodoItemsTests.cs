using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TodoApi.Models;
using Xunit;

namespace TodoApi.Tests;

public class TodoItemsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TodoItemsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostAndGetTodoItem_ShouldSucceed()
    {
        // Arrange
        var newItem = new TodoItem { Title = "Test desde GitHub Actions", IsComplete = false };

        // Act
        var postResponse = await _client.PostAsJsonAsync("/api/todoitems", newItem);
        postResponse.EnsureSuccessStatusCode();

        var getResponse = await _client.GetAsync("/api/todoitems");
        var items = await getResponse.Content.ReadFromJsonAsync<List<TodoItem>>();

        // Assert
        Assert.NotNull(items);
        Assert.Contains(items, x => x.Title == "Test desde GitHub Actions");
    }
}
