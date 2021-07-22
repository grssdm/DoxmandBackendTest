using System.Collections.Generic;
using DoxmandBackend.Common;
using DoxmandBackend.DTOs;
using DoxmandBackend.Models;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DoxmandBackend.Repos
{
    public class DoxmandRepo : IUsersRepo, IProductRepo, IPlanRepo
    {
        private readonly IFirebaseConfig _config = new FirebaseConfig
        {
            // Service Accounts -> Database secrets
            AuthSecret = "XOTyqL2pDHAUw3no957X7Oj0gGNgVFNgsJhnGKpx",
            // General -> Database link
            BasePath = "https://asp-testing-f6b86-default-rtdb.europe-west1.firebasedatabase.app/"
        };
    
        private IFirebaseClient _client;

        #region USERS
        public IEnumerable<User> GetAllUsers()
        {
            _client = new FirebaseClient(_config);
            var response = _client.Get("Users");
            
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

            if (data == null)
            {
                return null;
            }
            
            var list = new List<User>();
            
            foreach (var user in data)
            {
                list.Add(JsonConvert.DeserializeObject<User>(((JProperty) user).Value.ToString()));
            }

            return list;
        }

        public User GetUserById(string userId)
        {
            _client = new FirebaseClient(_config);
            var response = _client.Get($"Users/{userId}");
            
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

            if (data == null)
            {
                return null;
            }
            
            var user = JsonConvert.DeserializeObject<User>(((JObject) data).ToString());
            
            return user;
        }
        
        /*
        public User AddUserToFirebase(UserDTO userDto)
        {
            _client = new FirebaseClient(_config);

            User user = new User(userDto.Email, CommonMethods.EncryptPassword(userDto.Password), userDto.Username);

            var response = _client.Push("Users/", user);
            user.User_ID = response.Result.name;
            _client.Set($"Users/{user.User_ID}", user);

            return user;
        }
        */

        public User EditUser(User user)
        {
            _client = new FirebaseClient(_config);

            _client.Set($"Users/{user.User_ID}", user);

            return user;
        }

        public bool AddProductToUser(User user, Product product)
        {
            _client = new FirebaseClient(_config);

            bool conflict = false;

            foreach (var userProduct in user.Products)
            {
                if (product.Room_ID == userProduct.Room_ID)
                {
                    product.Room_ID++;
                    conflict = true;
                }
            }
            
            user.Products.Add(product);

            _client.Set($"Users/{user.User_ID}", user);

            return conflict;
        }

        public void DeleteUser(User user)
        {
            _client = new FirebaseClient(_config);

            if (user.Plans != null)
            {
                foreach (var plan in user.Plans)
                {
                    DeletePlanById(plan.Plan_ID);
                }
            }

            if (user.Products != null)
            {
                foreach (var product in user.Products)
                {
                    DeleteProductById(product.Product_ID);
                }
            }
            
            _client.Delete($"Users/{user.User_ID}");
        }

        public void DeleteProductFromUser(User user, string productId)
        {
            _client = new FirebaseClient(_config);

            int idx;

            if (user.Plans != null)
            {
                foreach (var plan in user.Plans)
                {
                    idx = -1;

                    for (int i = 0; i < plan.PlacedProducts.Count; i++)
                    {
                        if (plan.PlacedProducts[i].Product.Product_ID == productId)
                        {
                            idx = i;
                            break;
                        }
                    }

                    if (idx != -1)
                    {
                        plan.PlacedProducts.RemoveAt(idx);
                        EditPlan(plan);
                    }
                }
            }

            idx = -1;

            for (int i = 0; i < user.Products.Count; i++)
            {
                if (user.Products[i].Product_ID == productId)
                {
                    idx = i;
                    break;
                }
            }

            if (idx != -1)
            {
                user.Products.RemoveAt(idx);
                EditUser(user);
            }

            _client.Delete($"Products/{productId}");
        }

        public void DeletePlanFromUser(User user, string planId)
        {
            _client = new FirebaseClient(_config);

            int idx = -1;

            if (user.Plans != null)
            {
                for (int i = 0; i < user.Plans.Count; i++)
                {
                    if (user.Plans[i].Plan_ID == planId)
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx != -1)
                {
                    user.Plans.RemoveAt(idx);
                    EditUser(user);
                }
            }

            _client.Delete($"Plans/{planId}");
        }
        #endregion

        #region PRODUCTS
        public IEnumerable<Product> GetAllProducts()
        {
            _client = new FirebaseClient(_config);
            var response = _client.Get("Products");
            
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

            if (data == null)
            {
                return null;
            }
            
            var list = new List<Product>();
            
            foreach (var product in data)
            {
                list.Add(JsonConvert.DeserializeObject<Product>(((JProperty) product).Value.ToString()));
            }

            return list;
        }

        public Product GetProductById(string productId)
        {
            _client = new FirebaseClient(_config);
            var response = _client.Get($"Products/{productId}");
            
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

            if (data == null)
            {
                return null;
            }
            
            var product = JsonConvert.DeserializeObject<Product>(((JObject) data).ToString());
            
            return product;
        }

        public Product EditProduct(Product product)
        {
            _client = new FirebaseClient(_config);

            _client.Set($"Products/{product.Product_ID}", product);

            return product;
        }

        public void DeleteProductById(string productId)
        {
            _client = new FirebaseClient(_config);

            var plans = GetAllPlans();

            if (plans != null)
            {
                foreach (var plan in plans)
                {
                    int idx = -1;

                    for (int i = 0; i < plan.PlacedProducts.Count; i++)
                    {
                        if (plan.PlacedProducts[i].Product.Product_ID == productId)
                        {
                            idx = i;
                            break;
                        }
                    }

                    if (idx != -1)
                    {
                        plan.PlacedProducts.RemoveAt(idx);
                        EditPlan(plan);
                    }
                }
            }

            var users = GetAllUsers();

            if (users != null)
            {
                foreach (var user in users)
                {
                    int idx = -1;

                    for (int i = 0; i < user.Products.Count; i++)
                    {
                        if (user.Products[i].Product_ID == productId)
                        {
                            idx = i;
                            break;
                        }
                    }

                    if (idx != -1)
                    {
                        user.Products.RemoveAt(idx);
                        EditUser(user);
                        break;
                    }
                }
            }

            _client.Delete($"Products/{productId}");
        }

        public Product AddProductToFirebase(ProductDTO productDto)
        {
            _client = new FirebaseClient(_config);

            Product product = new Product(productDto);
            
            var response = _client.Push("Products/", productDto);
            product.Product_ID = response.Result.name;
            _client.Set($"Products/{product.Product_ID}", product);

            return product;
        }

        public IEnumerable<Product> GetBasicProducts()
        {
            List<Product> basicProducts = new List<Product> {
                new Product("Doxmand Normal ultrahangos vadriasztó", 1, Product.AlarmType.Wild, "https://doxmand.hu/wp-content/uploads/2019/12/Normal.jpg", "", -1, "https://firebasestorage.googleapis.com/v0/b/asp-testing-f6b86.appspot.com/o/MapIcons%2FNormalVR.png?alt=media"),
                new Product("Doxmand Dual ultrahangos vadriasztó", 2, Product.AlarmType.Wild, "https://doxmand.hu/wp-content/uploads/2021/06/Dual.jpg", "", -1, "https://firebasestorage.googleapis.com/v0/b/asp-testing-f6b86.appspot.com/o/MapIcons%2FNormalDualVR.png?alt=media"),
                new Product("Doxmand VR4 ultrahangos vadriasztó", 4, Product.AlarmType.Wild, "https://doxmand.hu/wp-content/uploads/2021/06/Vr4.jpg", "", -1, "https://firebasestorage.googleapis.com/v0/b/asp-testing-f6b86.appspot.com/o/MapIcons%2FVR4.png?alt=media"),
                new Product("Doxmand VR8 ultrahangos vadriasztó", 8, Product.AlarmType.Wild, "https://doxmand.hu/wp-content/uploads/2020/01/VR8.jpg", "", -1, "https://firebasestorage.googleapis.com/v0/b/asp-testing-f6b86.appspot.com/o/MapIcons%2FVR8.png?alt=media"),
                new Product("Doxmand Nomus ultrahangos rágcsálóriasztó", 1, Product.AlarmType.Rodent, "https://doxmand.hu/wp-content/uploads/2021/06/Nomus.jpg", "", -1, "https://firebasestorage.googleapis.com/v0/b/asp-testing-f6b86.appspot.com/o/MapIcons%2FNomusRR.png?alt=media"),
                new Product("Doxmand Nomus Plus ultrahangos rágcsálóriasztó", 1, Product.AlarmType.Rodent, "https://doxmand.hu/wp-content/uploads/2021/06/Nomus-Plus.jpg", "", -1, "https://firebasestorage.googleapis.com/v0/b/asp-testing-f6b86.appspot.com/o/MapIcons%2FNomusPlusRR.png?alt=media"),
                new Product("Doxmand Nomus 360 ultrahangos rágcsálóriasztó", 1, Product.AlarmType.Rodent, "https://doxmand.hu/wp-content/uploads/2021/06/Nomus-Plus-360psd.jpg", "", -1, "https://firebasestorage.googleapis.com/v0/b/asp-testing-f6b86.appspot.com/o/MapIcons%2FNomusPlus360RR.png?alt=media"),
                new Product("Doxmand SR2 madárriasztó", 2, Product.AlarmType.Starling, "https://doxmand.hu/wp-content/uploads/2021/06/SR2-1.jpg", "", -1, "https://firebasestorage.googleapis.com/v0/b/asp-testing-f6b86.appspot.com/o/MapIcons%2FSR2.png?alt=media"),
                new Product("Doxmand SR4 madárriasztó", 4, Product.AlarmType.Starling, "https://doxmand.hu/wp-content/uploads/2021/06/SR4-1.jpg", "", -1, "https://firebasestorage.googleapis.com/v0/b/asp-testing-f6b86.appspot.com/o/MapIcons%2FSR4.png?alt=media"),
            };

            return basicProducts;
        }

        public int NumberOfProductInPlans(string productId)
        {
            var plans = GetAllPlans();

            if (plans == null)
            {
                return 0;
            }

            int counter = 0;

            foreach (var plan in plans)
            {
                foreach (var placedProduct in plan.PlacedProducts)
                {
                    if (placedProduct.Product.Product_ID == productId)
                    {
                        counter++;
                    }
                }
            }

            return counter;
        }

        public Product CheckUserProductsForName(User user, string name)
        {
            foreach (var product in user.Products)
            {
                if (product.SavedName == name)
                {
                    return product;
                }
            }

            return null;
        }
        #endregion

        #region PLANS
        public bool AddPlanToUser(User user, Plan plan)
        {
            _client = new FirebaseClient(_config);

            bool conflict = false;

            foreach (var userPlan in user.Plans)
            {
                if (plan.Room_ID == userPlan.Room_ID)
                {
                    plan.Room_ID++;
                    conflict = true;
                }
            }

            user.Plans.Add(plan);

            _client.Set($"Users/{user.User_ID}", user);

            return conflict;
        }

        public Plan AddPlanToFirebase(PlanDTO planDto)
        {
            _client = new FirebaseClient(_config);

            Plan plan = new Plan(planDto);

            var response = _client.Push("Plans/", plan);
            plan.Plan_ID = response.Result.name;
            _client.Set($"Plans/{plan.Plan_ID}", plan);

            return plan;
        }

        public IEnumerable<Plan> GetAllPlans()
        {
            _client = new FirebaseClient(_config);
            var response = _client.Get("Plans");

            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

            if (data == null)
            {
                return null;
            }

            var list = new List<Plan>();

            foreach (var plan in data)
            {
                list.Add(JsonConvert.DeserializeObject<Plan>(((JProperty)plan).Value.ToString()));
            }

            return list;
        }

        public Plan GetPlanById(string planId)
        {
            _client = new FirebaseClient(_config);
            var response = _client.Get($"Plans/{planId}");

            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

            if (data == null)
            {
                return null;
            }

            var plan = JsonConvert.DeserializeObject<Plan>(((JObject)data).ToString());

            return plan;
        }

        public Plan EditPlan(Plan plan)
        {
            _client = new FirebaseClient(_config);

            _client.Set($"Plans/{plan.Plan_ID}", plan);

            return plan;
        }

        public void DeletePlanById(string planId)
        {
            _client = new FirebaseClient(_config);

            _client.Delete($"Plans/{planId}");
        }

        public string FindPlanNameByProduct(string productId)
        {
            var plans = GetAllPlans();

            if (plans == null)
            {
                return null;
            }

            string planName = null;

            foreach (var plan in plans)
            {
                foreach (var placedProduct in plan.PlacedProducts)
                {
                    if (placedProduct.Product.Product_ID == productId)
                    {
                        planName = plan.Name;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(planName))
                {
                    return planName;
                }
            }

            return planName;
        }

        public Plan CheckUserPlansForName(User user, string name)
        {
            foreach (var plan in user.Plans)
            {
                if (plan.Name == name)
                {
                    return plan;
                }
            }

            return null;
        }
        #endregion
    }
}