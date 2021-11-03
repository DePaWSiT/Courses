using System;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using LibData;
using System.Text;

namespace BookHelper
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

            //opens and stores Books from Books.json
            List<BookData> bookContent = JsonSerializer.Deserialize<List<BookData>>(File.ReadAllText(@"BooksData.json"));

            //makes socket
            IPEndPoint bookHelperEndpoint = new IPEndPoint(IPAddress.Parse(settings.BookHelperIPAddress), settings.BookHelperPortNumber);
            Socket socket = new Socket(bookHelperEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //binds socket and puts it in listening state
            socket.Bind(bookHelperEndpoint);
            socket.Listen(5);
            Console.WriteLine("Waiting for connection::BookHelper");
            
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
                    //searching for the book and sends book info back when found
                    bool bookFound = false;
                    for (int i = 0; i < bookContent.Count; i++)
                    {

                        if (bookContent[i].Title == msgIn.Content)
                        {
                            msgOut.Type = MessageType.BookInquiryReply;
                            msgOut.Content = JsonSerializer.Serialize(bookContent[i]);
                            libServerSocket.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(msgOut)));
                            Console.WriteLine("Book found, send to server\n");

                            bookFound = true;
                            break;
                        }
                    }
                    //scenario for when book cannot be found
                    if (!bookFound)
                    {
                        msgOut.Type = MessageType.NotFound;
                        msgOut.Content = JsonSerializer.Serialize(new BookData());
                        libServerSocket.Send(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(msgOut)));
                        Console.WriteLine("Book NOT found, send to server\n");
                    }
                }
            }
        }
    }
}
