using System;
using GameStore.Api.Data;
using GameStore.Api.DTOS;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GenresEndpoints
{
    public static void MapGenresEndpoints(this WebApplication app)
    {
        var pathGroup= app.MapGroup("/genres");

        pathGroup.MapGet("/", async (GameStoreContext dbContext) =>
            await dbContext.Genres
                           .Select(genre => new GenreDto(genre.Id,genre.Name))
                           .AsNoTracking()
                           .ToListAsync()
        );
    }
}
