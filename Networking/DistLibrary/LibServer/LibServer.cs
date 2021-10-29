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

            byte[] buffer = new byte[1000];
            Message msgIn = new Message();
            Message msgOut = new Message()
            {
                Type = MessageType.Welcome,
                Content = "",
            };


            IPEndPoint userHelperEndpoint = new IPEndPoint(IPAddress.Loopback, 11113);
            IPEndPoint bookHelperEndpoint = new IPEndPoint(IPAddress.Loopback, 11112);
            IPEndPoint libServerEndpoint = new IPEndPoint(IPAddress.Loopback, 11111);
            Socket libSocket = new Socket(libServerEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            #region Connections
            //connecting with the user helper
            Socket libToUHSocket = new Socket(libServerEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            libToUHSocket.Connect(userHelperEndpoint);
            Console.WriteLine("Now connected to helper server:User");

            
            //connecting with the book helper
            Socket libToBHSocket = new Socket(libServerEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            libToBHSocket.Connect(bookHelperEndpoint);
            Console.WriteLine("Now connected to helper server:Book");


            //bind socket and wait for connection
            libSocket.Bind(libServerEndpoint);
            libSocket.Listen(5);
            Console.WriteLine("Waiting for connection::LibServer");

            //accept connection from client
            Socket clientSocket = libSocket.Accept();
            Console.WriteLine("Connection accepted from client");
            #endregion

            //receiving first message from client
            int b = clientSocket.Receive(buffer);
            msgIn = JsonSerializer.Deserialize<Message>(Encoding.ASCII.GetString(buffer, 0, b));
            Console.WriteLine("receiving first message from client");

            //send welcome message to client
            clientSocket.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(msgOut)));
            Console.WriteLine("sending back welcome message");

            //forwarding book request to bookHelperServer
            Forwarding(clientSocket, libToBHSocket, buffer);
            Console.WriteLine("forwarding book inquiry to book helper");
            

            //repacking message from bookHelper and sending to client
            Forwarding(libToBHSocket, clientSocket, buffer);
            Console.WriteLine("forwarding bookinfo from the helper to the client");

            //repacking message and forwarding to userhelper
            Forwarding(clientSocket, libToUHSocket, buffer);
            Console.WriteLine("forwarding userID to userhelper");

            //repacking message from userHelpler and sending to client
            Forwarding(libToUHSocket, clientSocket, buffer);
            Console.WriteLine("forwarding user information from helper to client");

            //end communcation from server
            b = clientSocket.Receive(buffer);
            msgIn = JsonSerializer.Deserialize<Message>(Encoding.ASCII.GetString(buffer, 0, b));
        }

        private void Forwarding(Socket origin, Socket destination, byte[] buffer)
        {
            int b = origin.Receive(buffer);
            Message msgIn = JsonSerializer.Deserialize<Message>(Encoding.ASCII.GetString(buffer, 0, b));
            Message msgOut = new Message()
            {
                Type = msgIn.Type,
                Content = msgIn.Content
            };
            destination.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(msgOut)));
        }
    }

}



