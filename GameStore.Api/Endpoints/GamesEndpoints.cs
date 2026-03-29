using System;
using GameStore.Api.Data;
using GameStore.Api.DTOS;
using GameStore.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
    const string GetGameEndpointName = "GetGame";


    public static void MapGamesEndpoints(this WebApplication app)
    {   
        var pathGroup=app.MapGroup("/games");

        app.MapGet("/", () => Results.Ok("Hello from Games Endpoint!"));



        pathGroup.MapGet("/", async ([FromServices]GameStoreContext dbContext) => {
            var temp=await dbContext.Games
                                    .Include(game=>game.Genre)
                                    .Select(game =>new GameSummaryDto(
                                            game.Id,
                                            game.Name,
                                            game.Genre!.Name,
                                            game.Price,
                                            game.ReleaseDate
                                    )).AsNoTracking().ToListAsync();
            return temp;
            });



        pathGroup.MapGet("/{id}", async ([FromRoute]int id, [FromServices] GameStoreContext dbContext) =>
        {
            var game=await dbContext.Games.FindAsync(id);
            if (game is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(new GameDetailsDto(game.Id,game.Name,game.GenreId,game.Price,game.ReleaseDate));
        }).WithName(GetGameEndpointName);



        pathGroup.MapPost("/", async ([FromBody]CreateGameDto newGame, [FromServices]GameStoreContext dbContext) =>
        {
            if (string.IsNullOrEmpty(newGame.Name))
            {
                return Results.BadRequest("Name is required");
            }
            
            Game game= new Game
            {
                Name=newGame.Name,
                GenreId=newGame.GenreId,
                Price=newGame.Price,
                ReleaseDate=newGame.ReleaseDate
            };

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();

            GameDetailsDto gameDetails= new
            (
                game.Id,
                game.Name,
                game.GenreId,
                game.Price,
                game.ReleaseDate
            );
            return Results.CreatedAtRoute(GetGameEndpointName, new {id=gameDetails.Id}, gameDetails);
        });


        pathGroup.MapPut("/{id}", async ([FromRoute]int id,[FromBody] UpdateGameDto updatedGame,[FromServices] GameStoreContext dbContext) =>
        {
            var existingGame=await dbContext.Games.FindAsync(id);
            
            if (existingGame is null)
            {
                return Results.NotFound();
            }

            existingGame.Name=updatedGame.Name;
            existingGame.GenreId=updatedGame.GenreId;
            existingGame.Price=updatedGame.Price;
            existingGame.ReleaseDate=updatedGame.ReleaseDate;
            
            await dbContext.SaveChangesAsync();

            return Results.NoContent();

        });


        pathGroup.MapDelete("/{id}", async ([FromRoute]int id, [FromServices]GameStoreContext dbContext) =>
        {
            await dbContext.Games
                            .Where(game=> game.Id==id)
                            .ExecuteDeleteAsync();

            return Results.NoContent(); 
        });
    }
}
