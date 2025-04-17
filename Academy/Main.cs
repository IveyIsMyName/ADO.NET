using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Configuration;

namespace Academy
{
	public partial class Main : Form
	{
		Connector connector;

		public Dictionary<string, int> d_directions;
		public Dictionary<string, int> d_groups;

		public Dictionary<ComboBox, List<ComboBox>> d_children;
		public Dictionary<ComboBox, List<ComboBox>> d_parents;

		DataGridView[] tables;
		Query[] queries = new Query[]
		{
			new Query
			(
				"last_name,first_name,middle_name,birth_date,group_name,direction_name",
				"Students JOIN Groups ON ([group]=group_id) JOIN Directions ON (direction=direction_id)"
				//"[group]=group_id AND direction=direction_id"),
			),
			new Query
			(
				"group_name,dbo.GetLearningDaysFor(group_name) AS weekdays,start_time,direction_name", "Groups,Directions",
				"direction=direction_id"
			),
			new Query
			(
				"direction_name,COUNT(DISTINCT group_id) AS group_count, " +
				"COUNT(stud_id) AS student_count",
				"Students RIGHT JOIN Groups ON ([group]=group_id) " +
				"RIGHT JOIN Directions ON (direction=direction_id)",
				"",
				"direction_name"
			),
			new Query ("*", "Disciplines"),
			new Query ("*", "Teachers")
		};
		string[] status_messages = new string[]
		{
			$"Количество студентов: ",
			$"Количество групп: ",
			$"Количество направлений: ",
			$"Количество дисциплин: ",
			$"Количество преподавателй: ",
		};

		public Main()
		{
			InitializeComponent();

			d_children = new Dictionary<ComboBox, List<ComboBox>>()
			{
				{ cbStudentsDirection, new List<ComboBox> () { cbStudentsGroup } }
			};
			d_parents = new Dictionary<ComboBox, List<ComboBox>>()
			{
				{cbStudentsGroup, new List<ComboBox> {cbStudentsDirection} }
			};

			tables = new DataGridView[]
		{
			dgvStudents,
			dgvGroups,
			dgvDirections,
			dgvDisciplines,
			dgvTeachers
		};
			connector = new Connector
			   (
				   ConfigurationManager.ConnectionStrings["PV_319_Import"].ConnectionString
			   );
			d_directions = connector.GetDictionary("*", "directions");
			d_groups = connector.GetDictionary("group_id,group_name", "Groups");
			cbStudentsGroup.Items.AddRange(d_groups.Select(g => g.Key).ToArray());
			cbGroupsDirection.Items.AddRange(d_directions.Select(d => d.Key).ToArray());
			cbStudentsDirection.Items.AddRange(d_directions.Select(d => d.Key).ToArray());
			cbStudentsGroup.Items.Insert(0, "Все группы");
			cbStudentsDirection.Items.Insert(0, "Все направления");
			cbGroupsDirection.Items.Insert(0, "Все направления");
			cbStudentsGroup.SelectedIndex = 0;
			cbStudentsDirection.SelectedIndex = 0;
			cbGroupsDirection.SelectedIndex = 0;
			dgvStudents.DataSource = connector.Select
				("last_name,first_name,middle_name,birth_date,group_name," +
				"direction_name", "Students,Groups,Directions",
				"[group]=group_id AND direction=direction_id");
			toolStripStatusLabelCount.Text = $"Количество студентов:{dgvStudents.Rows.Count - 1}";

			cbGroupsDirection.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
			//cbGroupsDirections.Items.AddRange(connector.SelectColumn("direction_name", "Directions").ToArray());
		}

		//private void cbGroupsDirections_SelectedIndexChanged(object sender, EventArgs e)
		//{
		//	if (cbGroupsDirections.SelectedIndex == -1) return;

		//	//string selectedDirection = cbGroupsDirections.SelectedItem.ToString();

		//	// Загрузка только групп выбранного направления
		//	//dgvGroups.DataSource = connector.Select(
		//	//	"group_name, dbo.GetLearningDaysFor(group_name) AS weekdays, start_time, direction_name",
		//	//	"Groups,Directions",
		//	//	$"direction=direction_id AND direction_name='{selectedDirection}'"
		//	//);

		//	dgvGroups.DataSource = connector.Select
		//				(
		//				"group_name,dbo.GetLearningDaysFor(group_name) AS weekdays,start_time,direction_name", "Groups,Directions",
		//				$"direction=direction_id AND direction = '{d_directions[cbGroupsDirections.SelectedItem.ToString()]}'"
		//				);
		//	toolStripStatusLabelCount.Text = $"Количество групп: {CountRecordsInDGV(dgvGroups)}";
		//}

		void LoadPage(int i, Query query = null)
		{
			if (query == null) query = queries[i];
			tables[i].DataSource = connector.Select(query.Columns, query.Tables, query.Condition, query.Group_by);
			toolStripStatusLabelCount.Text = status_messages[i] + CountRecordsInDGV(tables[i]);
		}
		private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			string tab_name = tabControl.SelectedTab.Name;
			Console.WriteLine(tab_name);
			LoadPage(tabControl.SelectedIndex);

			#region SWITCH
			//switch (tabControl.SelectedIndex)
			//{
			//	case 0:
			//		dgvStudents.DataSource = connector.Select
			//			(
			//			"last_name,first_name,middle_name,birth_date,group_name," +
			//			"direction_name", "Students,Groups,Directions",
			//			"[group]=group_id AND direction=direction_id"
			//			);
			//		toolStripStatusLabelCount.Text = $"Количество студентов:{dgvStudents.Rows.Count - 1}";
			//		break;
			//	case 1:
			//		dgvGroups.DataSource = connector.Select
			//			(
			//			"group_name,dbo.GetLearningDaysFor(group_name) AS weekdays,start_time,direction_name", "Groups,Directions", 
			//			"direction=direction_id"
			//			);
			//		toolStripStatusLabelCount.Text = $"Количество групп:{dgvGroups.Rows.Count - 1}";
			//		break;
			//	case 2:
			//		//dgvDirections.DataSource = connector.Select("*", "Directions");
			//		dgvDirections.DataSource = connector.Select
			//			(
			//		   //"d.direction_id, d.direction_name, " +
			//		   //"COUNT(DISTINCT g.group_id) AS group_count, " +
			//		   //"COUNT(s.stud_id) AS student_count",
			//		   //"Directions d " +
			//		   //"LEFT JOIN Groups g ON d.direction_id = g.direction " +
			//		   //"LEFT JOIN Students s ON g.group_id = s.[group]",
			//		   //"",
			//		   //"d.direction_id, d.direction_name"

			//		   //"direction_name,COUNT(DISTINCT group_id) AS group_count, COUNT(stud_id) AS student_count",
			//		   //"Students,Groups,Directions",
			//		   //"[group]=group_id AND direction=direction_id",
			//		   //"direction_name"

			//		   "direction_name,COUNT(DISTINCT group_id) AS group_count, " +
			//		   "COUNT(stud_id) AS student_count",
			//		   "Students RIGHT JOIN Groups ON ([group]=group_id) " +
			//		   "RIGHT JOIN Directions ON (direction=direction_id)",
			//		   "",
			//		   "direction_name"
			//		   );
			//		toolStripStatusLabelCount.Text = $"Количество направлений:{dgvDirections.Rows.Count - 1}";
			//		break;
			//	case 3:
			//		dgvDisciplines.DataSource = connector.Select("*", "Disciplines");
			//		toolStripStatusLabelCount.Text = $"Количество дисциплин:{dgvDisciplines.Rows.Count - 1}";
			//		break;
			//	case 4:
			//		dgvTeachers.DataSource = connector.Select("*", "Teachers");
			//		toolStripStatusLabelCount.Text = $"Количество преподавателей:{dgvTeachers.Rows.Count - 1}";
			//		break;
			//} 
			#endregion

		}

		private void btnShowAll_Click(object sender, EventArgs e)
		{
			dgvGroups.DataSource = connector.Select(
				"group_name, dbo.GetLearningDaysFor(group_name) AS weekdays, start_time, direction_name",
				"Groups,Directions",
				"direction=direction_id"
			);
			toolStripStatusLabelCount.Text = $"Количество групп: {dgvGroups.Rows.Count - 1}";
		}

		int CountRecordsInDGV(DataGridView dgv)
		{
			return dgv.Rows.Count == 0 ? 0 : dgv.Rows.Count - 1;
		}

		private void btnEmptyDirections_Click(object sender, EventArgs e)
		{
			dgvDirections.DataSource = connector.Select(
			"direction_name, COUNT(DISTINCT group_id) AS group_count, " +
			"COUNT(stud_id) AS student_count",
			"Students RIGHT JOIN Groups ON ([group]=group_id) " +
			"RIGHT JOIN Directions ON (direction=direction_id)",
			"",
			"direction_name HAVING COUNT(DISTINCT group_id) = 0 AND COUNT(stud_id) = 0");

			toolStripStatusLabelCount.Text = $"Количество пустых направлений: {CountRecordsInDGV(dgvDirections)}";
		}

		private void btnShowAllDirections_Click(object sender, EventArgs e)
		{
			int i = tabControl.SelectedIndex;
			Query query = queries[i];
			dgvDirections.DataSource = connector.Select(query.Columns, query.Tables, query.Condition, query.Group_by);
			toolStripStatusLabelCount.Text = status_messages[i] + CountRecordsInDGV(dgvDirections);
		}

		private void btnShowNonEmptyDirections_Click(object sender, EventArgs e)
		{
			dgvDirections.DataSource = connector.Select(
			"direction_name, COUNT(DISTINCT group_id) AS group_count, " +
			"COUNT(stud_id) AS student_count",
			"Students RIGHT JOIN Groups ON ([group]=group_id) " +
			"RIGHT JOIN Directions ON (direction=direction_id)",
			"",
			"direction_name HAVING COUNT(DISTINCT group_id) > 0 AND COUNT(stud_id) > 0");

			toolStripStatusLabelCount.Text = $"Количество заполненных направлений: {CountRecordsInDGV(dgvDirections)}";
		}

		private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			string cb_name = (sender as ComboBox).Name;
			Console.WriteLine(cb_name);
			string tab_name = tabControl.SelectedTab.Name;

			int last_capital_index =
				Array.FindLastIndex<char>(cb_name.ToCharArray(), Char.IsUpper);
			string cb_suffix = cb_name.Substring(last_capital_index);
			Console.WriteLine(cb_suffix);

			int i = (sender as ComboBox).SelectedIndex;
			string dictionary_name = $"d_{cb_suffix.ToLower()}s";
			Dictionary<string, int> dictionary = this.GetType().GetField(dictionary_name).GetValue(this) as Dictionary<string, int>;

			if (d_children.ContainsKey(sender as ComboBox))
			{
				foreach (ComboBox cb in d_children[sender as ComboBox])
				{
					GetDependentData(cb, sender as ComboBox);
				}
			}

			int t = tabControl.SelectedIndex;

			Query q = new Query(queries[t]);

			string condition =
				(i == 0 || (sender as ComboBox).SelectedItem == null ? "" : $"[{cb_suffix.ToLower()}]={dictionary[$"{(sender as ComboBox).SelectedItem}"]}");
			string parent_condition = "";
			if (d_parents.ContainsKey(sender as ComboBox))
			{
				foreach (ComboBox cb in d_parents[sender as ComboBox])
				{
					if (cb.SelectedItem != null && cb.SelectedIndex >0)
					{
						string column_name = cb.Name.Substring(Array.FindLastIndex<char>(cb.Name.ToCharArray(), Char.IsUpper));
						string parent_dictionary_name = $"d_{column_name.ToLower()}s";
						Dictionary<string, int> parent_dictionary = this.GetType().GetField(parent_dictionary_name).GetValue(this) as Dictionary<string, int>;
						if (parent_condition != "") parent_condition += " AND ";
						parent_condition += $"[{column_name}]= {parent_dictionary[cb.SelectedItem.ToString()]}"; 
					}
				}
			}
			if (q.Condition == "") q.Condition = condition;
			else if (condition != "") q.Condition += $" AND {condition}";
			if (q.Condition == "") q.Condition = parent_condition;
			else if (parent_condition != "") q.Condition += $" AND {parent_condition}";
				LoadPage(t, q);
			toolStripStatusLabelCount.Text = $"Количество студентов:{dgvStudents.Rows.Count - 1}";
		}
		void GetDependentData(ComboBox dependent, ComboBox determinant)
		{
			Console.WriteLine("\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");
			Console.WriteLine(dependent.Name + "\t" + determinant.Name);
			string dependent_root = dependent.Name.Substring(Array.FindLastIndex<char>(dependent.Name.ToCharArray(), Char.IsUpper));
			string determinant_root = determinant.Name.Substring(Array.FindLastIndex<char>(determinant.Name.ToCharArray(), Char.IsUpper));
			Dictionary<string, int> dictionary =
				connector.GetDictionary
				(
					$"{dependent_root.ToLower()}_id,{dependent_root.ToLower()}_name",
					$"{dependent_root}s,{determinant_root}s",
					determinant.SelectedItem == null || determinant.SelectedIndex <= 0 ? "" : $"{determinant_root.ToLower()}={determinant.SelectedIndex}"
					);

			dependent.Items.Clear();
			dependent.Items.AddRange(dictionary.Select(d => d.Key).ToArray());
			dependent.Items.Insert(0, "Все группы");
			dependent.SelectedIndex = 0;
			Console.WriteLine("Dependent:\t" + dependent_root);
			Console.WriteLine("Determinant:\t" + determinant_root);
			Console.WriteLine("\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");
		}

		private void btnGroupsDirectionCLR_Click(object sender, EventArgs e)
		{
			dgvStudents.DataSource = connector.Select
				(queries[0].Columns, queries[0].Tables);
			cbStudentsDirection.SelectedIndex = cbStudentsGroup.SelectedIndex = 0;
			toolStripStatusLabelCount.Text = $"Количество студентов:{dgvStudents.Rows.Count - 1}";
		}

		//private void cbStudentsGroup_SelectedIndexChanged(object sender, EventArgs e)
		//{
		//	string cb_name = (sender as ComboBox).Name;
		//	Console.WriteLine(cb_name);
		//	string tab_name = tabControl.SelectedTab.Name;

		//	int last_capital_index =
		//		Array.FindLastIndex<char>(cb_name.ToCharArray(), Char.IsUpper);
		//	string cb_suffix = cb_name.Substring(last_capital_index);
		//	Console.WriteLine(cb_suffix);

		//	int i = (sender as ComboBox).SelectedIndex;
		//	string dictionary_name = $"d_{cb_suffix.ToLower()}s";
		//	Dictionary<string, int> dictionary = this.GetType().GetField(dictionary_name).GetValue(this) as Dictionary<string, int>;

		//	int t = tabControl.SelectedIndex;

		//	Query q = new Query(queries[t]);

		//	string condition =
		//		(i == 0 || (sender as ComboBox).SelectedItem == null ? "" : $"[{cb_suffix.ToLower()}]={dictionary[$"{(sender as ComboBox).SelectedItem}"]}");

		//	if (q.Condition == "") q.Condition = condition;
		//	else if (condition != "") q.Condition += $" AND {condition}";

		//	LoadPage(t, q);
		//	toolStripStatusLabelCount.Text = $"Количество студентов:{dgvStudents.Rows.Count - 1}";
		//}
	}
}
