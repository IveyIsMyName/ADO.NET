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
		
		Dictionary<string, int> d_directions;
		Dictionary<string, int> d_groups;

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
			cbGroupsDirections.Items.AddRange(d_directions.Select(d => d.Key).ToArray());
			cbStudentsDirection.Items.AddRange(d_directions.Select (d => d.Key).ToArray());
			cbStudentsGroup.Items.Insert(0, "Все группы");
			cbStudentsDirection.Items.Insert(0, "Все направления");
			cbStudentsDirection.SelectedIndex = cbStudentsGroup.SelectedIndex = 0;
			dgvStudents.DataSource = connector.Select
				("last_name,first_name,middle_name,birth_date,group_name," +
				"direction_name", "Students,Groups,Directions",
				"[group]=group_id AND direction=direction_id");
			toolStripStatusLabelCount.Text = $"Количество студентов:{dgvStudents.Rows.Count - 1}";
			
			cbGroupsDirections.SelectedIndexChanged += cbGroupsDirections_SelectedIndexChanged;
			//cbGroupsDirections.Items.AddRange(connector.SelectColumn("direction_name", "Directions").ToArray());
		}
		
		private void cbGroupsDirections_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cbGroupsDirections.SelectedIndex == -1) return;

			//string selectedDirection = cbGroupsDirections.SelectedItem.ToString();

			// Загрузка только групп выбранного направления
			//dgvGroups.DataSource = connector.Select(
			//	"group_name, dbo.GetLearningDaysFor(group_name) AS weekdays, start_time, direction_name",
			//	"Groups,Directions",
			//	$"direction=direction_id AND direction_name='{selectedDirection}'"
			//);

			dgvGroups.DataSource = connector.Select
						(
						"group_name,dbo.GetLearningDaysFor(group_name) AS weekdays,start_time,direction_name", "Groups,Directions",
						$"direction=direction_id AND direction = '{d_directions[cbGroupsDirections.SelectedItem.ToString()]}'"
						);
			toolStripStatusLabelCount.Text = $"Количество групп: {CountRecordsInDGV(dgvGroups)}";
		}
		void LoadPage(int i, Query query = null)
		{
			if(query==null)query = queries[i];
			tables[i].DataSource = connector.Select(query.Columns, query.Tables, query.Condition, query.Group_by);
			toolStripStatusLabelCount.Text = status_messages[i] + CountRecordsInDGV(tables[i]);
		}
		private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			LoadPage(tabControl.SelectedIndex);
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

		}

		private void btnShowAll_Click(object sender, EventArgs e)
		{
			cbGroupsDirections.SelectedIndex = -1;
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

		private void cbStudentsDirection_SelectedIndexChanged(object sender, EventArgs e)
		{
			int i = cbStudentsDirection.SelectedIndex;
			Dictionary<string, int> d_groups = connector.GetDictionary
				(
				"group_id,group_name",
				"Groups",
				i == 0 ? "" : $"direction={d_directions[cbStudentsDirection.SelectedItem.ToString()]}"
				);
			cbStudentsGroup.Items.Clear();
			cbStudentsGroup.Items.AddRange(d_groups.Select(g => g.Key).ToArray());

			int t = tabControl.SelectedIndex;
			
			Query q = new Query (queries[t]);
			q.Condition = (i == 0 ? "" : $"direction={d_directions[cbStudentsDirection.SelectedItem.ToString()]}");
			LoadPage(0, q);
			toolStripStatusLabelCount.Text = $"Количество студентов:{dgvStudents.Rows.Count - 1}";
		}

		private void btnGroupsDirectionCLR_Click(object sender, EventArgs e)
		{
			dgvStudents.DataSource = connector.Select
				(queries[0].Columns, queries[0].Tables);
			cbStudentsDirection.SelectedIndex = cbStudentsGroup.SelectedIndex = 0;
			toolStripStatusLabelCount.Text = $"Количество студентов:{dgvStudents.Rows.Count - 1}";
		}
	}
}
