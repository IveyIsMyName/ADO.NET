﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient;
using System.ComponentModel.Design;

namespace ADO.NET
{
	static internal class Connector
	{
		const int PADDING = 25;
		const string CONNECTION_STRING =
			"Data Source=(localdb)\\MSSQLLocalDB; " +
			"Initial Catalog=Movies;Integrated Security=True;" +
			"Connect Timeout=30;Encrypt=False;" +
			"TrustServerCertificate=False;" +
			"ApplicationIntent=ReadWrite;" +
			"MultiSubnetFailover=False";
		static readonly SqlConnection connection;
		static Connector()
		{
			connection = new SqlConnection(CONNECTION_STRING);
			//Статический конструктор нужен только для инициализации статических полей класса.
		}
		public static void SelectDirectors()
		{
			Select("*", "Directors");
		}
		public static void SelectMovies()
		{
			Connector.Select("title,release_date,FORMATMESSAGE(N'%s %s',first_name,last_name)", "Movies,Directors", "director=director_id");
		}
		public static void Select(string columns, string tables, string condition = null)
		{
			//3) Создаем команду, которую нужно выполняить на сервере
			//string cmd = "SELECT * FROM Directors";
			//string cmd = "SELECT title, release_date, FORMATMESSAGE(N'%s %s', first_name, last_name) FROM Movies, Directors WHERE director=director_id";
			string cmd = $"SELECT {columns} FROM {tables}";
			if (condition != null) cmd += $" WHERE {condition}";
			cmd += ";";

			SqlCommand command = new SqlCommand(cmd, connection);

			//4) Получаем результаты выполнения команды:
			connection.Open();
			SqlDataReader reader = command.ExecuteReader();

			//5) Обрабатываем результаты запроса:
			if (reader.HasRows)
			{
				Console.WriteLine("=====================================================================================================");
				for (int i = 0; i < reader.FieldCount; i++)
					Console.Write(reader.GetName(i).PadRight(PADDING));
				Console.WriteLine();
				Console.WriteLine("=====================================================================================================");
				while (reader.Read())
				{
					//Console.WriteLine($"{reader[0].ToString().PadRight(5)}\t{reader[2].ToString().PadRight(15)}\t{reader[1].ToString().PadRight(15)}");
					for (int i = 0; i < reader.FieldCount; i++)
					{
						Console.Write(reader[i].ToString().PadRight(PADDING));
					}
					Console.WriteLine();
				}
			}

			//6)Закрываем SqlDataReader и Connection
			reader.Close();
			connection.Close();
		}
		public static void InsertDirector(string first_name, string last_name)
		{
			//string cmd = $"INSERT Directors(first_name,last_name) VALUES (N'{first_name}',N'{last_name}')";
			//SqlCommand command = new SqlCommand(cmd, connection);
			//connection.Open();
			//command.ExecuteNonQuery();
			//connection.Close();
			Insert("Directors", "first_name,last_name", $"N'{first_name}',N'{last_name}'");
		}
		public static void InsertMovie(string title, string release_date, string director)
		{
			Insert("Movies", "title,release_date,director", $"N'{title}','{release_date}',{director}");
		}
		public static void Insert(string table, string columns, string values, string key = "")
		{
			if (key == "")
			{
				key = table.ToLower();
				key = key.Remove(key.Length - 1, 1) + "_id";
			}
			string[] all_columns = columns.Split(',');
			string[] all_values = values.Split(',');
			string condition = "";
			for (int i = 0; i < all_columns.Length; i++)
			{
				condition += $"{all_columns[i]}={all_values[i]}";
				if (i != all_columns.Length - 1) condition += " AND ";
			}
			string check_string = $"IF NOT EXISTS (SELECT {key} FROM {table} WHERE {condition})";
			string query = $"INSERT {table}({columns}) VALUES ({values})";
			string cmd = $"{check_string} BEGIN {query} END";
			Console.WriteLine(cmd);
			SqlCommand command = new SqlCommand(cmd, connection);
			connection.Open();
			command.ExecuteNonQuery();
			connection.Close();
		}
	}
}
