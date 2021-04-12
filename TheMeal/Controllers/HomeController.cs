using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TheMeal.Models;

namespace TheMeal.Controllers
{
    public class HomeController : Controller
    {
        string Baseurl = "https://www.themealdb.com/";
        string ConnStr = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionForMeals"].ConnectionString;
        public async Task<ActionResult> Index()
        {
            MealsCollection MealInfo = new MealsCollection();

            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Baseurl);

                client.DefaultRequestHeaders.Clear();
                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                HttpResponseMessage Res = await client.GetAsync("api/json/v1/1/search.php?f=a");

                //Checking the response is successful or not which is sent using HttpClient  
                if (Res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api   
                    var MealResponse = Res.Content.ReadAsStringAsync().Result;
                    
                    //Deserializing the response recieved from web api and storing into the Employee list  
                    MealInfo = JsonConvert.DeserializeObject<MealsCollection>(MealResponse);

                }
                //returning the Meal list to view  
                return View(MealInfo);
            }
        }
  
        [HttpGet]
        public async Task<ActionResult> SearchByMealName(MealsCollection MC)
        {
            MealsCollection MealInfo = new MealsCollection();

            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Baseurl);

                client.DefaultRequestHeaders.Clear();
                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service resource Meal details using HttpClient  
                HttpResponseMessage Res = await client.GetAsync("api/json/v1/1/search.php?s="+ HttpUtility.HtmlEncode(MC.singlemeal.strMeal));

                //Checking the response is successful or not which is sent using HttpClient  
                if (Res.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api   
                    var MealResponse = Res.Content.ReadAsStringAsync().Result;
                    
                    //Deserializing the response recieved from web api and storing into the Meal list  
                    MealInfo = JsonConvert.DeserializeObject<MealsCollection>(MealResponse);

                }
                
            //Inserting Data into the DataBase.    
            using (SqlConnection sqlconn = new SqlConnection(ConnStr))
            {

                    if (MealInfo.meals != null)
                    {
                        foreach (Meals item in MealInfo.meals)
                        {
                            string query = " IF NOT EXISTS(SELECT 1 FROM tblmeal WHERE id=N'" + item.idMeal + "')BEGIN INSERT INTO tblmeal values(N'" + item.idMeal + "',N'" + item.strMeal + "',N'" + replaceQuote(item.strInstructions) + "',N'"
                                 + item.strIngredient1 + "',N'"
                                 + item.strIngredient2 + "',N'"
                                 + item.strIngredient3 + "',N'"
                                 + item.strIngredient4 + "',N'"
                                 + item.strIngredient5 + "',N'"
                                 + item.strIngredient6 + "',N'"
                                 + item.strIngredient7 + "',N'"
                                 + item.strIngredient8 + "',N'"
                                 + item.strIngredient9 + "',N'"
                                 + item.strIngredient10 + "',N'"
                                 + item.strIngredient11 + "',N'"
                                 + item.strIngredient12 + "',N'"
                                 + item.strIngredient13 + "',N'"
                                 + item.strIngredient14 + "',N'"
                                 + item.strIngredient15 + "',N'"
                                 + item.strIngredient16 + "',N'"
                                 + item.strIngredient17 + "',N'"
                                 + item.strIngredient18 + "',N'"
                                 + item.strIngredient19 + "',N'"
                                 + item.strIngredient20 + "') END;";

                            SqlCommand sqlcom = new SqlCommand(query, sqlconn);
                            sqlconn.Open();
                            sqlcom.ExecuteNonQuery();
                            sqlconn.Close();
                        }
                    }
                    
                }
                //returning the Meal list to view  
                return View(MealInfo);
            }
        }

        private string replaceQuote(string str)
        {
            string strReplace = str.Replace("'", "''");
            return strReplace;
        }
   
        [HttpGet]
        public ActionResult GetRecipeByIngredient(MealsCollection MC)
        {
            MealsCollection MealInfo = new MealsCollection();
            DataSet ds = new DataSet();
            using (SqlConnection sqlconn = new SqlConnection(ConnStr))
            {
                String query = " SELECT * FROM tblmeal WITH(NOLOCK) WHERE ";
                int count = 1;
                if (MC.singlemeal.strIngredient1 != null && MC.singlemeal.strIngredient2 != null)
                {
                    if (MC.singlemeal.strIngredient1 != null)//Check If "Search By Ingrdients Name" is not empty
                    {
                        query += " ( ";
                        foreach (string str in MC.singlemeal.strIngredient1.Split(','))
                        {
                            string strReplace = replaceQuote(str);
                            query += getFilter(strReplace, 1);

                            if (count != MC.singlemeal.strIngredient1.Split(',').Length)
                            {
                                query += " OR ";
                            }
                            else
                            {
                                query += " ) ";
                            }
                            count++;
                        }
                    }
                    if (MC.singlemeal.strIngredient2 != null)//Check If "Exclude Ingrdients" is not empty
                    {
                        count = 1;
                        if (MC.singlemeal.strIngredient1 != null)
                            query += " AND ";
                        foreach (string str in MC.singlemeal.strIngredient2.Split(','))
                        {
                            string strReplace = replaceQuote(str);
                            query += getFilter(strReplace, 2);

                            if (count != MC.singlemeal.strIngredient2.Split(',').Length)
                            {
                                query += " AND ";
                            }
                            count++;
                        }
                    }

                }
                else
                {//returns nothing
                    query += " 1=2;";
                }
                //Fetching Data from the Database
                SqlDataAdapter sde = new SqlDataAdapter(query, sqlconn);
                sde.Fill(ds);
                MealsCollection objMC = new MealsCollection();
                DataTable dt = ds.Tables[0];
                //returning the Meal list to view  
                return View(ds);
            }
        }
        private string getFilter(string str,int callFrom)
        {
            //Function to create where clause to search based on ingredients.
            string strFilter = string.Empty;
            
            for (int i = 1; i <= 20; i++)
            {
                strFilter += " Ingredient" + i;
                if (callFrom == 2)
                {
                    strFilter += " NOT ";
                }
                strFilter += " LIKE '%" + str + "%' ";
                if (i != 20)
                {
                    if(callFrom==1)
                        strFilter += " OR ";
                    else
                        strFilter += " AND ";
                }
            }
            return strFilter;
        }
    }
}