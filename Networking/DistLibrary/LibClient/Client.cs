using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using LibData;
using System.Text;
using System.Collections.Generic;

namespace LibClient
{
    // Note: Do not change this class 
    public class Setting
    {
        public int ServerPortNumber { get; set; }
        public int BookHelperPortNumber { get; set; }
        public int UserHelperPortNumber { get; set; }
        public string ServerIPAddress { get; set; }
        public string BookHelperIPAddress { get; set; }
        public string UserHelperIPAddress { get; set; }
        public int ServerListeningQueue { get; set; }
    }

    // Note: Do not change this class 
    public class Output
    {
        public string Client_id { get; set; } // the id of the client that requests the book
        public string BookName { get; set; } // the name of the book to be reqyested
        public string Status { get; set; } // final status received from the server
        public string BorrowerName { get; set; } // the name of the borrower in case the status is borrowed, otherwise null
        public string BorrowerEmail { get; set; } // the email of the borrower in case the status is borrowed, otherwise null
    }

    // Note: Complete the implementation of this class. You can adjust the structure of this class.
    public class SimpleClient
    {
        // some of the fields are defined. 
        public Output result;
        public Socket clientSocket;
        public IPEndPoint serverEndPoint;
        public IPAddress ipAddress;
        public Setting settings;
        public string client_id;
        private string bookName;
        // all the required settings are provided in this file
        public string configFile = @"../ClientServerConfig.json";
        //public string configFile = @"../../../../ClientServerConfig.json"; // for debugging

        // todo: add extra fields here in case needed 

        /// <summary>
        /// Initializes the client based on the given parameters and seeting file.
        /// </summary>
        /// <param name="id">id of the clients provided by the simulator</param>
        /// <param name="bookName">name of the book to be requested from the server, provided by the simulator</param>
        public SimpleClient(int id, string bookName)
        {
            //todo: extend the body if needed.
            this.bookName = bookName;
            this.client_id = "Client " + id.ToString();
            this.result = new Output();
            result.BookName = bookName;
            result.Client_id = this.client_id;
            // read JSON directly from a file
            try
            {
                string configContent = File.ReadAllText(configFile);
                this.settings = JsonSerializer.Deserialize<Setting>(configContent);
                this.ipAddress = IPAddress.Parse(settings.ServerIPAddress);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("[Client Exception] {0}", e.Message);
            }
        }

        /// <summary>
        /// Establishes the connection with the server and requests the book according to the specified protocol.
        /// Note: The signature of this method must not change.
        /// </summary>
        /// <returns>The result of the request</returns>
        public Output start()
        {

            // todo: implement the body to communicate with the server and requests the book. Return the result as an Output object.
            // Adding extra methods to the class is permitted. The signature of this method must not change.
            Setting settings = JsonSerializer.Deserialize<Setting>(File.ReadAllText(@"../ClientServerConfig.json"));

            byte[] buffer = new byte[1000];
            Message msgIn = new Message();
            Message msgOut = new Message()
            {
                Type = MessageType.Hello,
                Content = client_id.ToString(),
            };

            #region connection
            //making connection with server
            IPEndPoint libServerEndpoint = new IPEndPoint(IPAddress.Parse(settings.ServerIPAddress), settings.ServerPortNumber);
            Socket socket = new Socket(libServerEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(libServerEndpoint);
            #endregion

            #region Hello message
            //sending first message
            socket.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(msgOut)));
            Console.WriteLine("Sending first message to server\n");

            //receiving first message from server
            int b = socket.Receive(buffer);
            msgIn = JsonSerializer.Deserialize<Message>(Encoding.ASCII.GetString(buffer, 0, b));
            Console.WriteLine("receiving welcome from server\n");
            #endregion


            //end communications
            if (client_id == "Client -1")
            {
                //send message to close everything
                Console.WriteLine("Closing Connections") ;
                msgOut.Type = MessageType.EndCommunication;
                msgOut.Content = "";
                socket.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(msgOut)));

                //receive message to close socket
                b = socket.Receive(buffer);
                msgIn = JsonSerializer.Deserialize<Message>(Encoding.ASCII.GetString(buffer, 0, b));
                Console.WriteLine("Closing socket::Server");
                socket.Close();
                Console.WriteLine("program finished\n");
                return result;
            }
            
            #region bookinquiry
            //sending bookinquiry
            msgOut.Type = MessageType.BookInquiry;
            msgOut.Content = bookName;
            socket.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(msgOut)));
            Console.WriteLine("Sending bookinquiry to server");

            //receiving bookinfo from server
            b = socket.Receive(buffer);
            msgIn = JsonSerializer.Deserialize<Message>(Encoding.ASCII.GetString(buffer, 0, b));
            Console.WriteLine("Receiving bookinquiryReply from server\n");
            
            //when no book was found
            if (msgIn.Type == MessageType.NotFound)
            {
                Console.WriteLine("Book not found\n");
                socket.Close();
                result.Client_id = client_id;
                result.BookName = bookName;
                result.Status = "NotFound";
                result.BorrowerName = null;
                result.BorrowerEmail = null;
                return result;
            }
            
            BookData bookData = JsonSerializer.Deserialize<BookData>(msgIn.Content);
            #endregion

            #region userinquiry
            //when the book is availible
            if (bookData.Status == "Available")
            {
                socket.Close();
                result.Client_id = client_id;
                result.BookName = bookData.Title;
                result.Status = bookData.Status;
                result.BorrowerName = null;
                result.BorrowerEmail = null;
                return result;
            }

            //when the book is borrowed
            if (bookData.Status == "Borrowed")
            {
                Console.WriteLine("sending userinquiry to server");
                //sending userID to the server
                msgOut.Type = MessageType.UserInquiry;
                msgOut.Content = bookData.BorrowedBy;
                socket.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(msgOut)));

                //receiving user-information to client
                Console.WriteLine("receiving userinquiryReply from server\n");
                b = socket.Receive(buffer);
                msgIn = JsonSerializer.Deserialize<Message>(Encoding.ASCII.GetString(buffer, 0, b));
                UserData userData = JsonSerializer.Deserialize<UserData>(msgIn.Content);

                result.Client_id = client_id;
                result.BookName = bookData.Title;
                result.Status = bookData.Status;
                result.BorrowerName = userData.Name;
                result.BorrowerEmail = userData.Email;
                return result;
            }
            #endregion

            return result;
        }
    }
}
