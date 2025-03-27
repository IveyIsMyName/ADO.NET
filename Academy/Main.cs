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
		public Main()
		{
			InitializeComponent();
			Connector connector = new Connector
				(
					ConfigurationManager.ConnectionStrings["PV_319_Import"].ConnectionString
				);

			dgvStudents.DataSource = connector.Select("*", "Students");
			dgvGroups.DataSource = connector.Select("*", "Groups");
			dgvDirections.DataSource = connector.Select("*", "Directions");
			dgvDisciplines.DataSource = connector.Select("*", "Disciplines");
			dgvTeachers.DataSource = connector.Select("*", "Teachers");
		
			UpdateStatusBar();
		}
		public void UpdateStatusBar()
		{
			int studentCount = dgvStudents.Rows.Count;
			int groupCount = dgvGroups.Rows.Count;
			int directionCount = dgvDirections.Rows.Count;
			int disciplinesCount = dgvDisciplines.Rows.Count;
			int teacherCount = dgvTeachers.Rows.Count;

			lblStudentCount.Text = $"Students:{studentCount}";
			lblGroupCount.Text = $"Groups:{groupCount}";
			lblDirectionsCount.Text = $"Directions:{directionCount}";
			lblDisciplineCount.Text = $"Disciplines:{disciplinesCount}";
			lblTeacherCount.Text = $"Teachers:{teacherCount}";
		}
	}
}
