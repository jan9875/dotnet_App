namespace GameStore.Api.XUnit;
using System;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using GameStore.Api;
using FluentAssertions;
using System.Net.Http.Json;
using GameStore.Api.Models;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using GameStore.Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;

public class MyTestClass: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MyTestClass(WebApplicationFactory<Program> factory)
    {
        _client=factory.CreateClient();

        using (var scope= factory.Services.CreateScope())
        {
            var dbContext=scope.ServiceProvider.GetRequiredService<GameStoreContext>();
            dbContext.Games.RemoveRange(dbContext.Games);
            dbContext.SaveChanges();
        }
    }
    
    [Fact]
    public async Task Test1()
    {
        for(int i=0;i<5;i++){
            var response = await _client.PostAsync("/games", new StringContent("{\"name\":\"Test Game\",\"genreId\": 1, \"price\":49.99,\"releaseDate\":\"2023-01-01\"}", System.Text.Encoding.UTF8, "application/json"));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        }
        var response1= await _client.GetAsync("/games");
        response1.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        response1.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        var games= await response1.Content.ReadFromJsonAsync<JsonArray>();

        games.Should().HaveCount(5);
    }
    [Fact]
    public async Task Test2()
    {
        var jsonObject= new
        {
            name="Test Game 1",
            genreId=1,
            price= 9.99,
            releaseDate="2023-01-01"
        };
        var response= await _client.PostAsJsonAsync("/games",jsonObject);
        
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var content1=await response.Content.ReadFromJsonAsync<Game>();
        

        var response2=await _client.DeleteAsync($"/games/{content1!.Id}");

        response2.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Test3()
    {

        var response1 = await _client.PostAsync("/games", new StringContent("{\"name\":\"Test Game\",\"genreId\": 1, \"price\":49.99,\"releaseDate\":\"2023-01-01\"}", System.Text.Encoding.UTF8, "application/json"));
        response1.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var content= await response1.Content.ReadFromJsonAsync<Game>();
        var id=content!.Id;
        Console.WriteLine(id);
        var response= await _client.GetAsync($"/games/{id}");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();

        body.Should().Contain("Test Game");
    }
}
