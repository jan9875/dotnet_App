using System;

namespace GameStore.Api.Models;

public class Genre
{
    private int _id;
    public int Id
    {
        get { return _id; }
        set { _id=value; }
    }

    public required string Name{get; set;}
}
