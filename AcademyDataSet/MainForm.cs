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
				string cmd = $"SELECT * FROM {tables[i].Split(',')[0]}";
				SqlDataAdapter adapter = new SqlDataAdapter(cmd, connection);
				adapter.Fill(GroupsRelatedData.Tables[tables[i].Split(',')[0]]);
			}
		}
		public void LoadGroupsRelatedData()
		{
			Console.WriteLine(nameof(GroupsRelatedData));
			//1) Создаем 'DataSet'
			//Перенесли в конструктор

			//2) Добавляем таблицы в 'DataSet':

			DataTable dsTable_Directions = new DataTable("Directions");
			dsTable_Directions.Columns.Add("direction_id", typeof(byte));
			dsTable_Directions.Columns.Add("direction_name", typeof(string));
			dsTable_Directions.PrimaryKey = new DataColumn[] { dsTable_Directions.Columns["direction_id"] };
			GroupsRelatedData.Tables.Add(dsTable_Directions);

			DataTable ds_Table_Groups = new DataTable("Groups");
			ds_Table_Groups.Columns.Add("group_id", typeof(int));
			ds_Table_Groups.Columns.Add("group_name", typeof(string));
			ds_Table_Groups.Columns.Add("direction", typeof(byte));
			ds_Table_Groups.PrimaryKey = new DataColumn[] { ds_Table_Groups.Columns["group_id"] };
			GroupsRelatedData.Tables.Add(ds_Table_Groups);

			//3) Строим связи между таблицами:
			string dsRelation_GroupsDirections = "GroupsDirection";
			GroupsRelatedData.Relations.Add
				(
				dsRelation_GroupsDirections,
				GroupsRelatedData.Tables["Directions"].Columns["direction_id"],             //Parent field (PK)
				ds_Table_Groups.Columns["direction"]                                        //Child field (FK)
				);

			//4) Загружаем данные в таблицы:
			string directions_cmd = "SELECT * FROM Directions";
			string groups_cmd = "SELECT * FROM Groups";
			SqlDataAdapter directionsAdapter = new SqlDataAdapter(directions_cmd, connection);
			SqlDataAdapter groupsAdapter = new SqlDataAdapter(groups_cmd, connection);

			connection.Open();
			directionsAdapter.Fill(GroupsRelatedData.Tables["Directions"]);
			groupsAdapter.Fill(GroupsRelatedData.Tables["Groups"]);
			connection.Close();

			foreach (DataRow row in GroupsRelatedData.Tables["Directions"].Rows)
			{
				Console.WriteLine($"{row["direction_id"]}\t{row["direction_name"]}");
			}
			Console.WriteLine("\n------------------------------------------\n");
			foreach (DataRow row in GroupsRelatedData.Tables["Groups"].Rows)
			{
				Console.WriteLine($"{row["group_id"]}\t{row["group_name"]}\t{row.GetParentRow(dsRelation_GroupsDirections)["direction_name"]}");
			}
		}
		void Print(string table)
		{
			Console.WriteLine("\n-------------------------------------\n");
			foreach(DataRow row in GroupsRelatedData.Tables[table].Rows)
			{
				for (int i = 0; i < row.ItemArray.Length; i++)
				{
					Console.Write(row[i].ToString() + "\t");
				}
				Console.WriteLine();
			}
			Console.WriteLine("\n-------------------------------------\n");
		}
		void Check()
		{
			AddTable("Directions", "direction_id,direction_name");
			AddTable("Groups", "group_id,group_name,direction");
			AddRelation("GroupsDirections", "Groups,direction", "Directions,direction_id");
			Load();
			Print("Directions");
			Print("Groups");
		}
		[DllImport("kernel32.dll")]
		public static extern bool AllocConsole();
		[DllImport("kernel32.dll")]
		public static extern bool FreeConsole();
	}
}
