﻿using System;
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
            //todo: implement the body. Add extra fields and methods to the class if it is needed


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
            
            #region Connecting
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

            //send welcome message to client
            clientSocket.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(msgOut)));

            //forwarding book request to bookHelperServer
            _ = clientSocket.Receive(buffer);
            libToBHSocket.Send(buffer);

            //forwarding book info from bookHelperServer to client
            _ = libToBHSocket.Receive(buffer);
            clientSocket.Send(buffer);
            /**while (true)
            {
                //receive from client
                int b = clientSocket.Receive(buffer);
                data = Encoding.ASCII.GetString(buffer, 0, b);
                Console.WriteLine($"The message sent was: {data}");
                data = null;
                

                while (true)
                {
                    //send to user helper
                    libToUHSocket.Send(Encoding.ASCII.GetBytes("This message is from the library server"));

                    //receive from user helper
                    b = libToUHSocket.Receive(buffer);
                    data = Encoding.ASCII.GetString(buffer, 0, b);
                    Console.WriteLine($"The message from the user server was {data}");
                    data = null;


                    userDataFound = true;
                    break;
                }
               
                while (userDataFound)
                {
                    //send to book helper
                    libToBHSocket.Send(Encoding.ASCII.GetBytes("This message is from the library server"));

                    //receive from book helper
                    b = libToBHSocket.Receive(buffer);
                    data = Encoding.ASCII.GetString(buffer, 0, b);
                    Console.WriteLine($"The message from the book server was {data}");
                    data = null;

                    bookDataFound = true;
                    break;
                }
                //send message back to client
                clientSocket.Send(Encoding.ASCII.GetBytes("This message came from the library server"));
            
            }
            **/
        }
    }

}



