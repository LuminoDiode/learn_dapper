using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperDatabaseAccess;

public class RequestTemplates
{
	public readonly string DatabaseName;
	public RequestTemplates(string databaseName)
	{
		if (string.IsNullOrWhiteSpace(databaseName))
			throw new ArgumentException(nameof(databaseName));
		DatabaseName = databaseName;
	}

	private const string _findDatabase = """
		SELECT datname FROM pg_database WHERE datname = '{0}';
	""";
	public string FindDatabase()
	=> string.Format(_findDatabase, this.DatabaseName);


	private const string _createDatabase = """
		CREATE DATABASE {0};
	""";
	public string CreateDatabase()
		=> string.Format(_createDatabase, this.DatabaseName);


	private const string _dropDatabase = """
		DROP DATABASE {0};
	""";
	public string DropDatabase()
		=> string.Format(_createDatabase, this.DatabaseName);


	private const string _listTables = """
		SELECT table_name
		FROM information_schema.tables
		WHERE table_schema = 'public' {0}
		ORDER BY table_name;
	""";
	public string GetTables(string? searchName = null)
	 => string.Format(_listTables, searchName is not null ?
		 $"AND table_name = '{searchName}'" : string.Empty);


	private const string _createTable = """
		CREATE TABLE {0}({1});
	""";
	public string CreateTable(string tableName, IEnumerable<KeyValuePair<string, string>> columnNameToType)
	 => string.Format(_createTable, tableName,
		 string.Join(",", columnNameToType.Select(x => $"{x.Key} {x.Value}")));
}
