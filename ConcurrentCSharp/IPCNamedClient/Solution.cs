using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using Exercise;

/*
 * This is an example representing how two processes can communicate through NamedPipe
 */

namespace Solution
{
    public class SolutionIPCNamedClient : IPCNamedClient
    {
        NamedPipeServerStream server;
        StreamReader serverReader;
        StreamWriter serverWriter;

        public SolutionIPCNamedClient(string pipeName)
        {
            server = new NamedPipeServerStream(pipeName);
        }

        public void prepareClient()
        {
            Console.WriteLine("Pipe Client is being executed ...");
            Console.WriteLine("[Client] Client will be waiting for the server");

            server.WaitForConnection();
            serverReader = new StreamReader(server);

            // The client needs a writer stream to write its processing result
            serverWriter = new StreamWriter(server);
        }

        public void communicate()
        {
            while (true)
            {
                string msg = serverReader.ReadLine();

                if (string.IsNullOrEmpty(msg))
                {
                    Console.WriteLine("[Client] Programs is being terminated.");
                    break;
                }
                else
                {
                    Console.WriteLine(msg);
                    string reverseMsg = string.Join("", msg.Reverse());
                    Console.WriteLine(reverseMsg);
                    serverWriter.WriteLine(reverseMsg);
                    serverWriter.Flush();
                }
            }
        }
    }

}
