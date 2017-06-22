using MecMauritius.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MecMauritius.Controllers
{
    
    public static class SchoolService
    {
        public static SchoolModel GenerateSchools(string zone_Id)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString;

            SchoolModel schoolModel = new SchoolModel();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("spFetchSchools", connection))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@zone_id", zone_Id);
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    schoolModel.Schoollist = new Collection<School>();

                    // Set the default school value
                    schoolModel.Schoollist.Add(new School { Name = "Select School", Id = "0" });
                    
                    // Set all the zones in the dropdown
                    while (reader.Read())
                    {
                        string name = string.Format(CultureInfo.InvariantCulture, reader.GetValue(1).ToString());
                        string id = string.Format(CultureInfo.InvariantCulture, reader.GetValue(0).ToString());

                        schoolModel.Schoollist.Add(new School { Name= name, Id = id });
                    }
                }
            }

            if (schoolModel.Schoollist.Count > 0)
                schoolModel.Selected = 0;

            return schoolModel;
        }
    }
}