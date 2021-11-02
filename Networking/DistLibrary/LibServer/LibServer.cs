using System;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using LibData;
using System.Text;

namespace LibServer
{
    // Note: Do not change this class.
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


    // Note: Complete the implementation of this class. You can adjust the structure of this class. 
    public class SequentialServer
    {
        public SequentialServer()
        {
            //todo: implement the body. Add extra fields and methods to the class if it is needed
        }

        public void start()
        {
            Setting settings = JsonSerializer.Deserialize<Setting>(File.ReadAllText(@"../ClientServerConfig.json"));


            IPEndPoint userHelperEndpoint = new IPEndPoint(IPAddress.Loopback, 11113);
            IPEndPoint bookHelperEndpoint = new IPEndPoint(IPAddress.Loopback, 11112);
            IPEndPoint libServerEndpoint = new IPEndPoint(IPAddress.Loopback, 11111);
            Socket libSocket = new Socket(libServerEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            #region Connections
            //connecting with the user helper
            Socket UHSocket = new Socket(libServerEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            UHSocket.Connect(userHelperEndpoint);
            Console.WriteLine("Now connected to helper server:User");

            
            //connecting with the book helper
            Socket BHSocket = new Socket(libServerEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            BHSocket.Connect(bookHelperEndpoint);
            Console.WriteLine("Now connected to helper server:Book");
            #endregion

            #region Variables
            byte[] buffer = new byte[1000];
            Message msgIn = new Message();
            Message msgOut = new Message();
            #endregion

            //bind socket and wait for connection
            libSocket.Bind(libServerEndpoint);

            while (true)
            {
                libSocket.Listen(5);
                Console.WriteLine("Waiting for connection::LibServer\n");

                //accept connection from client
                Socket clientSocket = libSocket.Accept();
                Console.WriteLine("Connection accepted from client\n");
                
                do
                {
                    //receiving first message from client
                    Console.WriteLine("waiting for message\n");
                    int b = clientSocket.Receive(buffer);
                    msgIn = JsonSerializer.Deserialize<Message>(Encoding.ASCII.GetString(buffer, 0, b));
                    Console.WriteLine("message received\n");

                    //receiving first message
                    if (msgIn.Type == MessageType.Hello)
                    {
                        Console.WriteLine("receiving first message from client\n");

                        //send welcome message to client
                        msgOut.Type = MessageType.Welcome;
                        msgOut.Content = "";

                        clientSocket.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(msgOut)));
                        Console.WriteLine("sending back welcome message\n");
                    }



                    //forwarding book request to bookHelperServer
                    if (msgIn.Type == MessageType.BookInquiry)
                    {
                        Forwarding(BHSocket, msgIn, buffer);
                        Console.WriteLine("forwarding book inquiry to book helper\n");

                        //repacking message from bookHelper and sending to client
                        //Forwarding(BHSocket, clientSocket, buffer);
                        b = BHSocket.Receive(buffer);
                        msgIn = JsonSerializer.Deserialize<Message>(Encoding.ASCII.GetString(buffer, 0, b));
                        BookData bookContent = JsonSerializer.Deserialize<BookData>(msgIn.Content);
                        if (bookContent.Status != "Borrowed")
                        {
                            Console.WriteLine("forwarding bookinquiryReply with break\n");
                            Forwarding(clientSocket, msgIn, buffer);
                            clientSocket.Close();
                            break;
                        }
                        else
                        {
                            Console.WriteLine("forwarding bookinquiryReply without break\n");
                            Forwarding(clientSocket, msgIn, buffer);
                        }
                    }

                    //forwarding user request to userHelperServer
                    if (msgIn.Type == MessageType.UserInquiry)
                    {
                        //repacking message and forwarding to userhelper
                        Forwarding(UHSocket, msgIn, buffer);
                        Console.WriteLine("forwarding userID to userhelper\n");

                        //repacking message from userHelpler and sending to client
                        Forwarding(UHSocket, clientSocket, buffer);
                        Console.WriteLine("forwarding user information from helper to client\n");
                        clientSocket.Close();
                        break;
                    }
                
                } while (msgIn.Type != MessageType.EndCommunication);

                //end communcation from server
                if (msgIn.Type == MessageType.EndCommunication)
                {
                    Console.WriteLine("closing connections\n");
                    Forwarding(BHSocket, msgIn, buffer);
                    Forwarding(UHSocket, msgIn, buffer);
                    Forwarding(clientSocket, msgIn, buffer);
                    BHSocket.Close();
                    UHSocket.Close();
                    clientSocket.Close();
                    libSocket.Close();
                    Console.ReadKey();
                    break;
                }
            }
            
        }

        /// <summary>
        /// For forwarding messages from one place to another (include receiving)
        /// </summary>
        /// <param name="origin">the socket where the incoming message is coming from</param>
        /// <param name="destination">the destination socket of the message</param>
        /// <param name="buffer">the buffer thats going to be used to send</param>
        private void Forwarding(Socket origin, Socket destination, byte[] buffer)
        {
            int b = origin.Receive(buffer);
            Message msgIn = JsonSerializer.Deserialize<Message>(Encoding.ASCII.GetString(buffer, 0, b));
            
            Message msgOut = new Message()
            {
                Type = msgIn.Type,
                Content = msgIn.Content
            };
            Console.WriteLine($"Forwarding::{msgIn.Type} ,{msgOut.Content}");
            destination.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(msgOut)));
        }

        /// <summary>
        /// For forwarding messages from one place to another (NOT include receiving)
        /// </summary>
        /// <param name="destination">the destination socket of the message</param>
        /// <param name="msgIn">the message that needs forwarding</param>
        /// <param name="buffer">the buffer thats going to be used to send</param>
        private void Forwarding(Socket destination, Message msgIn, byte[] buffer)
        {
            Message msgOut = new Message()
            {
                Type = msgIn.Type,
                Content = msgIn.Content
            };
            Console.WriteLine($"Forwarding::{msgIn.Type} ,{msgOut.Content}");
            destination.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(msgOut)));
            
        }
    }
}



