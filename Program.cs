using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.Data.SQLite;

// docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
namespace chatApp
{
  class SqlDataBase
  {
    static void DataBaseSQL(string[] args)
    {
        string connectionString = "Data Source= order.db";
        SQLiteConnection connection1 = new SQLiteConnection(connectionString);
        connection1.Open();

        //check version of SQLite is running
        string statement = "SELECT SQLITE_VERSION()";
        SQLiteCommand cmd = new SQLiteCommand(statement, connection1);

        //executeScaler gives 1 value back(1st row, 1st column if lots of data from select)
        string version = cmd.ExecuteScalar().ToString();

        Console.WriteLine("Sqlite version: " + version);
            //LETS BUILD A TABLE
            cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS order1(OrderID INTEGER PRIMARY KEY AUTOINCREMENT, username, quantity, price, buying, selling)", connection1);
            cmd.ExecuteNonQuery();
            cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS gridusers(GridID INTEGER PRIMARY KEY AUTOINCREMENT, username, X, Y)", connection1);
            cmd.ExecuteNonQuery();
            // Save changes to db
            connection1.Close();
    }
  }
    public class Send
    {
        public enum program
        {
            Chatting = 1,
            trading = 2,
            grid = 3,
            exit = 4
        }
        public string username;
        public bool buy = false;
        public bool sell = false;
        public int quanitity = 100;
        public double price;
        public string orderReq;

        string connectionString = "Data Source= order.db";
        SQLiteConnection connection1;

        static void Main(string[] args)
        {
            var tryout = new Send();
            tryout.app();
        }
        //----------------------------APPLICATION-------------------------------------------------------
        public void app()
        {
            int state;
            int i = 3;
            connect();
            //Calling username function---------
            user();
           
            while (i > 0)
            {
                // Menu----------
                Console.WriteLine("Would you like to enter Chatting(1), Trading(2), Contact Tracing(3), Exit(4)");
                state = Convert.ToInt32(Console.ReadLine());

                // Chatting-----------
                if (state == Convert.ToInt32(program.Chatting))
                {
                    while (i == 3)
                    {
                        Console.WriteLine("If you wish to Exit chatting press (e), to Continue (c)");
                        string exit = Console.ReadLine();
                        //Exit
                        if (exit == "e")
                        {
                            break;
                        }
                        // Calling Receive and Sent Function
                        else if (exit == "c")
                        {
                            rec();
                            send();
                        }
                    }
                }
                //Trading
                else if (state == Convert.ToInt32(program.trading))
                {
                    Console.WriteLine("If you wish to Exit chatting press (e), to Continue (c)");
                    string exit = Console.ReadLine();
                    //Exit
                    if (exit == "e")
                    {
                        break;
                    }
                    // Calling Receive and Sent Function
                    else if (exit == "c")
                    {
                        order();
                        exchange();

                        transaction();
                    }
                }
                //Tracing
                else if (state == Convert.ToInt32(program.grid))
                {
                    gridConnect();
                    grid();
                    radius();
                    Console.WriteLine("If you wish to Exit chatting press (e), to Continue (c)");
                    string exit = Console.ReadLine();
                    //Exit
                    if (exit == "e")
                    {
                        gridwipe();
                        break;
                    }  
                }
                //Exit
                else if (state == Convert.ToInt32(program.exit))
                {
                    break;
                }
            }
        }
 //----------------------------SEND-------------------------------------------------------
        public void send()
        {
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                    bool sent = false;
                    while (sent == false)
                    {
                        Console.WriteLine("Write a message");
                        string message = Console.ReadLine();
                        if (message.Length > 0)
                        {
                            var body = Encoding.UTF8.GetBytes(username + ": " + message);
                            channel.BasicPublish(exchange: "",
                                                 routingKey: "hello",
                                                 basicProperties: null,
                                                 body: body);
                            Console.WriteLine(message);
                            sent = true;
                        }
                        else
                        {
                            Console.WriteLine("Invalid message");
                        }
                    }
                }
            }
        }
//----------------------------RECEIVE-------------------------------------------------------
        public void rec()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                };
                channel.BasicConsume(queue: "hello",
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" You're in receiving mode press [enter] to change sending mode.");
                Console.ReadLine();
            }
        }
 //----------------------------USERNAME-------------------------------------------------------
        string user()
        {
            bool validUser = false;
            while (validUser == false)
            {
                Console.WriteLine("Write your username, more than 3 characters");
                username = Console.ReadLine();
                if (username.Length > 3)
                {
                    validUser = true;

                }
                else
                {
                    Console.WriteLine("Invalid username");
                }
            }
            return username;
        }
//----------------------------CONNECT--------------------------------------------------------
        public void connect()
        {
            connection1 = new SQLiteConnection(connectionString);
            connection1.Open();

            //check version of SQLite is running
            string statement = "SELECT SQLITE_VERSION()";
            SQLiteCommand cmd = new SQLiteCommand(statement, connection1);

            //executeScaler gives 1 value back(1st row, 1st column if lots of data from select)
            string version = cmd.ExecuteScalar().ToString();

            //Console.WriteLine("Sqlite version: " + version);
            cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS order1(OrderID INTEGER PRIMARY KEY AUTOINCREMENT, username, quantity, price, buying, selling)", connection1);
            cmd.ExecuteNonQuery();
        }
//----------------------------ORDER-------------------------------------------------------
        public void order()
        {
            Console.WriteLine("what is the price for the shares");
            double temp = Convert.ToDouble(Console.ReadLine());
            price = temp;

            while (buy == false && sell == false)
            {
                Console.WriteLine("are you buying(b) or selling(s)?");
                string buySell = Console.ReadLine();
                if (buySell == "b")
                {
                    buy = true;
                    orderReq = "buying";
                }
                else if (buySell == "s")
                {
                    sell = true;
                    orderReq = "selling";
                }
                else
                {
                    Console.WriteLine("invalid input");
                }
            }
            string message = (username + ": " + quanitity + " Shares | $" + price + " | " + orderReq);
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "logs",
                                     routingKey: "",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(message);
            }
            SQLiteCommand cmd2 = new SQLiteCommand();
            cmd2.Connection = connection1;
            cmd2.CommandText = "INSERT INTO order1(username, quantity, price, buying, selling) VALUES( '" + username + "', '" + quanitity + "' ,'" + price + "','" + buy + "','" + sell + "')";
            int rowsAffected = cmd2.ExecuteNonQuery();
        }
//----------------------------EXCHANGE-------------------------------------------------------
        public void exchange()
        {
            string output;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                                  exchange: "logs",
                                  routingKey: "");

                Console.WriteLine("Exchange Open");

                SQLiteCommand cmd3 = new SQLiteCommand("", connection1);
                cmd3.CommandText = "SELECT * FROM order1";

                List<string> Orders = new List<string>();
                using (SQLiteDataReader rdr = cmd3.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        output = rdr["username"].ToString() + " | share quantity: " + rdr["quantity"].ToString()+ " | price: "+rdr["price"].ToString();
                   
                        Console.WriteLine(output);
                    }
                } 
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(message);
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to declare an order.");
                Console.ReadLine();
            }
        }
//----------------------------Transaction-------------------------------------------------------
        public void transaction()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                                  exchange: "logs",
                                  routingKey: "");

                Console.WriteLine("Trades Open");

                //SQL command queary that matches the orders 
                SQLiteCommand cmd7 = new SQLiteCommand("", connection1);
                SQLiteCommand cmd6 = new SQLiteCommand("", connection1);
                cmd6.CommandText = "SELECT * FROM order1 AS t1 WHERE EXISTS(SELECT * FROM order1 AS t2 WHERE t2.price = t1.price AND t2.buying != t1.buying)";
                // GET usernames and ID usings list from SQL
                List<int> orderID = new List<int>();
                List<string> names = new List<string>();
                using (SQLiteDataReader rdr = cmd6.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        orderID.Add(Convert.ToInt32(rdr["orderID"]));
                        names.Add(rdr["username"].ToString());
                    }
                }
                int a = 0;
                Console.WriteLine("these users have completed a transaction: ");
                foreach (string i in names)
                {
                    Console.WriteLine(names[a]);
                    a++;
                }
                names.Clear();
                foreach (int i in orderID)
                {
                    cmd7.CommandText = "DELETE FROM order1 WHERE orderID = '" + orderID[i] + "'";
                }
                orderID.Clear();
                Console.WriteLine("trades complete: exchange updated");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(message);
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);
                Console.WriteLine(" Press [enter] to declare an order.");
                Console.ReadLine();
            }
        }
 //----------------------------GridConnect-------------------------------------------------------
        public void gridConnect()
        {
            // Connect database to grid 
            connection1 = new SQLiteConnection(connectionString);
            connection1.Open();

            //check version of SQLite is running
            string statement = "SELECT SQLITE_VERSION()";
            SQLiteCommand cmd = new SQLiteCommand(statement, connection1);

            //executeScaler gives 1 value back(1st row, 1st column if lots of data from select)
            string version = cmd.ExecuteScalar().ToString();
            cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS gridusers(GridID INTEGER PRIMARY KEY AUTOINCREMENT, username, X, Y)", connection1);
            cmd.ExecuteNonQuery();
        }
 //----------------------------Grid-------------------------------------------------------
        public void grid()
        {
            // Create grid for tracing and open rabbitMQ
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "grid", type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                                  exchange: "grid",
                                  routingKey: "");
                Random rand = new Random();
          
                Console.WriteLine("Do you want to placed on the grid? (y) return to menu (n)");
                string decision = Console.ReadLine();
                if (decision == "y")
                {
                    // random generators to place user on the grid
                    int rand1 = rand.Next(1, 10);
                    int rand2 = rand.Next(1, 10);
                    SQLiteCommand cmd2 = new SQLiteCommand();
                    cmd2.Connection = connection1;
                    cmd2.CommandText = "INSERT INTO gridusers(username,X,Y) VALUES( '" + username + "','"+rand1+"','"+rand2+"')";
                    int rowsAffected = cmd2.ExecuteNonQuery();
                    Console.WriteLine("You have been placed on the grid");
                }
            }        
        }
 //----------------------------RADIUS-------------------------------------------------------
        public void radius()
        {
            //coordinates for user
            double X1 =0;
            double Y1 =0;
            //coordinates for other users
            double X2;
            double Y2;

            int radius = 6;
            List<string> closeuser = new List<string>();
            SQLiteCommand cmd3 = new SQLiteCommand("", connection1);
            SQLiteCommand cmd4 = new SQLiteCommand("", connection1);
            cmd3.CommandText = "SELECT X,Y FROM gridusers WHERE username = '"+username+"'";
            cmd4.CommandText = "SELECT username,X,Y FROM gridusers WHERE username != '" + username + "'";

            using (SQLiteDataReader rdr = cmd3.ExecuteReader())
            {
                while(rdr.Read())
                {
                    X1 = Convert.ToDouble(rdr["X"]);   
                    Y1 = Convert.ToDouble(rdr["Y"]);
                }
                rdr.Close(); 
            }
            using (SQLiteDataReader rdr = cmd4.ExecuteReader())
            {
                
                while (rdr.Read())
                {
                    X2 = Convert.ToInt32(rdr["X"]);
                    Y2 = Convert.ToInt32(rdr["Y"]);
                    var distance = Math.Sqrt(Math.Pow(X1 - X2, 2) + Math.Pow(Y1 - Y2, 2));
                    if (distance < radius)
                    {
                        closeuser.Add(rdr["username"].ToString());
                    }
                }
            }
            Console.WriteLine("Here are all users within your radius");
            for (int i = 0; i < closeuser.Count; i++)
            {
                Console.WriteLine(closeuser[i]);
            }    
        }
//----------------------------Delete the user from the grid-------------------------------------------------------
        public void gridwipe()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                                  exchange: "logs",
                                  routingKey: "");

                Console.WriteLine("You've exited the grid succesfully ");
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = connection1;
                cmd.CommandText = "DELETE FROM gridusers WHERE username = '" + username + "'";
                cmd.ExecuteNonQuery();

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(message);
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
