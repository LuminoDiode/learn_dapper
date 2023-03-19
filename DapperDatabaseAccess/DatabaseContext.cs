using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dapper;
using Npgsql;

namespace DapperDatabaseAccess;

public interface IDatabaseContext
{

}

public class DatabaseContext: IDatabaseContext
{
	public string DatabaseName { get; init; }
	public string ConnectionString { get; init; }
	private RequestTemplates _requestTemplates { get; init; }

	private static string BuildStringFromValues(IEnumerable<KeyValuePair<string, object?>> connectionValues)
	{
		var builder = new NpgsqlConnectionStringBuilder();
		foreach (var t in connectionValues) builder.Add(t);
		return builder.ConnectionString;
	}

	public DatabaseContext(string databaseName, IEnumerable<KeyValuePair<string, object?>> connectionValues)
		: this(databaseName, BuildStringFromValues(connectionValues)) { }
	public DatabaseContext(string databaseName, string connectionString)
	{
		this.DatabaseName = databaseName;
		this.ConnectionString = connectionString;

		this._requestTemplates = new(this.DatabaseName);
	}

	#region Database methods
	public async Task<bool> IsDatabaseExists()
	{
		await using var conn = new NpgsqlConnection(this.ConnectionString);
		await using var result = await conn.ExecuteReaderAsync(_requestTemplates.FindDatabase());

		return result.HasRows;
	}
	public async Task EnsureDatabaseCreated()
	{
		if (!(await IsDatabaseExists())) await CreateDatabase();
	}
	public async Task EnsureDatabaseDropped()
	{
		if (await IsDatabaseExists()) await DropDatabase();
	}
	public async Task EnsureDatabaseRecreated()
	{
		await EnsureDatabaseDropped();
		await CreateDatabase();
	}

	private async Task CreateDatabase()
	{
		await using var conn = new NpgsqlConnection(this.ConnectionString);
		await conn.ExecuteAsync(_requestTemplates.CreateDatabase());
	}
	private async Task DropDatabase()
	{
		await using var conn = new NpgsqlConnection(this.ConnectionString);
		await conn.ExecuteAsync(_requestTemplates.DropDatabase());
	}
	#endregion

	#region Tables methods
	public async Task<DbDataReader> GetAllTables() {
		await using var conn = new NpgsqlConnection(this.ConnectionString);
		var result = await conn.ExecuteReaderAsync(_requestTemplates.GetTables());

		return result;
	}
	public async Task<bool> IsTableExists(string name)
	{
		await using var conn = new NpgsqlConnection(this.ConnectionString);
		await using var result = await conn.ExecuteReaderAsync(_requestTemplates.GetTables(name));

		return result.HasRows;
	}
	public async Task CreateTable(string tableName, Dictionary<string,string> columnToType)
	{
		await using var conn = new NpgsqlConnection(this.ConnectionString);
		await conn.ExecuteAsync(_requestTemplates.CreateTable(tableName,columnToType));
	}
	public async Task DropTable(string tableName)
	{
		await using var conn = new NpgsqlConnection(this.ConnectionString);
		await conn.ExecuteAsync(_requestTemplates.DropTable(tableName));
	}
	#endregion

	#region Insert methods
	public async Task InsertIntoTable(string tableName, IEnumerable<IEnumerable<object>> rows)
	{
		await using var conn = new NpgsqlConnection(this.ConnectionString);
		await conn.ExecuteAsync(_requestTemplates.InsertValues(tableName, rows));
	}
	#endregion
}
