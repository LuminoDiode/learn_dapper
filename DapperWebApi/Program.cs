using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Options;
using DapperDatabaseAccess;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.MapGet("api/status", async () =>
{
	var t = new Npgsql.NpgsqlConnectionStringBuilder();
    t.Host = "localhost";
    t.Username = "postgres";
    t.Password = "qwerty";
	using var conn = new NpgsqlConnection(t.ConnectionString);

	RequestTemplates templates = new("dapper_learn1");
	var result = await conn.ExecuteReaderAsync(templates.FindDatabase());
    if (!result.HasRows)
    {
        await result.DisposeAsync();
		await conn.ExecuteAsync(templates.CreateDatabase());
        return Results.Ok("created.");
    }
	return Results.Ok("exists.");
});


app.MapGet("api/endpoint1", async () =>
{
    var t = new Npgsql.NpgsqlConnectionStringBuilder();
    t.Host = "localhost";
    t.Username = "postgres";
    t.Password = "qwerty";
    t.IncludeErrorDetail = true;

    DatabaseContext ctx = new("dapper_learn2", t.ConnectionString);
    await ctx.EnsureDatabaseRecreated();

    t.Database = "dapper_learn2";
	ctx = new("dapper_learn2", t.ConnectionString);

    await ctx.CreateTable("users", new Dictionary<string, string>
    {
        {"user_id", "SERIAL PRIMARY KEY" },
        {"user_city","varchar(50)" },
       {"user_age", "smallint CHECK(user_age>=0)" }
    });

    await ctx.InsertIntoTable("users", new string[][]
    {
        new string[]
        {
            "DEFAULT", "MOSCOW",22.ToString()
        },
         new string[]
        {
            "DEFAULT", "AMSTERDAM",23.ToString()
		}
        , new string[]
        {
            "DEFAULT", "LONDON",24.ToString()
		}
    });
});

app.MapGet("api/endpoint2", async () =>
{
    var templates = new RequestTemplates("dapper_learn2");

    return Results.Ok(templates.InsertValues("users", new string[][]
    {
        new string[]
        {
            "DEFAULT", "MOSCOW",22.ToString()
        },
         new string[]
        {
            "DEFAULT", "AMSTERDAM",23.ToString()
        }
        , new string[]
        {
            "DEFAULT", "LONDON",24.ToString()
        }
    }));
});




app.Run();
