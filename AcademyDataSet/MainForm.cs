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
		Cache GroupsRelatedData;
		public MainForm()
		{
			InitializeComponent();
			AllocConsole();

			GroupsRelatedData = new Cache();
			GroupsRelatedData.AddTable("Directions", "direction_id,direction_name");
			GroupsRelatedData.AddTable("Groups", "group_id,group_name,direction");
			GroupsRelatedData.AddRelation("GroupsDirections", "Groups,direction","Directions,direction_id");
			GroupsRelatedData.Load();
			GroupsRelatedData.Print("Directions");
			GroupsRelatedData.Print("Groups");

			//Загружаем направления из Базы в ComboBox
			cbDirections.DataSource = GroupsRelatedData.Set.Tables["Directions"];
			//Из множества полей таблицы нужно указать какое поле будет отображаться в выпадающем списке
			cbDirections.DisplayMember = "direction_name";
			//и какое поле
			cbDirections.ValueMember = "direction_id";

			cbGroups.DataSource = GroupsRelatedData.Set.Tables["Groups"];
			cbGroups.DisplayMember = "group_name";
			cbGroups.ValueMember = "group_id";
			//Check();

			GroupsRelatedData.RefreshCache();
		}
		private void cbDirections_SelectedIndexChanged(object sender, EventArgs e)
		{
			//DataRow row = GroupsRelatedData.Tables["Directions"].Rows.Find(cbDirections.SelectedValue);
			//Console.WriteLine($"{cbDirections.SelectedIndex}\t{cbDirections.SelectedValue}");
			//cbGroups.DataSource = row.GetChildRows("GroupsDirections");
			GroupsRelatedData.Set.Tables["Groups"].DefaultView.RowFilter = $"direction={cbDirections.SelectedValue}";
		}
		
		[DllImport("kernel32.dll")]
		public static extern bool AllocConsole();
		[DllImport("kernel32.dll")]
		public static extern bool FreeConsole();

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			GroupsRelatedData.RefreshCache();
			MessageBox.Show("Cache refreshed succeesfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		
	}
}
