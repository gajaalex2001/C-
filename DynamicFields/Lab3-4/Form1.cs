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
using System.Data.Common;

namespace Lab3_4
{
    public partial class Form1 : Form
    {
        DataSet ds = new DataSet();
        SqlDataAdapter adapter = new SqlDataAdapter();

        public Form1()
        {
            InitializeComponent();
        }

        public void getParentList()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["CompanieJocuri"].ConnectionString;
                SqlConnection conn = new SqlConnection(connectionString);
                string parentTable = ConfigurationManager.AppSettings["ParentTableName"];
                adapter.SelectCommand = new SqlCommand("SELECT * FROM " + parentTable, conn);
                adapter.Fill(ds, parentTable);
                dataGridViewParent.DataSource = ds.Tables[parentTable];
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void getChildList()
        {
            try
            {
                if (dataGridViewParent.SelectedCells.Count > 0)
                {
                    string pkParent = dataGridViewParent.CurrentRow.Cells[0].Value.ToString();
                    string connectionString = ConfigurationManager.ConnectionStrings["CompanieJocuri"].ConnectionString;
                    SqlConnection conn = new SqlConnection(connectionString);
                    string childTable = ConfigurationManager.AppSettings["ChildTableName"];
                    string fkStatement = ConfigurationManager.AppSettings["pk_parent"];
                    adapter.SelectCommand = new SqlCommand("SELECT * FROM " + childTable + " WHERE " + fkStatement, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@Parent", pkParent);
                    if (ds.Tables[childTable] != null)
                        ds.Tables[childTable].Clear();
                    adapter.Fill(ds, childTable);
                    dataGridViewChild.DataSource = ds.Tables[childTable];
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void loadTextBoxes()
        {
            panelTextBoxes.Controls.Clear();
            List<string> childColumnsList = new List<string>(ConfigurationManager.AppSettings["ChildColumnNames"].Split(','));
            int textboxCount = Convert.ToInt32(ConfigurationManager.AppSettings["textbox_count"]);
            int locationTextBox = 50;
            int locationLabel = 50;
            int index = 1;
            while (index <= textboxCount)
            {
                Label label = new Label();
                TextBox textBox = new TextBox();
                label.Location = new Point(0, locationLabel);
                textBox.Location = new Point(100, locationTextBox);
                textBox.Name = childColumnsList[index - 1];
                //Console.WriteLine(textBox.Name);
                label.Text = ConfigurationManager.AppSettings["label" + index.ToString()];
                // Console.WriteLine(label.Text);
                panelTextBoxes.Controls.Add(label);
                panelTextBoxes.Controls.Add(textBox);
                locationTextBox += textBox.Height + 5;
                locationLabel += label.Height + 5;
                index++;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            getParentList();
            loadTextBoxes();
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string parentKey = ConfigurationManager.AppSettings["parentKey"];
                string childTable = ConfigurationManager.AppSettings["ChildTableName"];
                string insertedParametersNames = ConfigurationManager.AppSettings["InsertedParametersNames"];
                string childColumnNames = ConfigurationManager.AppSettings["InsertNames"];
                List<string> childColumnsList = new List<string>(ConfigurationManager.AppSettings["ChildColumnNames"].Split(','));
                string connectionString = ConfigurationManager.ConnectionStrings["CompanieJocuri"].ConnectionString;
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                if (dataGridViewParent.SelectedCells.Count > 0)
                {
                    string idd = dataGridViewParent.CurrentRow.Cells[0].Value.ToString();
                    SqlCommand cmd = new SqlCommand("INSERT INTO " + childTable + " (" + childColumnNames + ") VALUES (" + insertedParametersNames + ")", conn);
                    foreach (string parameter in childColumnsList)
                    {
                        TextBox textBox = (TextBox)panelTextBoxes.Controls[parameter];
                        cmd.Parameters.AddWithValue("@" + parameter, textBox.Text);
                    }
                    cmd.Parameters.AddWithValue("@" + parentKey, idd);
                    
                    cmd.ExecuteNonQuery();
                    getChildList();
                    MessageBox.Show("Record inserted succesfully!");
                    conn.Close();
                }
                else
                {
                    MessageBox.Show("Plese select a parent record");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void delBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewChild.SelectedCells.Count > 0)
                {
                    string pkChild = dataGridViewChild.CurrentRow.Cells[0].Value.ToString();
                    string childTable = ConfigurationManager.AppSettings["ChildTableName"];
                    string pkStatement = ConfigurationManager.AppSettings["pk_child"];
                    string connectionString = ConfigurationManager.ConnectionStrings["CompanieJocuri"].ConnectionString;
                    SqlConnection conn = new SqlConnection(connectionString);
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("DELETE FROM " + childTable + " WHERE " + pkStatement, conn);
                    cmd.Parameters.AddWithValue("@Child", pkChild);
                    cmd.ExecuteNonQuery();
                    getChildList();
                    MessageBox.Show("Deleted item succesfully!");
                    conn.Close();
                }
                else
                {
                    MessageBox.Show("Plese select a child record");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void modBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewChild.SelectedCells.Count > 0)
                {
                    string pkChild = dataGridViewChild.CurrentRow.Cells[0].Value.ToString();
                    string childTable = ConfigurationManager.AppSettings["ChildTableName"];

                    List<string> childColumnsList = new List<string>(ConfigurationManager.AppSettings["ChildColumnNames"].Split(','));
                    string updateColumns = ConfigurationManager.AppSettings["UpdateColumns"];
                    string connectionString = ConfigurationManager.ConnectionStrings["CompanieJocuri"].ConnectionString;
                    SqlConnection conn = new SqlConnection(connectionString);
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("Update " + childTable + " SET " + updateColumns, conn);
                    cmd.Parameters.AddWithValue("@id", pkChild);
                    foreach (string column in childColumnsList)
                    {
                        TextBox textBox = (TextBox)panelTextBoxes.Controls[column];
                        cmd.Parameters.AddWithValue("@" + column, textBox.Text);
                    }

                    cmd.ExecuteNonQuery();
                    getChildList();
                    MessageBox.Show("Updated item succesfully!");
                    conn.Close();
                }
                else
                {
                    MessageBox.Show("Plese select a child record");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void dataGridViewParent_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            getChildList();
        }

        private void dataGridViewParent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            getChildList();
        }

        private void dataGridViewChild_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dataGridViewChild.SelectedCells.Count > 0)
                {
                    int index = dataGridViewChild.CurrentCell.RowIndex;
                    string childTable = ConfigurationManager.AppSettings["ChildTableName"];
                    List<string> childColumnsList = new List<string>(ConfigurationManager.AppSettings["ChildColumnNames"].Split(','));
                    
                    foreach (string col in childColumnsList)
                    {
                        TextBox textBox = (TextBox)panelTextBoxes.Controls[col];
                        textBox.Text = ds.Tables[childTable].Rows[index][col].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
