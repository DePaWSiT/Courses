﻿using System;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using LibData;
using System.Text;

namespace UserHelper
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
    public class SequentialHelper
    {
        public SequentialHelper()
        {
            //todo: implement the body. Add extra fields and methods to the class if needed
        }

        public void start()
        {
            //todo: implement the body. Add extra fields and methods to the class if needed

            byte[] buffer = new byte[1000];
            string data = null;

            IPEndPoint userHelperEndpoint = new IPEndPoint(IPAddress.Loopback, 11113);
            Socket socket = new Socket(userHelperEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(userHelperEndpoint);

            socket.Listen(5);
            Console.WriteLine("Waiting for connection::UserHelper");

            Socket libServerSocket = socket.Accept();
            Console.WriteLine("Connection accepted");

            while (true)
            {
                //receive from lib server
                int b = libServerSocket.Receive(buffer);
                data = Encoding.ASCII.GetString(buffer, 0, b);
                Console.WriteLine(data);
                data = null;

                //send to lib server
                libServerSocket.Send(Encoding.ASCII.GetBytes("Userhelper message!"));
                
                //libServerSocket.Close();
            }
        }
    }
}
