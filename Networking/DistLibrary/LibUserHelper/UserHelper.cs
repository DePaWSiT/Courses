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
            Setting settings = JsonSerializer.Deserialize<Setting>(File.ReadAllText(@"../ClientServerConfig.json"));

            byte[] buffer = new byte[1000];
            Message msgIn = new Message();
            Message msgOut = new Message();

            //opens and stores Users from Users.json
            List<UserData> usersContent = JsonSerializer.Deserialize<List<UserData>>(File.ReadAllText(@"UsersData.json"));
            
            //makes socket
            IPEndPoint userHelperEndpoint = new IPEndPoint(IPAddress.Parse(settings.UserHelperIPAddress), settings.UserHelperPortNumber);
            Socket socket = new Socket(userHelperEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //binds socket and puts it in listen state
            socket.Bind(userHelperEndpoint);
            socket.Listen(5);
            Console.WriteLine("Waiting for connection::UserHelper");

            Socket libServerSocket = socket.Accept();
            Console.WriteLine("Connection accepted\n");
            

            while (true)
            {
                //receiving forwarded bookinquiry from server
                int b = libServerSocket.Receive(buffer);
                msgIn = JsonSerializer.Deserialize<Message>(Encoding.ASCII.GetString(buffer, 0, b));
                Console.WriteLine("Receiving inquiry from server");

                //close socket when server says to do so
                if (msgIn.Type == MessageType.EndCommunication)
                {
                    Console.WriteLine("Goodbye");
                    socket.Close();
                    break;
                }
                else
                {
                    bool userFound = false;
                    for (int i = 0; i < usersContent.Count; i++)
                    {
                        if (usersContent[i].User_id == msgIn.Content)
                        {
                            msgOut.Type = MessageType.UserInquiryReply;
                            msgOut.Content = JsonSerializer.Serialize(usersContent[i]);
                            libServerSocket.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(msgOut)));
                            Console.WriteLine("User found, send to server\n");
                            
                            userFound = true;
                            break;
                        }
                    }
                    if (!userFound)
                    {
                        msgOut.Type = MessageType.NotFound;
                        msgOut.Content = JsonSerializer.Serialize(new UserData());
                        libServerSocket.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(msgOut)));
                        Console.WriteLine("User NOT found, send to server\n");
                    }
                }
            }
        }
    }
}
