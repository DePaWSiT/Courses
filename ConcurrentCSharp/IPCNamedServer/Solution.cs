using System;
using System.IO;
using System.IO.Pipes;
using Exercise;

/*
 * This is an example representing how two processes can communicate through NamedPipe
 */

namespace Solution
{
    class SolutionIPCNamedServer: IPCNamedServer
    {
        NamedPipeClientStream client;
        StreamReader clientReader;
        StreamWriter clientWriter;

        public SolutionIPCNamedServer(string pipeName)
        {
            client = new NamedPipeClientStream(pipeName);
        }

        public void prepareServer()
        {
            Console.WriteLine("Pipe Server is being executed ...");
            Console.WriteLine("[Server] Enter a message to be reversed by the client (press ENTER to exit)");
            client.Connect();
            clientReader = new StreamReader(client);
            clientWriter = new StreamWriter(client);
        }

        public void communicate()
        {
            while (true)
            { 
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("[Server] Program is being terminated.");
                    break;
                }
                else
                {
                    clientWriter.WriteLine(input);
                    clientWriter.Flush();
                    string clientMsg = clientReader.ReadLine();
                    Console.WriteLine(clientMsg);

                }
            }
        }
    }
 }