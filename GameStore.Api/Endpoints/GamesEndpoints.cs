using System;
using GameStore.Api.Data;
using GameStore.Api.DTOS;
using GameStore.Api.Models;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
    const string GetGameEndpointName = "GetGame";

    private static readonly List<GameDto> games= [ 
        new (1, "Street Fighter II", "Fighting", 19.99M, new DateOnly(1992,7,15)),
        new (2, "The Legend of Zelda: Ocarina of Time", "Action-Adventure", 29.99M, new DateOnly(1998,11,21)),
        new (3, "Super Mario 64", "Platformer", 24.99M, new DateOnly(1996,6,23))
    ];


    public static void MapGamesEndpoints(this WebApplication app)
    {   
        var pathGroup=app.MapGroup("/games");
        pathGroup.MapGet("/", () => {return games;});



        pathGroup.MapGet("/{id}", (int id) =>
        {
            var game=games.Find(game=>game.Id==id);
            if (game == null)
            {
                return Results.NotFound();
            }
            return Results.Ok(game);
        }).WithName(GetGameEndpointName);



        pathGroup.MapPost("/", (CreateGameDto newGame, GameStoreContext dbContext) =>
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
            dbContext.SaveChanges();

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


        pathGroup.MapPut("/{id}", (int id, UpdateGameDto updatedGame) =>
        {
            var index=games.FindIndex(game=> game.Id==id);
            if (index == -1)
            {
                return Results.NotFound();
            }
            games[index]=new GameDto(id,updatedGame.Name,updatedGame.Genre,updatedGame.Price,updatedGame.ReleaseDate);
            return Results.NoContent();

        });


        pathGroup.MapDelete("/{id}", (int id) =>
        {
           games.RemoveAll(game=> game.Id==id);
           return Results.NoContent(); 
        });
    }
}
