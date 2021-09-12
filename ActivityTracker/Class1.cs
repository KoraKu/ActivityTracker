using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;

namespace ActivityTracker
{
    class DataManagement
    {

        public String appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        //My data

        public DataTable loadMyData()
        {
            DataTable myDataTable = new DataTable();

            String path = appData + "/ActivityTracker/myData.xml";

            if (System.IO.File.Exists(path))
            {
                myDataTable.ReadXml(path);
                return myDataTable;
            }
            else
            {
                //data col
                DataColumn dates = new DataColumn
                {
                    ColumnName = "Date",
                    DataType = typeof(System.DateTime)

                };
                myDataTable.Columns.Add(dates);

                //activity time col
                DataColumn activityTime = new DataColumn
                {
                    ColumnName = "Total Activity Time",
                    DataType = typeof(double)
                };
                myDataTable.Columns.Add(activityTime);

                myDataTable.Rows.Add(DateTime.Today, 0);

                myDataTable.DefaultView.Sort = "";
                myDataTable.TableName = "20";

                return myDataTable;

            }
        }

        public DataRow updateMyData(DataRow selected, DateTime date, double totalTime)
        {

            selected["Date"] = date.ToString();
            selected["Total Activity Time"] = totalTime;

            return selected;
        }

        public double averageActivityTime(List<double> durations)
        {
            return Math.Round(durations.Sum() / durations.Count, 1);
        }

        public struct Recommendation
        {
            public int min;
            public int max;
            public string msg;
        }
        public Recommendation PhysicalActivityCheck(int age)
        {
            Recommendation values = new Recommendation();

            if (age == 1)
            {
                values.min = 0;
                values.max = 0;
                values.msg = "be physically active several times a day in a variety of ways, particularly through interactive floor-based play; more is better.";
            } else if (age == 2)
            {
                values.min = 180;
                values.max = -1;
                values.msg = "spend at least 180 minutes in a variety of types of physical activities at any intensity, including moderate- to vigorous-intensity physical activity, spread throughout the day; more is better";
            }else if (age == 3 || age == 4)
            {
                values.min = 180;
                values.max = -1;
                values.msg = "spend at least 180 minutes in a variety of types of physical activities at any intensity, of which at least 60 minutes is moderate- to vigorous-intensity physical activity, spread throughout the day; more is better;";
            }else if (age >= 5 && age <= 17)
            {
                values.min = 60;
                values.max = -1;
                values.msg = "should do at least an average of 60 minutes per day of moderate-to-vigorous intensity, mostly aerobic, physical activity, across the week.";
            }else if(age >= 18 && age <= 65)
            {
                values.min = 150;
                values.max = 300;
                values.msg = "should do at least 150–300 minutes of moderate-intensity aerobic physical activity";
            }else
            {
                values.min = 150;
                values.max = 300;
                values.msg = "should do at least 150–300 minutes of moderate-intensity aerobic physical activity.\nolder adults should do varied multicomponent physical activity that emphasizes functional balance and strength training at moderate or greater intensity, on 3 or more days a week";
            }

            return values;
        }

        //Activities
        public DataTable newActivityTable()
        {
            DataTable data = new DataTable("New Activity");

            //Date col
            DataColumn date = new DataColumn
            {
                ColumnName = "Date",
                DataType = typeof(System.DateTime)
            };
            data.Columns.Add(date);

            //Duration col
            DataColumn duration = new DataColumn
            {
                ColumnName = "Duration",
                DataType = typeof(double)
            };
            data.Columns.Add(duration);

            //Time Unit Col
            DataColumn timeUnit = new DataColumn
            {
                ColumnName = "TimeUnit",
                DataType = typeof(int)
                
            };
            data.Columns.Add(timeUnit);

            //Score col
            DataColumn score = new DataColumn
            {
                ColumnName = "Score",
                DataType = typeof(double)
            };
            data.Columns.Add(score);

            //Unit col
            DataColumn unit = new DataColumn
            {
                ColumnName = "Unit",
                DataType = typeof(string)
            };
            data.Columns.Add(unit);

            data.Rows.Add(DateTime.Today, 0.0, 1, 0.0, " ");

            data.DefaultView.Sort = "";

            return data;
        }


        public List<DataTable> loadList()
        {
            String path = appData + "/ActivityTracker/Activities";

            List<DataTable> l = new List<DataTable>();

            if ((System.IO.Directory.Exists(path)) && (System.IO.Directory.GetFiles(path).Length > 0))
            {
                String[] files = System.IO.Directory.GetFiles(path);


                for (int i = 0; i < files.Length; i++)
                {
                    DataTable data = new DataTable();
                    data.ReadXml(files[i]);

                    l.Add(data);
                }
            }
            return l;
        }


        public double getDalyActivityTime(List<DataTable> activities, DateTime date)
        {
            double t = 0;

            for (int actIdx = 0; actIdx < activities.Count; actIdx++)
            {
                DataRowCollection currentAct = activities[actIdx].Rows;

                for (int day = 0; day < currentAct.Count; day++)
                {
                    if (date.Date.Equals((DateTime)currentAct[day]["Date"]))
                    {
                        t += (double)currentAct[day]["Duration"];
                    }
                    
                }
            }

            return t;
        }

        public double getTotalActivityTime(DataTable data)
        {
            double t = 0;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                t += (double)data.Rows[i]["Total Activity Time"];
            }

            return t;
        }

        public DataRow updateMyActivity(DataRow selected, DateTime date, double duration, int timeUnit, double score, string unit)
        {
            selected["Date"] = date.ToString();
            selected["Duration"] = duration;
            selected["TimeUnit"] = timeUnit;
            selected["Score"] = score;
            selected["Unit"] = unit;

            return selected;
        }

        public int getUnitIndexFromValue(string unitString)
        {

            if (unitString == " ") { return 0; }
            if (unitString == "m") { return 1; }
            if (unitString == "km") { return 2; }
            if (unitString == "cycle") { return 3; }
            if (unitString == "series") { return 4; }

            return 0;
        }

        public double convertTime(double baseTime, int target, int mode)
        {
            double converted = 0;

            if (mode == 1)
            {
                if (target == 0) { converted =  baseTime * 60; }
                if (target == 1) { converted =  baseTime; }
                if (target == 2) { converted =  baseTime / 3600; }
            } else
            {
                if (target == 0) { converted = baseTime / 60; }
                if (target == 1) { converted = baseTime; }
                if (target == 2) { converted = baseTime * 3600; }
            }
            return converted;
            
        }

        //Saving
        public bool saveAll(DataTable myData, List<DataTable> activities, string path = "")
        {
            String myDataPath;
            String activityPath;

            if (path == "")
            {
                myDataPath = appData + "/ActivityTracker";
                activityPath = appData + "/ActivityTracker/Activities";
            }
            else
            {
                myDataPath = path + "/ActivityTracker";
                activityPath = path + "/ActivityTracker/Activities";
            }


            if (!(System.IO.Directory.Exists(myDataPath)))
            {
                System.IO.Directory.CreateDirectory(myDataPath);
            }

            if (!(System.IO.Directory.Exists(activityPath)))
            {
                System.IO.Directory.CreateDirectory(activityPath);
            }

            myData.WriteXml(myDataPath + "/myData.xml", XmlWriteMode.WriteSchema);

            int i; //xml file count

            for (i = 0; i < activities.Count; i++)//Loop for every datatable and add 1 to i, keeps track of how many files we have
            {
                DataTable act = activities[i];
                String actPath = activityPath + "/" + i.ToString() + ".xml";

                if (System.IO.File.Exists(actPath))
                {
                    System.IO.File.Delete(actPath);
                }

                act.WriteXml(actPath, XmlWriteMode.WriteSchema);
            }

            int xmlFilesCount = System.IO.Directory.GetFiles(activityPath).Length;

            if (xmlFilesCount > i)
            {
                for (int toDelete = i; toDelete < xmlFilesCount; toDelete++)
                {
                    System.IO.File.Delete(activityPath + "/" + toDelete.ToString() + ".xml");
                }
            }


            return true;
        }
    }   
}
