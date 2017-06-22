using MecMauritius.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Configuration;

namespace MecMauritius.Controllers
{
    public class CategoriesService : Controller
    {
        public static CategoriesModel GetAllCategories()
        {
            CategoriesModel categoriesModel = new CategoriesModel();
            categoriesModel.categories = new Collection<Category>();
            categoriesModel.categories.Add(new Category
            {
                ID = "0",
                Name = "Select Category"
            });
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spGetAllCategories", sqlConnection))
                {
                    sqlConnection.Open();
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categoriesModel.categories.Add(new Category
                            {
                                ID = reader.GetValue(0).ToString(),
                                Name = reader.GetValue(1).ToString()
                            });
                        }
                    }
                }
            }
            if (categoriesModel.categories.Count > 0)
                categoriesModel.Selected = 0;

            return categoriesModel;
        }
    }
}