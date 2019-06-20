using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;  
using System.Configuration;
using System.Reflection;

using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.Data;
using OncologyAppService.Data;
using System.Web.Script.Services;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace OncologyAppService
{
    /// <summary>
    /// Summary description for WebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService : System.Web.Services.WebService
    {



        #region WHEN-TO-CALL
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetSymptomInfo()
        {
            string query = @"select id, symptom, type, detail from tbl_symptom order by id asc";
            List<TblSymptom> list = new List<TblSymptom>();

            using (SqlConnection con = new SqlConnection(Global.CONN_STRING))
            {
                try
                {
                    list.Clear();

                    con.Open();

                    SqlCommand command = new SqlCommand(query, con);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        TblSymptom item = new TblSymptom();

                        item.Id = Convert.ToInt64(reader["id"].ToString().Trim());
                        item.Symptom = reader["symptom"].ToString().Trim();
                        item.Type = reader["type"].ToString().Trim();
                        item.Detail = reader["detail"].ToString().Trim();

                        list.Add(item);
                    }

                    con.Close();

                    Global.QUERY_EXECUTION_RESP = "Success";
                }
                catch (Exception exp)
                {
                    Global.QUERY_EXECUTION_RESP = exp.Message.ToString();
                }

            }


            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(list));
        }
        #endregion



        #region SSPEDI

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void SaveDailyRating()
        {


            //Extraction data from POST request
            int data_exist = 0;
            StreamReader stream = new StreamReader(HttpContext.Current.Request.InputStream);
            String data = stream.ReadToEnd();

            //data = "user_id=1&disappointed=1&scared=1&cranky=1"; //for testing

            Int64[] points = { 0, 0, 0 };
            DateTime dateTime = DateTime.Now;
            Int64 user_id = 0;
            String[] split_data = data.Split('&');
            for (int i = 0; i < split_data.Length; i++)
            {
                String temp = split_data[i];
                String[] rating = temp.Split('=');
                if (rating[0] == "disappointed")
                {
                    points[0] = Convert.ToInt64(rating[1]);
                }
                else if (rating[0] == "scared")
                {
                    points[1] = Convert.ToInt64(rating[1]);
                }
                else if (rating[0] == "cranky")
                {
                    points[2] = Convert.ToInt64(rating[1]);
                }
                else if (rating[0] == "user_id")
                {
                    user_id = Convert.ToInt64(rating[1]);
                }
            }




            //check if entry already exits
            using (SqlConnection con = new SqlConnection(Global.CONN_STRING))
            {
                try
                {

                    con.Open();

                    String dt1 = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + " 00:00:00.000";
                    String dt2 = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + " 23:59:59.000";

                    using (SqlCommand command = new SqlCommand("select * from tbl_daily_rating where user_id=" + user_id + " and date_recorded between '" + dt1 + "' and '" + dt2 + "'", con))
                    {
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            data_exist++;
                        }
                        else
                        {
                            data_exist = 0;
                        }
                    }

                    con.Close();

                    //Global.QUERY_EXECUTION_RESP = "Success";
                }
                catch (Exception exp)
                {
                    //Global.QUERY_EXECUTION_RESP = exp.Message.ToString().Trim();
                }

            }




            if (data_exist == 0)
            {
                string query = @"insert into tbl_daily_rating (date_recorded, rating_disappointed, rating_scared, rating_cranky, user_id) 
                            values (@date_recorded, @rating_disappointed, @rating_scared, @rating_cranky, @user_id)";

                DataTable dt = new DataTable("dt");
                dt.Columns.Add("date_recorded"); dt.Columns.Add("rating_disappointed"); dt.Columns.Add("rating_scared");
                dt.Columns.Add("rating_cranky"); dt.Columns.Add("user_id");

                DataRow row = dt.NewRow();
                row["date_recorded"] = dateTime; row["rating_disappointed"] = points[0];
                row["rating_scared"] = points[1]; row["rating_cranky"] = points[2]; row["user_id"] = user_id;


                dt.Rows.Add(row);
                dt.TableName = "dt";


                using (SqlConnection con = new SqlConnection(Global.CONN_STRING))
                {
                    try
                    {

                        con.Open();

                        using (SqlCommand command = new SqlCommand(query, con))
                        {
                            foreach (DataRow datarow in dt.Rows)
                            {
                                command.Parameters.AddWithValue("@date_recorded", Convert.ToDateTime(datarow["date_recorded"]));
                                command.Parameters.AddWithValue("@rating_disappointed", Convert.ToInt32(datarow["rating_disappointed"]));
                                command.Parameters.AddWithValue("@rating_scared", Convert.ToInt32(datarow["rating_scared"]));
                                command.Parameters.AddWithValue("@rating_cranky", Convert.ToInt32(datarow["rating_cranky"]));
                                command.Parameters.AddWithValue("@user_id", Convert.ToInt32(datarow["user_id"]));

                                break;
                            }

                            command.ExecuteNonQuery();
                        }

                        con.Close();

                        Global.QUERY_EXECUTION_RESP = "Success";
                    }
                    catch (Exception exp)
                    {
                        Global.QUERY_EXECUTION_RESP = exp.Message.ToString().Trim();
                    }

                }
            }
            else if (data_exist > 0)
            {
                Global.QUERY_EXECUTION_RESP = "Duplicate";
            }


            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(Global.QUERY_EXECUTION_RESP.Trim()));

        }




        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetDailyRating()
        {

            Int64 user_id = 0;

            StreamReader stream = new StreamReader(HttpContext.Current.Request.InputStream);
            String data = stream.ReadToEnd();

            data = "user_id=1"; //testing

            String[] split_data = data.Split('=');
            if (split_data[0] == "user_id")
            {
                user_id = Convert.ToInt64(split_data[1]);
            }



            string query = @"select * from tbl_daily_rating where user_id=" + user_id + " order by date_recorded asc";
            List<TblSSpedi> list = new List<TblSSpedi>();

            using (SqlConnection con = new SqlConnection(Global.CONN_STRING))
            {
                try
                {
                    list.Clear();

                    con.Open();

                    SqlCommand command = new SqlCommand(query, con);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        TblSSpedi item = new TblSSpedi();

                        item.Id = Convert.ToInt64(reader["id"].ToString().Trim());
                        DateTime temp = Convert.ToDateTime(reader["date_recorded"].ToString());
                        item.Date = temp.Day.ToString() + "-" + temp.Month.ToString() + "-" + temp.Year.ToString();

                        string[] weekdays = { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" };
                        temp = new DateTime(temp.Year, temp.Month, temp.Day);
                        item.Day = weekdays[((int) temp.DayOfWeek)];
                        item.Disappointed = Convert.ToInt64(reader["rating_disappointed"]);
                        item.Scared = Convert.ToInt64(reader["rating_scared"]);
                        item.Cranky = Convert.ToInt64(reader["rating_cranky"]);

                        list.Add(item);
                    }

                    con.Close();

                    Global.QUERY_EXECUTION_RESP = "Success";
                }
                catch (Exception exp)
                {
                    Global.QUERY_EXECUTION_RESP = exp.Message.ToString();
                }

            }


            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(list));



        }


        #endregion





        #region GOAL
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void SaveNewGoal()
        {


            //Extraction data from POST request
            Int64 user_id = 0;
            string completion_date = "";
            string date_completed = "1800-01-01 00:00:00.000";
            string goal = "";
            string date_created = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + " 00:00:00.000";
            int complete = 0;
            int notified = 0;
            Int64 points = 0;

            StreamReader stream = new StreamReader(HttpContext.Current.Request.InputStream);
            String data = stream.ReadToEnd();

            //data = "user_id=1&completion_date=2019-04-30&goal=sing+a+song+weekly"; //for testing

            String[] split_data = data.Split('&');
            for (int i = 0; i < split_data.Length; i++)
            {
                String temp = split_data[i];
                String[] details = temp.Split('=');
                if (details[0] == "user_id")
                {
                    user_id = Convert.ToInt64(details[1].Trim());
                }
                else if (details[0] == "completion_date")
                {
                    completion_date = details[1].Trim() + " 00:00:00.000";
                }
                else if (details[0] == "goal")
                {
                    string temp2 = details[1].Replace('+', ' ');
                    goal = temp2;
                }
            }

            if (user_id > 0 && goal != "" && completion_date != "")
            {


                string query = @"insert into tbl_goal (goal, completion_date, user_id, complete, date_created, notified, points, date_completed) 
                            values ('" + goal + "', '" + completion_date + "', " + user_id + ", " + complete + ", '" + date_created + "', " + notified + ", " + points + ", '" + date_completed + "')";


                using (SqlConnection con = new SqlConnection(Global.CONN_STRING))
                {
                    try
                    {

                        con.Open();

                        using (SqlCommand command = new SqlCommand(query, con))
                        {

                            command.ExecuteNonQuery();
                        }

                        con.Close();

                        Global.QUERY_EXECUTION_RESP = "Success";
                    }
                    catch (Exception exp)
                    {
                        Global.QUERY_EXECUTION_RESP = exp.Message.ToString().Trim();
                    }

                }


            }
            else
            {
                Global.QUERY_EXECUTION_RESP = "Error in sent data, try again.";
            }







            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(Global.QUERY_EXECUTION_RESP.Trim()));

        }



        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetGoalInfo()
        {
            //Extraction data from POST request
            Int64 user_id = 0;
            StreamReader stream = new StreamReader(HttpContext.Current.Request.InputStream);
            String data = stream.ReadToEnd();

            //data = "user_id=1"; //testing

            String[] split_data = data.Split('=');
            if (split_data[0] == "user_id")
            {
                user_id = Convert.ToInt64(split_data[1]);
            }



            string query = @"select id, goal, completion_date, complete, date_completed from tbl_goal where user_id=" + user_id;
            List<TblGoal> list = new List<TblGoal>();

            using (SqlConnection con = new SqlConnection(Global.CONN_STRING))
            {
                try
                {
                    list.Clear();

                    con.Open();

                    SqlCommand command = new SqlCommand(query, con);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        TblGoal item = new TblGoal();

                        item.Id = Convert.ToInt64(reader["id"].ToString().Trim());
                        item.Goal = reader["goal"].ToString().Trim();
                        item.CompletionDate = reader["completion_date"].ToString().Trim();
                        item.Complete = Convert.ToInt32(reader["complete"]);
                        item.DateCompleted = reader["date_completed"].ToString().Trim(); //default is: 1800-01-01 00:00:00.000

                        list.Add(item);
                    }

                    con.Close();

                    Global.QUERY_EXECUTION_RESP = "Success";
                }
                catch (Exception exp)
                {
                    Global.QUERY_EXECUTION_RESP = exp.Message.ToString();
                }

            }


            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(list));
        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void ChangeGoalToComplete()
        {
            //Extraction data from POST request
            Int64 goal_id = 0;
            String date_completed = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + " 00:00:00.000";
            int isComplete = 1;

            StreamReader stream = new StreamReader(HttpContext.Current.Request.InputStream);
            String data = stream.ReadToEnd();

            //data = "goal_id=1"; //testing

            String[] split_data = data.Split('=');
            if (split_data[0] == "goal_id")
            {
                goal_id = Convert.ToInt64(split_data[1]);
            }



            string query = @"update tbl_goal set complete=" + isComplete + ", date_completed= '" + date_completed + "' where id=" + goal_id;


            using (SqlConnection con = new SqlConnection(Global.CONN_STRING))
            {
                try
                {
                    con.Open();

                    SqlCommand command = new SqlCommand(query, con);

                    command.ExecuteNonQuery();

                    con.Close();

                    Global.QUERY_EXECUTION_RESP = "Success";
                }
                catch (Exception exp)
                {
                    Global.QUERY_EXECUTION_RESP = exp.Message.ToString();
                }

            }


            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(Global.QUERY_EXECUTION_RESP.Trim()));
        }



        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void ChangeGoalToIncomplete()
        {
            //Extraction data from POST request
            Int64 goal_id = 0;
            String date_completed = "1800-01-01 00:00:00.000"; //changing to a default invalida date
            int isComplete = 0; //change to incomplete

            StreamReader stream = new StreamReader(HttpContext.Current.Request.InputStream);
            String data = stream.ReadToEnd();

            //data = "goal_id=1"; //testing

            String[] split_data = data.Split('=');
            if (split_data[0] == "goal_id")
            {
                goal_id = Convert.ToInt64(split_data[1]);
            }



            string query = @"update tbl_goal set complete=" + isComplete + ", date_completed= '" + date_completed + "' where id=" + goal_id;


            using (SqlConnection con = new SqlConnection(Global.CONN_STRING))
            {
                try
                {
                    con.Open();

                    SqlCommand command = new SqlCommand(query, con);

                    command.ExecuteNonQuery();

                    con.Close();

                    Global.QUERY_EXECUTION_RESP = "Success";
                }
                catch (Exception exp)
                {
                    Global.QUERY_EXECUTION_RESP = exp.Message.ToString();
                }

            }


            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(Global.QUERY_EXECUTION_RESP.Trim()));
        }

        #endregion





        #region SETTING

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetBloodResult()
        {

            Int64 user_id = 0;

            StreamReader stream = new StreamReader(HttpContext.Current.Request.InputStream);
            String data = stream.ReadToEnd();

            data = "user_id=1"; //testing

            String[] split_data = data.Split('=');
            if (split_data[0] == "user_id")
            {
                user_id = Convert.ToInt64(split_data[1]);
            }



            string query = @"select * from tbl_blood_result where user_id=" + user_id + " order by date_created asc";
            List<TblBloodResult> list = new List<TblBloodResult>();

            using (SqlConnection con = new SqlConnection(Global.CONN_STRING))
            {
                try
                {
                    list.Clear();

                    con.Open();

                    SqlCommand command = new SqlCommand(query, con);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        TblBloodResult item = new TblBloodResult();

                        item.Id = Convert.ToInt64(reader["id"].ToString().Trim());
                        item.Weight = Convert.ToInt64(reader["weight"].ToString().Trim());
                        item.Height = Convert.ToInt64(reader["height"].ToString().Trim());
                        item.Platelets = Convert.ToInt64(reader["hb"].ToString().Trim());
                        item.Hb = Convert.ToInt64(reader["platelets"].ToString().Trim());
                        //item.Wcc = Convert.ToDouble(reader["wcc"].ToString().Trim());
                        //item.Neutrophils = Convert.ToDouble(reader["neutrophils"].ToString().Trim());
                        item.Date = reader["date_created"].ToString().Trim();

                        DateTime temp = Convert.ToDateTime(reader["date_created"].ToString());
                        string[] weekdays = { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" };
                        temp = new DateTime(temp.Year, temp.Month, temp.Day);
                        item.Day = weekdays[((int)temp.DayOfWeek)];




                        list.Add(item);
                    }

                    con.Close();

                    Global.QUERY_EXECUTION_RESP = "Success";
                }
                catch (Exception exp)
                {
                    Global.QUERY_EXECUTION_RESP = exp.Message.ToString();
                }

            }


            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(list));



        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetWebsiteInfo()
        {
            



            string query = @"select * from tbl_website";
            List<TblWebsite> list = new List<TblWebsite>();

            using (SqlConnection con = new SqlConnection(Global.CONN_STRING))
            {
                try
                {
                    list.Clear();

                    con.Open();

                    SqlCommand command = new SqlCommand(query, con);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        TblWebsite item = new TblWebsite();

                        item.Id = Convert.ToInt64(reader["id"].ToString().Trim());
                        item.Website = reader["name"].ToString().Trim();
                        item.Url = reader["url"].ToString().Trim();

                        list.Add(item);
                    }

                    con.Close();

                    Global.QUERY_EXECUTION_RESP = "Success";
                }
                catch (Exception exp)
                {
                    Global.QUERY_EXECUTION_RESP = exp.Message.ToString();
                }

            }


            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(list));
        }





        #endregion




        #region PROVIDERS

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetProviderInfo()
        {
            string query = @"select * from tbl_provider order by id asc";
            List<TblProvider> list = new List<TblProvider>();

            using (SqlConnection con = new SqlConnection(Global.CONN_STRING))
            {
                try
                {
                    list.Clear();

                    con.Open();

                    SqlCommand command = new SqlCommand(query, con);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        TblProvider item = new TblProvider();

                        item.Id = Convert.ToInt64(reader["id"].ToString().Trim());
                        item.Name = reader["name"].ToString().Trim();
                        item.Address = reader["address"].ToString().Trim();
                        item.Latitude = Convert.ToDouble(reader["loc_lat"]);
                        item.Longitude = Convert.ToDouble(reader["loc_long"]);
                        item.Website = reader["website"].ToString().Trim();
                        item.Type = reader["type"].ToString().Trim();

                        list.Add(item);
                    }

                    con.Close();

                    Global.QUERY_EXECUTION_RESP = "Success";
                }
                catch (Exception exp)
                {
                    Global.QUERY_EXECUTION_RESP = exp.Message.ToString();
                }

            }


            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(list));
        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetProviderContact()
        {

            Int64 provider_id = 0;

            StreamReader stream = new StreamReader(HttpContext.Current.Request.InputStream);
            String data = stream.ReadToEnd();

            //data = "goal_id=1"; //testing

            String[] split_data = data.Split('=');
            if (split_data[0] == "provider_id")
            {
                provider_id = Convert.ToInt64(split_data[1]);
            }



            string query = @"select * from tbl_contact where provider_id="+ provider_id + " order by id asc";
            List<TblContact> list = new List<TblContact>();

            using (SqlConnection con = new SqlConnection(Global.CONN_STRING))
            {
                try
                {
                    list.Clear();

                    con.Open();

                    SqlCommand command = new SqlCommand(query, con);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        TblContact item = new TblContact();

                        item.Id = Convert.ToInt64(reader["id"].ToString().Trim());
                        item.Name = reader["name"].ToString().Trim();
                        item.Phone = reader["phone"].ToString().Trim();
                        item.Email = reader["email"].ToString().Trim();
                        item.Location = reader["location"].ToString().Trim();
                        item.Note = reader["note"].ToString().Trim();
                        item.Type = reader["contact_type"].ToString().Trim();
                        item.ProviderId = Convert.ToInt64(reader["provider_id"].ToString().Trim());

                        list.Add(item);
                    }

                    con.Close();

                    Global.QUERY_EXECUTION_RESP = "Success";
                }
                catch (Exception exp)
                {
                    Global.QUERY_EXECUTION_RESP = exp.Message.ToString();
                }

            }


            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(list));



        }


        #endregion










    }
}
