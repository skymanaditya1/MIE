
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Globalization;

namespace MecMauritius
{
    /// <summary>
    /// Summary description for ReviewHandler
    /// </summary>
    public class ReviewHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            // context.Response.Write("Hello World");
            float Relevance = float.Parse(context.Request.Form["Relevance"], CultureInfo.InvariantCulture.NumberFormat);
            float Adaptability = float.Parse(context.Request.Form["Adaptability"], CultureInfo.InvariantCulture.NumberFormat);
            float Language = float.Parse(context.Request.Form["Language"], CultureInfo.InvariantCulture.NumberFormat);
            float Pedagogy = float.Parse(context.Request.Form["Pedagogy"], CultureInfo.InvariantCulture.NumberFormat);
            float Design = float.Parse(context.Request.Form["Design"], CultureInfo.InvariantCulture.NumberFormat);

            
            float total = Relevance + Adaptability + Language + Pedagogy + Design;

            String review = context.Request.Form["reviewText"].ToString();
            String id = context.Request.Form["resourceId"].ToString();
            float currRating=0.0f, noOfRatings=0.0f;
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                
                string fetchcurrentrating = @"SELECT [Ratings],[NoOfRatings]
                                            FROM [dbo].[DigitalResources]
                                            WHERE [ID]=" + id + ";";
                SqlCommand cmd = new SqlCommand("spFetchCurrentRating", connection);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", id);


                connection.Open();              

                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        currRating = float.Parse(reader.GetValue(0).ToString(), CultureInfo.InvariantCulture.NumberFormat);
                        noOfRatings = float.Parse(reader.GetValue(1).ToString(), CultureInfo.InvariantCulture.NumberFormat);                          
                        
                    }
                }
                noOfRatings++;
                currRating = ((currRating * (noOfRatings-1)) + total/5) / noOfRatings;

                string updatecurrentrating = @"UPDATE [dbo].[DigitalResources]
                                               SET [Ratings] = " + currRating +
                                                  ",[NoOfRatings] = " + noOfRatings +
                                               "WHERE [ID]= " + id + ";";


                cmd = new SqlCommand(updatecurrentrating, connection);
                cmd.ExecuteNonQuery();
            }


            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                // Current time required or the Resource_Posted field in ResourceReviews
                DateTime currentTime = DateTime.Now;
                string query = "INSERT INTO ResourceReviews (User_ID, Resource_ID, Resource_Description, Resource_Posted, Relevance, Adaptability, Language, Pedagogy, Design, Total_Rating) VALUES (@userid, @resourceid, @review, @currentTime, @relevance, @adaptability, @language, @pedagogy, @design, @total)";

                connection.Open();
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@userid", System.Web.HttpContext.Current.User.Identity.GetUserId());
                cmd.Parameters.AddWithValue("@resourceid", id);
                cmd.Parameters.AddWithValue("@review", review);
                cmd.Parameters.AddWithValue("@relevance", Relevance);
                cmd.Parameters.AddWithValue("@currentTime", currentTime);
                cmd.Parameters.AddWithValue("@adaptability", Adaptability);
                cmd.Parameters.AddWithValue("@language", Language);
                cmd.Parameters.AddWithValue("@pedagogy", Pedagogy);
                cmd.Parameters.AddWithValue("@design", Design);
                cmd.Parameters.AddWithValue("@total", total);
                cmd.ExecuteNonQuery(); // Inserts the review
            }

            // Redirect to the resource page
            // return RedirectToAction("ResourceDisplay", new { ID = id });
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}



