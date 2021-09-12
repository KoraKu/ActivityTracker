using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ActivityTracker
{
    public partial class Form1 : Form
    {
        string windowName = "Activity Tracker";

        DataManagement manager = new DataManagement();

        List<DataTable> activities;
        List<DataRow> newRows = new List<DataRow>();

        DataTable myData;
        List<Button> buttons = new List<Button>();
        int currentAct = -1;
        bool updatingDisplay = false;

        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            activityPanel.Visible = false;
            activities = manager.loadList();

            myData = manager.loadMyData();
            myDataGrid.DataSource = myData;
            myDataGrid.Columns["Date"].Width = 125;
            myDataGrid.Columns["Total Activity Time"].Width = 125;

            numericUpDownAge.Value = int.Parse(myData.TableName);

            dateTimePicker1.Value = (DateTime)myData.Rows[0]["Date"];

            myDataChart.Titles.Add("Total Activity per day (last 30 days)");

            for (int i = 0; i < activities.Count; i++)
            {
                buttons.Add(AddNewButton(activities[i].TableName, buttons.Count));
            }

            totalTime.Text = "Total Activity time : " + manager.getTotalActivityTime(myData).ToString() + "min.";
            currentTime.Text = "Current Day Activity Time :\n" + manager.getDalyActivityTime(activities, DateTime.Today) + "min.";

            updateMyDataChart();
            updateMyDataOverview();
        }

        private void createNew(object sender, EventArgs e)
        {
            activities.Add(manager.newActivityTable());
            buttons.Add(AddNewButton(activities[activities.Count-1].TableName, buttons.Count));
        }

        private Button newButton(String text = "", int idx = 0) 
        {
            Button b = new Button
            {
                Name = idx.ToString(),
                Text = text,
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(255, 255, 255),
                FlatStyle = FlatStyle.Flat,
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(25,25,25);
            b.Click += selectActivity;

            return b;
        }

        private Button AddNewButton(String text, int idx)
        {
            Button newB = newButton(text, idx);
            buttonPanel.Controls.Add(newB);

            return newB;
        }
        
        private void selectActivity(object sender, EventArgs e)
        {

            Button clicked = (Button)sender;

            if (clicked.Name == buttonInfo.Name)
            {
                currentAct = -1;

                //Check for non existing dates
                while (newRows.Count > 1)
                {
                    DataRow row = newRows[0];

                    bool present = false;

                    for (int myDataRow = 0; myDataRow < myData.Rows.Count; myDataRow++)
                    {
                        if ((DateTime)myData.Rows[myDataRow]["Date"] == (DateTime)row["Date"])
                        {
                            present = true;
                        }
                    }

                    if (!(present))
                    {
                        myData.Rows.Add((DateTime)row["Date"], 0);
                    }

                    newRows.RemoveAt(0);
                }

                //Update all
                for (int row = 0; row < myData.Rows.Count; row++)
                {
                    DataRow currentRow = myData.Rows[row];
                    currentRow = manager.updateMyData(currentRow, (DateTime)currentRow["Date"], manager.getDalyActivityTime(activities, (DateTime)currentRow["Date"]));
                }
            }
            else
            {
                currentAct = int.Parse(clicked.Name);
            }

            displayPanel(currentAct);
        }

        private void deleteActivityEvent(object sender, EventArgs e)
        {
            if (currentAct != -1)
            {
                buttonPanel.Controls.Remove(buttons[currentAct]);
                buttons.RemoveAt(currentAct);
                activities.RemoveAt(currentAct);

                currentAct = -1;

                //Updates all buttons name
                for (int i = 0; i<buttons.Count; i++)
                {
                    buttons[i].Name = i.ToString();
                }

                displayPanel(currentAct);

            }
        }

        private void displayPanel(int toDisplay)
        {
            if (toDisplay == -1)
            {
                activityPanel.Visible = false; 
                myDataPanel.Visible = true;

                totalTime.Text = "Total Activity time : " + manager.getTotalActivityTime(myData).ToString() + "min.";
                currentTime.Text = "Current Day Activity Time :" + manager.getDalyActivityTime(activities, DateTime.Today) + "min.";

                updateMyDataChart();
                updateMyDataOverview();
            }
            else
            {
                myDataPanel.Visible = false;
                activityPanel.Visible = true;

                activityGrid.DataSource = activities[toDisplay];
                activityNameTextBox.Text = activities[toDisplay].TableName;
                activityGrid.Columns["TimeUnit"].Visible = false;


                updatingDisplay = true;

                int selected = activityGrid.CurrentCell.RowIndex;
                DataRow selectedRow = activities[toDisplay].Rows[selected];

                timeUnit.SelectedIndex = (int)selectedRow["TimeUnit"];
                scoreUnit.SelectedIndex = manager.getUnitIndexFromValue(selectedRow["Unit"].ToString());
                numericUpDown2.Value = Convert.ToDecimal(manager.convertTime((double)selectedRow["Duration"], timeUnit.SelectedIndex, 0));
                numericUpDown1.Value = Convert.ToDecimal(selectedRow["Score"]);
                dateTimePicker2.Value = (System.DateTime)selectedRow["Date"];

                hoursLabel.Text = selectedRow["Duration"].ToString() + " min.";


                updatingDisplay = false;
            }

            manager.saveAll(myData, activities);
        }

        //MyData Tab controls
        private void myDataGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int selected = myDataGrid.CurrentCell.RowIndex;
            DataRow selectedRow = myData.Rows[selected];
            
            dateTimePicker1.Value = (DateTime)selectedRow["Date"];

            updateMyDataChart();

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            int selected = myDataGrid.CurrentCell.RowIndex;
            DataRow selectedRow = myData.Rows[selected];

            selectedRow = manager.updateMyData(selectedRow, dateTimePicker1.Value.Date, manager.getDalyActivityTime(activities, (DateTime)selectedRow["Date"]));
        }

        private void myDataNew_Click(object sender, EventArgs e)
        {
            myData.Rows.Add(DateTime.Today.Date, manager.getDalyActivityTime(activities, DateTime.Today));
            updateMyDataChart();
        }

        private void myDataDelete_Click(object sender, EventArgs e)
        {
            int selected = myDataGrid.CurrentCell.RowIndex;
            DataRow selectedRow = myData.Rows[selected];

            DialogResult result = MessageBox.Show("Do you really want to delete this day's tracking ?", windowName+" - Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                myData.Rows.Remove(selectedRow);
            }
            if (myData.Rows.Count < 1)
            {
                myData.Rows.Add(DateTime.Today.Date, 0);
            }
            updateMyDataChart();
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            DataView d = new DataView(myData);
            d.Sort = "Date ASC";

            DataTable temp = d.ToTable();

            myData = temp;

            myDataGrid.DataSource = myData;

            manager.saveAll(myData, activities);

            displayPanel(-1);


        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            myDataChart.Titles.Clear();
            myDataChart.Titles.Add("Total Activity per day (last " + numericUpDown3.Value.ToString() + " days)");

            updateMyDataChart();
        }

        private void updateMyDataChart()
        {
            DataView d = new DataView(myData);
            d.Sort = "Date ASC";

            DataTable temp = d.ToTable();


            List<String> dateArray = new List<String>();
            List<double> timeArray = new List<double>();

            for (int i = 0; i < temp.Rows.Count; i++)
            {
                if (dateArray.Count >= numericUpDown3.Value)
                {
                    dateArray.RemoveAt(0);
                    timeArray.RemoveAt(0);
                }
                dateArray.Add(temp.Rows[i]["Date"].ToString().Substring(0,10));
                timeArray.Add((double)temp.Rows[i]["Total Activity Time"]);
            }

            myDataChart.Titles.Clear();
            myDataChart.Titles.Add("Total Activity per day (last " + numericUpDown3.Value.ToString() + " days)");

            myDataChart.Series.Clear();

            var series = myDataChart.Series.Add("Activity Time");
            series.ChartType = SeriesChartType.Spline;
            series.BorderWidth = 2;

            var series2 = myDataChart.Series.Add("Your Average");
            series2.ChartType = SeriesChartType.Line;
            series2.BorderWidth = 2;

            double avg = manager.averageActivityTime(timeArray);

            for (int i = 0; i < dateArray.Count; i++)
            {
                series.Points.AddXY(dateArray[i], timeArray[i]);
                series2.Points.AddXY(dateArray[i], avg);
            }

        }
        private void numericUpDownAge_ValueChanged(object sender, EventArgs e)
        {
            
            int age = (int)numericUpDownAge.Value;

            myData.TableName = age.ToString();

            updateMyDataOverview();
            manager.saveAll(myData, activities);
        }

        private void updateMyDataOverview()
        {
            var values = manager.PhysicalActivityCheck(int.Parse(myData.TableName));

            msgBox.Text = values.msg;

            double total = manager.getDalyActivityTime(activities, DateTime.Today.Date);

            if (myData.TableName == "1")
            {
                labelConclusion.Text = "";
            }
            else if (values.max == -1) 
            {
                if (total >= values.min)
                {
                    labelConclusion.Text = "Your activitiy time is within WHO recommendations";
                }
                else
                {
                    labelConclusion.Text = "Your activitiy time is lower than WHO recommendations";
                }
            }
            else
            {
                if (total >= values.min && total <= values.max)
                {
                    labelConclusion.Text = "Your activitiy time is within WHO recommendations";
                }
                else if (total <= values.min)
                {
                    labelConclusion.Text = "Your activitiy time is lower than WHO recommendations";
                }
                else if (total >= values.max)
                {
                    labelConclusion.Text = "Your activitiy time is higher than WHO recommendations";
                }
            }
        }

        //Activities tab controls
        private void activityNameTextBox_TextChanged(object sender, EventArgs e)
        {
            buttons[currentAct].Text = activityNameTextBox.Text;
            activities[currentAct].TableName = activityNameTextBox.Text;
        }

        private void activity_ValueChanged(object sender, EventArgs e)
        {

            if (!(updatingDisplay))
            {

                int selected = activityGrid.CurrentCell.RowIndex;
                DataRow selectedRow = activities[currentAct].Rows[selected];

                double duration = manager.convertTime((double)numericUpDown2.Value, timeUnit.SelectedIndex, 1);

                hoursLabel.Text = manager.convertTime((double)numericUpDown2.Value, timeUnit.SelectedIndex, 1).ToString() + " min.";

                selectedRow = manager.updateMyActivity(selectedRow, dateTimePicker2.Value.Date, duration, timeUnit.SelectedIndex, (double)numericUpDown1.Value, scoreUnit.SelectedItem.ToString());

                newRows.Add(activities[currentAct].Rows[activities[currentAct].Rows.Count - 1]);
            }
        }

        private void activityGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int selected = activityGrid.CurrentCell.RowIndex;
            DataRow selectedRow = activities[currentAct].Rows[selected];

            updatingDisplay = true;

            timeUnit.SelectedIndex = (int)selectedRow["TimeUnit"];
            scoreUnit.SelectedIndex = manager.getUnitIndexFromValue(selectedRow["Unit"].ToString());
            numericUpDown2.Value = Convert.ToDecimal(manager.convertTime((double)selectedRow["Duration"], timeUnit.SelectedIndex, 0));
            numericUpDown1.Value = Convert.ToDecimal(selectedRow["Score"]);
            dateTimePicker2.Value = (System.DateTime)selectedRow["Date"];

            hoursLabel.Text = selectedRow["Duration"].ToString() + " min.";


            updatingDisplay = false;
        }

        private void newActivityDay(object sender, EventArgs e)
        {
            int beforeLastRow = activities[currentAct].Rows.Count - 1;

            string lastUnit =  activities[currentAct].Rows[beforeLastRow]["Unit"].ToString();

            activities[currentAct].Rows.Add(DateTime.Today.Date, 0.0, 1, 0.0, lastUnit);

            newRows.Add(activities[currentAct].Rows[activities[currentAct].Rows.Count-1]);
        }

        private void deleteActivityDay(object sender, EventArgs e)
        {
            int selected = activityGrid.CurrentCell.RowIndex;
            DataRow selectedRow = activities[currentAct].Rows[selected];

            DialogResult result = MessageBox.Show("Do you really want to delete this day's tracking ?", windowName + " - Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                activities[currentAct].Rows.Remove(selectedRow);
            }
            if (activities[currentAct].Rows.Count < 1)
            {
                activities[currentAct].Rows.Add(DateTime.Today.Date, 0.0, 1, 0.0, " ");
            }
        }

        private void activitySort_Click(object sender, EventArgs e)
        {
            DataView d = new DataView(activities[currentAct]);
            d.Sort = "Date ASC";

            DataTable temp = d.ToTable();

            activities[currentAct] = temp;

            activityGrid.DataSource = myData;

            manager.saveAll(myData, activities);

            displayPanel(currentAct);
        }


        //More and header buttons
        private void btnMore_Click(object sender, EventArgs e)
        {
            if (btnMore.Text == "+")
            {
                morePanel.Visible = true;
                btnMore.Text = "-";
            }
            else
            {
                morePanel.Visible = false;
                btnMore.Text = "+";
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/KoraKu/ActivityTracker");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.who.int/news-room/fact-sheets/detail/physical-activity");
        }
        /*
        private void btnImport_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Importing data will replace existing data.\nPlease ensure that you selected the correct folder and backup your current data before procceding.", windowName + " - Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.OK)
            {
                folderBrowserDialog1.Description = "New Activity Tracker Folder";
                DialogResult folderResult = folderBrowserDialog1.ShowDialog();
                if (!(folderResult == DialogResult.Cancel))
                {
                    String path = folderBrowserDialog1.SelectedPath;
                    String appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ActivityTracker";
                    //Delete in %appdata%
                    System.IO.Directory.Delete(appPath, true);
                    //Copy from path to %appdata%

                    bool x = System.IO.File.Exists(path + "\\myData.xml") && System.IO.Directory.Exists(path + "\\Activities");

                    if (x)
                    {
                        System.IO.Directory.CreateDirectory(appPath);
                        System.IO.File.Copy(path + "\\myData.xml", appPath + "\\myData.xml");

                        System.IO.Directory.CreateDirectory(appPath + "\\Activities");
                        int filesCount = System.IO.Directory.GetFiles(path + "\\Activities").Length;


                        for (int i = 0; i < filesCount; i++)
                        {
                            System.IO.File.Copy(path + "\\Activities\\" + i.ToString() + ".xml", appPath + "\\Activities\\" + i.ToString() + ".xml");
                        }

                        MessageBox.Show("Activity Trakcer will restart in order to apply changes.", windowName + " - Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Application.Restart();
                    }
                }                
            }
        }
        
        private void btnExport_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Activity Tracker export destination";
            DialogResult r = folderBrowserDialog1.ShowDialog();

            if (r == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;

                manager.saveAll(myData, activities, path);
            }
            
        }
        */
        private void btnHelp_Click(object sender, EventArgs e)
        {
            FormHelp fHelp = new FormHelp();
            fHelp.Text = windowName + " - About + Help";
            fHelp.ShowDialog();
        }


        /*
        private void btnDeleteAll_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("This operation is irreversible.\nYou can export your data to get a backup.", windowName + " - Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.OK)
            {
                //Delete
                System.IO.Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ActivityTracker", true);

                MessageBox.Show("Activity Trakcer will restart in order to apply changes.", windowName + " - Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Restart();
            }
        }
        */
        //Saving on close
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            manager.saveAll(myData, activities);
        }
    }
}
