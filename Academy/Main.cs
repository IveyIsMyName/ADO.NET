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
		public Main()
		{
			InitializeComponent();
			connector = new Connector
			   (
				   ConfigurationManager.ConnectionStrings["PV_319_Import"].ConnectionString
			   );
			dgvStudents.DataSource = connector.Select
				("last_name,first_name,middle_name,birth_date,group_name," +
				"direction_name", "Students,Groups,Directions",
				"[group]=group_id AND direction=direction_id");
			toolStripStatusLabelCount.Text = $"Количество студентов:{dgvStudents.Rows.Count - 1}";

			//UpdateStatusStrip();
		}
		//public void UpdateStatusStrip()
		//{
		//	int studentCount = dgvStudents.Rows.Count - 1;
		//	int groupCount = dgvGroups.Rows.Count - 1;
		//	int directionCount = dgvDirections.Rows.Count - 1;
		//	int disciplinesCount = dgvDisciplines.Rows.Count - 1;
		//	int teacherCount = dgvTeachers.Rows.Count - 1;

		//	lblStudentCount.Text = $"Students:{studentCount}";
		//	lblGroupCount.Text = $"Groups:{groupCount}";
		//	lblDirectionsCount.Text = $"Directions:{directionCount}";
		//	lblDisciplineCount.Text = $"Disciplines:{disciplinesCount}";
		//	lblTeacherCount.Text = $"Teachers:{teacherCount}";
		//}

		private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (tabControl.SelectedIndex)
			{
				case 0:
					dgvStudents.DataSource = connector.Select
						(
						"last_name,first_name,middle_name,birth_date,group_name," +
						"direction_name", "Students,Groups,Directions",
						"[group]=group_id AND direction=direction_id"
						);
					toolStripStatusLabelCount.Text = $"Количество студентов:{dgvStudents.Rows.Count - 1}";
					break;
				case 1:
					dgvGroups.DataSource = connector.Select
						(
						"group_name,dbo.GetLearningDaysFor(group_name) AS weekdays,start_time,direction_name", "Groups,Directions", 
						"direction=direction_id"
						);
					toolStripStatusLabelCount.Text = $"Количество групп:{dgvGroups.Rows.Count - 1}";
					break;
				case 2:
					dgvDirections.DataSource = connector.Select("*", "Directions");
					toolStripStatusLabelCount.Text = $"Количество направлений:{dgvDirections.Rows.Count - 1}";
					break;
				case 3:
					dgvDisciplines.DataSource = connector.Select("*", "Disciplines");
					toolStripStatusLabelCount.Text = $"Количество дисциплин:{dgvDisciplines.Rows.Count - 1}";
					break;
				case 4:
					dgvTeachers.DataSource = connector.Select("*", "Teachers");
					toolStripStatusLabelCount.Text = $"Количество преподавателей:{dgvTeachers.Rows.Count - 1}";
					break;
			}
		}
	}
}
