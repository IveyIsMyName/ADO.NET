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
		SqlConnection connection;
		DataSet GroupsRelatedData;
		public MainForm()
		{
			InitializeComponent();
			AllocConsole();
			CONNECTION_STRING = ConfigurationManager.ConnectionStrings["PV_319_Import"].ConnectionString;
			connection = new SqlConnection(CONNECTION_STRING);
			Console.WriteLine(CONNECTION_STRING);
			LoadGroupsRelatedData();
		}
		void LoadGroupsRelatedData()
		{
			Console.WriteLine(nameof(GroupsRelatedData));
			//1) Создаем 'DataSet'
			GroupsRelatedData = new DataSet(nameof(GroupsRelatedData));

			//2) Добавляем таблицы в 'DataSet':
			
			const string dsTable_Directions = "Directions";
			const string dst_col_direction_id = "direction_id";
			const string dst_col_direction_name = "direction_name";
			GroupsRelatedData.Tables.Add(dsTable_Directions);
			GroupsRelatedData.Tables[dsTable_Directions].Columns.Add(dst_col_direction_id, typeof(byte));
			GroupsRelatedData.Tables [dsTable_Directions].Columns.Add(dst_col_direction_name, typeof(string));
			GroupsRelatedData.Tables[dsTable_Directions].PrimaryKey =
				new DataColumn[] { GroupsRelatedData.Tables[dsTable_Directions].Columns[dst_col_direction_id] };

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
				GroupsRelatedData.Tables[dsTable_Directions].Columns[dst_col_direction_id], //Parent field (PK)
				ds_Table_Groups.Columns["direction"]										//Child field (FK)
				);

			//4) Загружаем данные в таблицы:
			string directions_cmd	= "SELECT * FROM Directions";
			string groups_cmd		= "SELECT * FROM Groups";
			SqlDataAdapter directionsAdapter = new SqlDataAdapter(directions_cmd, connection);
			SqlDataAdapter groupsAdapter = new SqlDataAdapter(groups_cmd, connection);

			connection.Open();
			directionsAdapter.Fill(GroupsRelatedData.Tables["Directions"]);
			groupsAdapter.Fill(GroupsRelatedData.Tables["Groups"]);
			connection.Close();

			foreach(DataRow row in GroupsRelatedData.Tables["Directions"].Rows)
			{
				Console.WriteLine($"{row["direction_id"]}\t{row["direction_name"]}");
			}
			Console.WriteLine("\n------------------------------------------\n");
			foreach(DataRow row in GroupsRelatedData.Tables["Groups"].Rows)
			{
				Console.WriteLine($"{row["group_id"]}\t{row["group_name"]}\t{row.GetParentRow(dsRelation_GroupsDirections)["direction_name"]}");
			}
		}
		[DllImport("kernel32.dll")]
		public static extern bool AllocConsole();
		[DllImport("kernel32.dll")]
		public static extern bool FreeConsole();
	}
}
