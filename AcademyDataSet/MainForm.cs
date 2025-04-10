using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data.SqlClient;
using System.Configuration;
using System.Runtime.InteropServices;

namespace AcademyDataSet
{
	public partial class MainForm : Form
	{
		readonly string CONNECTION_STRING = "";
		readonly SqlConnection connection;
		DataSet GroupsRelatedData;
		List<string> tables;
		List<string> commands;
		public MainForm()
		{
			InitializeComponent();
			AllocConsole();
			CONNECTION_STRING = ConfigurationManager.ConnectionStrings["PV_319_Import"].ConnectionString;
			connection = new SqlConnection(CONNECTION_STRING);
			Console.WriteLine(CONNECTION_STRING);

			tables = new List<string>();
			GroupsRelatedData = new DataSet(nameof(GroupsRelatedData));
			//LoadGroupsRelatedData();
			Check();
			//Загружаем направления из Базы в ComboBox
			cbDirections.DataSource = GroupsRelatedData.Tables["Directions"];
			//Из множества полей таблицы нужно указать какое поле будет отображаться в выпадающем списке
			cbDirections.DisplayMember = "direction_name";
			//и какое поле
			cbDirections.ValueMember = "direction_id";

			cbGroups.DataSource = GroupsRelatedData.Tables["Groups"];
			cbGroups.DisplayMember = "group_name";
			cbGroups.ValueMember = "group_id";
		}
		public void AddTable(string tableName, string columns)
		{
			DataTable dt = new DataTable(tableName);
			string[] seperatedColumns = columns.Split(',');
			GroupsRelatedData.Tables.Add(dt);
			for (int i = 0; i < seperatedColumns.Length; i++)
			{
				GroupsRelatedData.Tables[tableName].Columns.Add(seperatedColumns[i]);
			}
			GroupsRelatedData.Tables[tableName].PrimaryKey =
				new DataColumn[] { GroupsRelatedData.Tables[tableName].Columns[seperatedColumns[0]] };
			tables.Add($"{tableName},{columns}");
		}
		public void AddRelation(string name, string child, string parent)
		{
			GroupsRelatedData.Relations.Add
				(
				name,
				GroupsRelatedData.Tables[parent.Split(',')[0]].Columns[parent.Split(',')[1]],
				GroupsRelatedData.Tables[child.Split(',')[0]].Columns[child.Split(',')[1]]
				);
		}
		public void Load()
		{
			string[] tables = this.tables.ToArray();
			for (int i = 0; i < tables.Length; i++)
			{
				string columns = "";
				DataColumnCollection column_collection = GroupsRelatedData.Tables[tables[i].Split(',')[0]].Columns;
				foreach (DataColumn column in column_collection)
				{
					columns += $"[{column.ColumnName}],";
				}
				columns = columns.Remove(columns.LastIndexOf(','));
				Console.WriteLine(columns);
				string cmd = $"SELECT {columns} FROM {tables[i].Split(',')[0]}";
				SqlDataAdapter adapter = new SqlDataAdapter(cmd, connection);
				adapter.Fill(GroupsRelatedData.Tables[tables[i].Split(',')[0]]);
			}
		}
		//public void LoadGroupsRelatedData()
		//{
		//	Console.WriteLine(nameof(GroupsRelatedData));
		//	//1) Создаем 'DataSet'
		//	//Перенесли в конструктор

		//	//2) Добавляем таблицы в 'DataSet':

		//	DataTable dsTable_Directions = new DataTable("Directions");
		//	dsTable_Directions.Columns.Add("direction_id", typeof(byte));
		//	dsTable_Directions.Columns.Add("direction_name", typeof(string));
		//	dsTable_Directions.PrimaryKey = new DataColumn[] { dsTable_Directions.Columns["direction_id"] };
		//	GroupsRelatedData.Tables.Add(dsTable_Directions);

		//	DataTable ds_Table_Groups = new DataTable("Groups");
		//	ds_Table_Groups.Columns.Add("group_id", typeof(int));
		//	ds_Table_Groups.Columns.Add("group_name", typeof(string));
		//	ds_Table_Groups.Columns.Add("direction", typeof(byte));
		//	ds_Table_Groups.PrimaryKey = new DataColumn[] { ds_Table_Groups.Columns["group_id"] };
		//	GroupsRelatedData.Tables.Add(ds_Table_Groups);

		//	//3) Строим связи между таблицами:
		//	string dsRelation_GroupsDirections = "GroupsDirection";
		//	GroupsRelatedData.Relations.Add
		//		(
		//		dsRelation_GroupsDirections,
		//		GroupsRelatedData.Tables["Directions"].Columns["direction_id"],             //Parent field (PK)
		//		ds_Table_Groups.Columns["direction"]                                        //Child field (FK)
		//		);

		//	//4) Загружаем данные в таблицы:
		//	string directions_cmd = "SELECT * FROM Directions";
		//	string groups_cmd = "SELECT * FROM Groups";
		//	SqlDataAdapter directionsAdapter = new SqlDataAdapter(directions_cmd, connection);
		//	SqlDataAdapter groupsAdapter = new SqlDataAdapter(groups_cmd, connection);

		//	connection.Open();
		//	directionsAdapter.Fill(GroupsRelatedData.Tables["Directions"]);
		//	groupsAdapter.Fill(GroupsRelatedData.Tables["Groups"]);
		//	connection.Close();

		//	foreach (DataRow row in GroupsRelatedData.Tables["Directions"].Rows)
		//	{
		//		Console.WriteLine($"{row["direction_id"]}\t{row["direction_name"]}");
		//	}
		//	Console.WriteLine("\n------------------------------------------\n");
		//	foreach (DataRow row in GroupsRelatedData.Tables["Groups"].Rows)
		//	{
		//		Console.WriteLine($"{row["group_id"]}\t{row["group_name"]}\t{row.GetParentRow(dsRelation_GroupsDirections)["direction_name"]}");
		//	}
		//}
		void Print(string table)
		{
			Console.WriteLine("\n-------------------------------------\n");
			string parent_table_name = "";
			string parent_column_name = "";
			int parent_index = -1;
			string relation_name = "No relation";
			if (hasParents(table))
			{
				relation_name = GroupsRelatedData.Tables[table].ParentRelations[0].RelationName;
				parent_table_name = GroupsRelatedData.Tables[table].ParentRelations[0].ParentTable.TableName;
				parent_column_name = parent_table_name.ToLower().Substring(0, parent_table_name.Length - 1) + "_name";
				Console.WriteLine(parent_table_name);
				//DataColumn parent_column = GroupsRelatedData.Tables[parent_table_name].Columns["direction_name"];
				parent_index =
					GroupsRelatedData.Tables[table].Columns.
					IndexOf(parent_table_name.ToLower().Substring(0, parent_table_name.Length - 1));
				Console.WriteLine(parent_index);
			}

			foreach (DataRow row in GroupsRelatedData.Tables[table].Rows)
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
			return GroupsRelatedData.Tables[table].ParentRelations.Count > 0;
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
		[DllImport("kernel32.dll")]
		public static extern bool AllocConsole();
		[DllImport("kernel32.dll")]
		public static extern bool FreeConsole();

		private void cbDirections_SelectedIndexChanged(object sender, EventArgs e)
		{
			//DataRow row = GroupsRelatedData.Tables["Directions"].Rows.Find(cbDirections.SelectedValue);
			//Console.WriteLine($"{cbDirections.SelectedIndex}\t{cbDirections.SelectedValue}");
			//cbGroups.DataSource = row.GetChildRows("GroupsDirections");
			GroupsRelatedData.Tables["Groups"].DefaultView.RowFilter = $"direction={cbDirections.SelectedValue}";
		}
	}
}
