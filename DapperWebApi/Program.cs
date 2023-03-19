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




app.Run();
