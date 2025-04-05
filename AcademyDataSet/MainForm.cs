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
		private readonly string CONNECTION_STRING = "";
		private readonly SqlConnection connection;
		private	DataSet GroupsRelatedData;
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

			GroupsRelatedData = CreateGroupsRelatedDataSet();

			LoadDataIntoDataSet("SELECT * FROM Directions", "Directions");
			LoadDataIntoDataSet("SELECT * FROM Groups", "Groups");

			PrintForGroupsRelatedData();
			
		}
		private DataSet CreateGroupsRelatedDataSet()
		{
			DataSet dataSet = new DataSet(nameof(GroupsRelatedData));

			DataTable dsTable_Directions = new DataTable("Directions");
			dsTable_Directions.Columns.Add("direction_id", typeof(byte));
			dsTable_Directions.Columns.Add("direction_name", typeof(string));
			dsTable_Directions.PrimaryKey = new DataColumn[] { dsTable_Directions.Columns["direction_id"] };
			dataSet.Tables.Add(dsTable_Directions);

			DataTable ds_Table_Groups = new DataTable("Groups");
			ds_Table_Groups.Columns.Add("group_id", typeof(int));
			ds_Table_Groups.Columns.Add("group_name", typeof(string));
			ds_Table_Groups.Columns.Add("direction", typeof(byte));
			ds_Table_Groups.PrimaryKey = new DataColumn[] { ds_Table_Groups.Columns["group_id"] };
			dataSet.Tables.Add(ds_Table_Groups);
			string dsRelation_GroupsDirections = "GroupsDirection";
			dataSet.Relations.Add
				(
				dsRelation_GroupsDirections,
				dataSet.Tables["Directions"].Columns["direction_id"],             //Parent field (PK)
				ds_Table_Groups.Columns["direction"]                                        //Child field (FK)
				);

			return dataSet;
		}
		private void LoadDataIntoDataSet(string query, string targetTableName)
		{
			try
			{
				connection.Open();
				FillTable(query, targetTableName);
			}
			finally
			{
				connection.Close();
			}
		}
			
		private void FillTable(string query, string tableName)
		{
			using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
			{
				adapter.Fill(GroupsRelatedData.Tables[tableName]);
			}
		}
		private void PrintTableData(DataTable table, Func<DataRow, string> formatRow)
		{
			foreach (DataRow row in table.Rows)
			{
				Console.WriteLine(formatRow(row));
			}
		}
		private void PrintForGroupsRelatedData()
		{
			PrintTableData
				(
				GroupsRelatedData.Tables["Directions"],
				row => $"{row["direction_id"]}\t{row["direction_name"]}"
				);
			Console.WriteLine("\n---------------------------------------\n");

			PrintTableData
				(
				GroupsRelatedData.Tables["Groups"],
				row => $"{row["group_id"]}\t{row["group_name"]}\t{row.GetParentRow("GroupsDirection")["direction_name"]}"
				);
		}
		[DllImport("kernel32.dll")]
		public static extern bool AllocConsole();
		[DllImport("kernel32.dll")]
		public static extern bool FreeConsole();
	}
}
