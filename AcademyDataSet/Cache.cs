using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;


namespace AcademyDataSet
{
	internal class Cache
	{
		readonly string CONNECTION_STRING = "";
		readonly SqlConnection connection;
		public DataSet Set { get; set; }
		List<string> tables;
		List<string> commands;
		public Cache()
		{
			CONNECTION_STRING = ConfigurationManager.ConnectionStrings["PV_319_Import"].ConnectionString;
			connection = new SqlConnection(CONNECTION_STRING);
			Console.WriteLine(CONNECTION_STRING);

			tables = new List<string>();
			Set = new DataSet(nameof(Set));
		}
		public void AddTable(string tableName, string columns)
		{
			DataTable dt = new DataTable(tableName);
			string[] seperatedColumns = columns.Split(',');
			Set.Tables.Add(dt);
			for (int i = 0; i < seperatedColumns.Length; i++)
			{
				Set.Tables[tableName].Columns.Add(seperatedColumns[i]);
			}
			Set.Tables[tableName].PrimaryKey =
				new DataColumn[] { Set.Tables[tableName].Columns[seperatedColumns[0]] };
			tables.Add($"{tableName},{columns}");
		}
		public void AddRelation(string name, string child, string parent)
		{
			Set.Relations.Add
				(
				name,
				Set.Tables[parent.Split(',')[0]].Columns[parent.Split(',')[1]],
				Set.Tables[child.Split(',')[0]].Columns[child.Split(',')[1]]
				);
		}
		public void Load()
		{
			string[] tables = this.tables.ToArray();
			for (int i = 0; i < tables.Length; i++)
			{
				string columns = "";
				DataColumnCollection column_collection = Set.Tables[tables[i].Split(',')[0]].Columns;
				foreach (DataColumn column in column_collection)
				{
					columns += $"[{column.ColumnName}],";
				}
				columns = columns.Remove(columns.LastIndexOf(','));
				Console.WriteLine(columns);
				string cmd = $"SELECT {columns} FROM {tables[i].Split(',')[0]}";
				SqlDataAdapter adapter = new SqlDataAdapter(cmd, connection);
				adapter.Fill(Set.Tables[tables[i].Split(',')[0]]);
			}
		}
		public void Print(string table)
		{
			Console.WriteLine("\n-------------------------------------\n");
			string parent_table_name = "";
			string parent_column_name = "";
			int parent_index = -1;
			string relation_name = "No relation";
			if (hasParents(table))
			{
				relation_name = Set.Tables[table].ParentRelations[0].RelationName;
				parent_table_name = Set.Tables[table].ParentRelations[0].ParentTable.TableName;
				parent_column_name = parent_table_name.ToLower().Substring(0, parent_table_name.Length - 1) + "_name";
				Console.WriteLine(parent_table_name);
				//DataColumn parent_column = GroupsRelatedData.Tables[parent_table_name].Columns["direction_name"];
				parent_index =
					Set.Tables[table].Columns.
					IndexOf(parent_table_name.ToLower().Substring(0, parent_table_name.Length - 1));
				Console.WriteLine(parent_index);
			}

			foreach (DataRow row in Set.Tables[table].Rows)
			{
				for (int i = 0; i < row.ItemArray.Length; i++)
				{
					if (i == parent_index)
					{
						//DataRow parent_row = row.GetParentRow(relation_name);
						//Console.Write(parent_row[parent_column_name]);
						DataRow parent_row = row.GetParentRow(relation_name);

						if (parent_row == null)
						{
							Console.Write("(no parent)");
						}
						else
						{
							// Проверим, есть ли столбец parent_column_name
							if (parent_row.Table.Columns.Contains(parent_column_name))
							{
								Console.Write(parent_row[parent_column_name]);
							}
							else
							{
								Console.Write($"(column '{parent_column_name}' not found)");
							}
						}
					}
					else
						Console.Write(row[i].ToString() + "\t");
				}
				//if (hasParents(table))
				//{
				//	DataRow parent_row = row.GetParentRow(GroupsRelatedData.Tables[table].ParentRelations[0].RelationName);
				//	Console.Write(parent_row["direction_name"] + "\t");
				//}
				Console.WriteLine();
			}

			//foreach (DataRow row in GroupsRelatedData.Tables[table].Rows)
			//{
			//	for (int i = 0; i < row.ItemArray.Length; i++)
			//	{
			//		object value = row[i];
			//		if (hasParents(table))
			//		{
			//			//Находим отношение, где текущая таблица - дочерняя
			//			foreach (DataRelation relation in GroupsRelatedData.Relations)
			//			{
			//				if (
			//					relation.ChildTable.TableName == table &&
			//					relation.ChildColumns[0].ColumnName == GroupsRelatedData.Tables[table].Columns[i].ColumnName
			//					)
			//				{
			//					value = row.GetParentRow(relation)?[1] ?? value;
			//					break;
			//				}
			//			}
			//		}
			//		Console.Write(value + "\t");
			//	}
			//	Console.WriteLine();
			//}
			Console.WriteLine("\n-------------------------------------\n");
		}
		bool hasParents(string table)
		{
			return Set.Tables[table].ParentRelations.Count > 0;
		}
		void Check()
		{
			AddTable("Directions", "direction_id,direction_name");
			AddTable("Groups", "group_id,group_name,direction");
			AddTable("Students", "stud_id,last_name,first_name,middle_name,birth_date,group");
			AddRelation("GroupsDirections", "Groups,direction", "Directions,direction_id");
			AddRelation("StudentsGroups", "Students,group", "Groups,group_id");

			Load();
			Print("Directions");
			Print("Groups");
			Print("Students");
		}
	}
}
