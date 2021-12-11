using System;
using System.Diagnostics;

namespace Example
{
    public class Processes
    {
        private Process[] localProcsAll;
        // Prints all the running processes.
        public void printAllProcesses()
        {
            Console.WriteLine("This method is going to print information of current processes ... ");

            // How to get all the running processes in a local computer
            this.localProcsAll = Process.GetProcesses();
            foreach (Process pr in this.localProcsAll)
            {
                // Print some information from the process: name and id. There are more.
                Console.WriteLine("Process Name = {0}, Id = {1}", pr.ProcessName, pr.Id.ToString());
            }
        }
        // This example shows how to terminate a running process.
        public void terminateProcess()
        {
            Console.WriteLine("This method is going to terminate a process: Choose a process ");

            bool stop = false;
            // How to get the current running process, i.e. this program.
            Process currentProc = Process.GetCurrentProcess();

            while (!stop)
            {
                this.printAllProcesses();

                Console.WriteLine(" Enter a process id to terminate (enter stop to finish):");
                string inp = Console.ReadLine();
                if (inp == "stop") { stop = true; }
                else
                {
                    foreach (Process p in this.localProcsAll)
                    {
                        try
                        {
                            if (p.Id == Int32.Parse(inp))
                            {
                                // Terminate a specific process
                                p.Kill();
                                Console.WriteLine("Process {0} is terminated ... ", p.ProcessName);
                                Console.ReadLine();
                            }

                        }
                        catch (Exception e)
                        {
                            Console.Out.WriteLine("Your input is not valid ...", e.Message);
                            break;
                        }
                    }
                }
            }
        }

        public void printIdByName()
        {
            Process[] currentProc = Process.GetProcesses();

            foreach (Process process in currentProc)
            {
                Console.WriteLine($"Name: {process.ProcessName}   ID: {process.Id}");
            }

            bool stop = false;
            while (!stop)
            {
                
                Console.WriteLine("give me the name of a process and i will give you the ID or type \"stop\" to cancel");
                string input = Console.ReadLine();

                if (input == "stop")
                {
                    stop = true;
                }
                else
                {
                    foreach (Process process in currentProc)
                    {
                        if (process.ProcessName == input)
                        {
                            Console.WriteLine($"the process you requested has ID: {process.Id}");
                        }
                    }                 
                }
            }
        }
    }
}