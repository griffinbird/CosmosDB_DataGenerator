namespace DataGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;
    using Bogus;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Class contains code that creates randomized data from a clothing 
    /// store and inserts it into a Cosmos DB collection.
    /// </summary>
    public class Program
    {
        public static int count = 0;
        /// <summary>
        /// Singleton instance of the Cosmos DB client that accesses the service.
        /// </summary>
        private static readonly DocumentClient Client = new DocumentClient(
            new Uri(ConfigurationManager.AppSettings["endpoint"]),
            ConfigurationManager.AppSettings["authKey"],
            new ConnectionPolicy()
            {
                UserAgentSuffix = " samples-net/3",
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            });

        /// <summary>
        /// Initializes Uri for the Cosmos DB collection.
        /// </summary>
        private static readonly Uri CollectionUri = UriFactory.CreateDocumentCollectionUri(
            ConfigurationManager.AppSettings["database"],
            ConfigurationManager.AppSettings["collection"]);

        /// <summary>
        /// Contains the valid actions a user can take.
        /// </summary>
        public enum Action
        {
            /// <summary>
            /// User has viewed an item.
            /// </summary>
            Viewed,

            /// <summary>
            /// User has added an item to cart. 
            /// </summary>
            Added,

            /// <summary>
            /// User has purchased an item.
            /// </summary>
            Purchased
        }

       
        /// <summary>
        /// Main method that calls CreateData().
        /// </summary>
        /// <param name="args"> Default main arguments. </param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Simulation of data started....");
            //try
            //{
            //    for (int i = 0; i < 4; i++)
            //    {
            //        CreateData();
            //        Thread.Sleep(1000);
            //    }
            //}
            //catch (Exception exception)
            //{
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
            //    Console.ResetColor();
            //}
            CreateData();
            Console.ReadKey(false);

            return;
        }

        /// <summary>
        /// Method that creates randomized data by generating a random number for the ShoppingCartID, selecting a 
        /// random item from the list of items, and matching it with a random Action from GetRandomAction(Randomizer r).
        /// </summary>
        public static async void CreateData()
        {
            Randomizer random = new Randomizer();
            string[] items = new string[]
            {
                "[{\"Category\":\"Tshirts\",\"Brand\":\"Nike\",\"Item\":\"Round colar x15 tshirt\",\"Price\":\"99\"}, {\"Category\":\"Tshirts\",\"Brand\":\"Nike\",\"Item\":\"V colar x16 tshirt\",\"Price\":\"95\"}]",
                "[{\"Category\":\"Shoes\",\"Brand\":\"Nike\",\"Item\":\"Leather y16 shoes\",\"Price\":\"199\"}, {\"Category\":\"Shoes\",\"Brand\":\"Nike\",\"Item\":\"Sports shoes striped\",\"Price\":\"94\"}]",
                "[{\"Category\":\"Pants\",\"Brand\":\"Nike\",\"Item\":\"Cotton z16 pants\",\"Price\":\"79\"}, {\"Category\":\"Pants\",\"Brand\":\"Nike\",\"Item\":\"Synthetic z7 pants\",\"Price\":\"55\"}]",
                "[{\"Category\":\"Socks\",\"Brand\":\"Nike\",\"Item\":\"Cotton socks\",\"Price\":\"9\"}, {\"Category\":\"Socks\",\"Brand\":\"Nike\",\"Item\":\"Stripe socks\",\"Price\":\"5\"}]",
                "[{\"Category\":\"Shorts\",\"Brand\":\"Nike\",\"Item\":\"Sports shorts\",\"Price\":\"29\"}, {\"Category\":\"Shorts\",\"Brand\":\"Nike\",\"Item\":\"Running shorts\",\"Price\":\"25\"}]",
                "[{\"Category\":\"Tshirts\",\"Brand\":\"Addidas\",\"Item\":\"Round colar x15 tshirt\",\"Price\":\"98\"}, {\"Category\":\"Tshirts\",\"Brand\":\"Addidas\",\"Item\":\"V colar x16 tshirt\",\"Price\":\"94\"}]",
                "[{\"Category\":\"Shoes\",\"Brand\":\"Addidas\",\"Item\":\"Leather y16 shoes\",\"Price\":\"198\"}, {\"Category\":\"Shoes\",\"Brand\":\"Addidas\",\"Item\":\"Sports shoes striped\",\"Price\":\"93\"}]",
                "[{\"Category\":\"Pants\",\"Brand\":\"Addidas\",\"Item\":\"Cotton z16 pants\",\"Price\":\"78\"}, {\"Category\":\"Pants\",\"Brand\":\"Addidas\",\"Item\":\"Synthetic z7 pants\",\"Price\":\"54\"}]",
                "[{\"Category\":\"Socks\",\"Brand\":\"Addidas\",\"Item\":\"Cotton socks\",\"Price\":\"8\"}, {\"Category\":\"Socks\",\"Brand\":\"Addidas\",\"Item\":\"Stripe socks\",\"Price\":\"4\"}]",
                "[{\"Category\":\"Shorts\",\"Brand\":\"Addidas\",\"Item\":\"Sports shorts\",\"Price\":\"28\"}, {\"Category\":\"Shorts\",\"Brand\":\"Addidas\",\"Item\":\"Running shorts\",\"Price\":\"24\"}]",
                "[{\"Category\":\"Tshirts\",\"Brand\":\"Puma\",\"Item\":\"Round colar x15 tshirt\",\"Price\":\"97\"}, {\"Category\":\"Tshirts\",\"Brand\":\"Puma\",\"Item\":\"V colar x16 tshirt\",\"Price\":\"93\"}]",
                "[{\"Category\":\"Shoes\",\"Brand\":\"Puma\",\"Item\":\"Leather y16 shoes\",\"Price\":\"197\"}, {\"Category\":\"Shoes\",\"Brand\":\"Puma\",\"Item\":\"Sports shoes striped\",\"Price\":\"92\"}]",
                "[{\"Category\":\"Pants\",\"Brand\":\"Puma\",\"Item\":\"Cotton z16 pants\",\"Price\":\"77\"}, {\"Category\":\"Pants\",\"Brand\":\"Puma\",\"Item\":\"Synthetic z7 pants\",\"Price\":\"53\"}]",
                "[{\"Category\":\"Socks\",\"Brand\":\"Puma\",\"Item\":\"Cotton socks\",\"Price\":\"7\"}, {\"Category\":\"Socks\",\"Brand\":\"Puma\",\"Item\":\"Stripe socks\",\"Price\":\"3\"}]",
                "[{\"Category\":\"Shorts\",\"Brand\":\"Puma\",\"Item\":\"Sports shorts\",\"Price\":\"27\"}, {\"Category\":\"Shorts\",\"Brand\":\"Puma\",\"Item\":\"Running shorts\",\"Price\":\"23\"}]"
            };
           
            

            bool loop = true;

           while (loop)
            {
                int itemIndex = random.Number(0, items.Length-1);
                Event e = new Event()
                {
                    ShoppingCartID = random.Number(1000, 9999),
                    Action = random.Enum<Action>(),
                    Item = items[itemIndex],
                    //Price = prices[itemIndex]
                };
                await InsertData(e);

                List<Action> previousActions = new List<Action>();
                switch (e.Action)
                {
                    case Action.Viewed:
                        break;
                    case Action.Added:
                        previousActions.Add(Action.Viewed);
                        break;
                    case Action.Purchased:
                        previousActions.Add(Action.Viewed);
                        previousActions.Add(Action.Added);
                        break;
                    default:
                        break;
                }

                foreach (Action previousAction in previousActions)
                {
                    Event previousEvent = new Event()
                    {
                        ShoppingCartID = e.ShoppingCartID,
                        Action = previousAction,
                        Item = e.Item,
                        //Price = e.Price
                    };
                    await InsertData(previousEvent);
                }               
            }

            string key = Console.ReadKey().Key.ToString();
            if (key == " ")
            {
                loop = false;
            }
            else
            {
                loop = true;
            }

            CreateData();
        }

        /// <summary>
        /// Inserts each event e to the database by using Azure DocumentClient library.
        /// </summary>
        /// <param name="e"> An instance of the Event class representing a user click. </param>/
        /// <returns>returns a Task</returns>        
        private static async Task InsertData(Event e)
        {
            count++;

            await Client.CreateDocumentAsync(CollectionUri, e);
            Console.Write("*" + (count).ToString());
        }

        /// <summary>
        /// Class that defines the parameters of an event a user can make.
        /// </summary>
        internal class Event
        {
            /// <summary>
            /// Gets or sets an ID to represent the user that is shopping.
            /// </summary>
            public int ShoppingCartID { get; set; }

            /// <summary>
            /// Gets or sets action from the action list.
            /// </summary>
            [JsonConverter(typeof(StringEnumConverter))]
            public Action Action { get; set; }

            /// <summary>
            /// Gets or sets item from the item list.
            /// </summary>
            public string Item { get; set; }

            /// <summary>
            /// Gets or sets price associated with each Item by index from the price list.
            /// </summary>
            //public double Price { get; set; }
        }
    }
}
