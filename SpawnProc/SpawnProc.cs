////////////////////////////////////////////////////////////////////////////////////
// SpawnProc.cs - demonstrate creation of multiple .net processes                 //
// ver 2.0                                                                        //
//                                                                                //
// Environment : C# Console Application                                           //
// Platform    : Windows 10 Home x64, Lenovo IdeaPad 700, Visual Studio 2017      //
// Application : Process Pool prototype for CSE681-SMA, Fall 2017                 //  
// Author: Theerut Foongkiatcharoen, EECS Department, Syracuse University         //
//         tfoongki@syr.edu                                                       //
// Source: Dr. Jim Fawcett, EECS Department, CST 4-187, Syracuse University       //
//         jfawcett @twcny.rr.com                                                 //
////////////////////////////////////////////////////////////////////////////////////
/*
* Package Operations:
* ===================
*
* - Mother Builder establishes a connection through Comm channel once it is invoked.
* 
* - Mother Builder calls startMotherBuilder function to demonstrate creation of multiple .net processes.
* 
* - Mother Builder spawns child processes through spawnChildProcesses function.
* 
* - spawnChildProcesses function will gratefully receive the argument from commandline or GUI so that it can spawn
* the number of child processes as many as the number given.
* 
* - Mother Builder checks any kind of messages through checkMessage function because a checkMessage function is 
directly designated for performing a background process on background Thread.
* 
* - Mother Builder checks if there's a ready message in the message sent by a child process
through checkReadyMessage function. If so, Mother Builder will enqueue a ready message into the ready message queue.
* 
* - Mother Builder examines whether there's a build request message in the message command sent by GUI
through MessagePassingComm service. If there's a build request message, 
Mother Builder will enqueue the message argument, which is BuildRequest in form of string, into a build request queue.
* 
* - Mother Builder checks if there's a build request in the build request queue.
through checkBuildRequestQueue function.
* 
* - If so, it will dequeue a readyMessage from the readyMessage queue and send
a buildrequest filename to a particular child process 
that has sent a ready message to it earlier via MessagePassingComm service.
* 
* - Mother Builder examines whether there's an incoming quit message or not through checkMessage function.
If so, it will set a message type to closeReceiver and send(forward)
to its Pool processes through sendQuitMsgToChildProc function. 

* - Mother Builder checks if there's an incoming spawn message
via checkMessage function. If so, it will spawn a new child builder through spawnChildProcesses function.
* 
* 
 * This package defines a single class:
 * - SpawnProc which demonstrates creation of multiple .net processes
 *   ----------------------------------------------------------------
 *   - SpawnProc                         : instantiates the SpawnProc, sets member variables,
and establishes the connection using Message-Passing Communication, implemented with Windows Communication Foundation (WCF).
 *   - startMotherBuilder                : spawns child processes and runs chkMessageThread as a background thread
 *   - sendQuitMsgToChildProc            : sets a message type to closeReceiver and send(forward) it to its Pool processes via MessagePassingComm service.
 *   - checkMessage                      : runs as a background thread to check any kind of messages including build request and ready message
 *   - checkBuildRequestQueue            : examines whether there's a build request in the build request queue,
and then dequeue a build request list in form of string from build request queue
and store it in message argument and sends a build request to the partiular child builder
 *   - spawnChildProcesses               : spawns the number of child processes as many as the number in parameter are given
by calling createProcess function and sends the process ID and portnumber as arguments to them
 *   - createProcess                      : starts a process resource by specifying the name of an
application and a set of command-line arguments, and associates the resource with a new Process component.
/*
* Added references to:
* - System.ServiceModel
* - System.Runtime.Serialization
* - IMessagePassingCommService
* - MessagePassingCommService
* - TestUtilities
/*
* Required Files:
* ---------------
* IMPCommService.cs MPCommService.cs BlockingQueue.cs Sender.cs Receiver.cs SpawnProc.cs ChildProc.cs, TestUtilities.cs
*
* Build Command:
* ---------------
* csc SpawnProc.cs
* 
* Maintenance History:
* --------------------
* ver 2.1 : Nov 27, 2017
* - Updated a prologue
* - Updated public interface documentation
* - Edited checkMessage function
* ver 2.0 : Oct 29, 2017
* - Added sendQuitMsgToChildProc function
* - Added checkMessage function
* - Deleted checkReadyMessage function
* - Deleted checkBuildRequestMessage function
* - Updated public interface documentation
* - second release
* ver 1.4 : Oct 26, 2017
* - Added spawnChildProcesses function
* - Edited startMotherBuilder function
* - Edited checkBuildRequestMessage function
* - Added SpawnProc constructor
* - Updated public interface documentation
* ver 1.3 : Oct 25, 2017
* - Added checkBuildRequestMessage function
* - Added checkBuildRequestQueue function
* - Added checkReadyMessage function
* - Added SpawnProc constructor
* - Edited startMotherBuilder function
* - Updated public interface documentation
* ver 1.2 : Oct 24, 2017
* - Added readyMessage and buildRequests queues
* - Added startMotherBuilder function
* ver 1.1 : Oct 20, 2017
* - Edited createProcess function provided by Professor Jim Fawcett
* - Added a prologue
* - Added public interface documentation
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Threading;
using System.Xml.Xsl;
using MessagePassingComm;
using SWTools;

namespace SpawnProc
{
    public class SpawnProc
    {
        /*----< Private Queue holds ready message >----*/
        private SWTools.BlockingQueue<string> readyMessage = new BlockingQueue<string>();
        /*----< Private Queue holds ready BuildRequests >----*/
        private SWTools.BlockingQueue<string> buildRequests = new BlockingQueue<string>();
        /*----< Sets a mother builder port >----*/
        private int motherPort = 8081;
        Thread chkMessageThread = null;
        Thread chkBrqQueueThread = null;

        /*----< A list of string stores Pool Processes address >----*/
        List<string> childProcAddressess = new List<string>();
        private Comm comm;
        private int childPort = 8089;
        /*----< Instantiates the SpawnProc, sets member variables,
        and establishes the connection via Comm channel >------------*/

        public SpawnProc()
        {
            comm = new Comm("http://localhost", motherPort);
        }
        /*----< Spawns child processes and runs chkMessageThread as a background thread >------------*/

        public void startMotherBuilder(int processCount)
        {
            Console.Title = "SpawnProc";
            Console.Write("\n  Demo Parent Process");
            Console.Write("\n =====================");
            Console.WriteLine("Mother Builder listens to the child processes via MessagePassingComm service on thread {0}\n", Thread.CurrentThread.ManagedThreadId);
            
            /*----< Spawns child builders >----*/

            spawnChildProcesses(processCount);

            /*----< Checks if there's an incoming message.>----*/

            chkMessageThread = new Thread((checkMessage));
            chkMessageThread.Start();

            /*----< Checks if there's a build request in the BuildRequest Queue>----*/

            chkBrqQueueThread = new Thread((checkBuildRequestQueue));
            chkBrqQueueThread.Start();
        }

        /*----< Sets a message type to closeReceiver and send(forward)
         to its Pool processes via MessagePassingComm service >------------*/

        private void sendQuitMsgToChildProc()
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.closeReceiver);
            csndMsg.command = "quit";
            csndMsg.author = "Theerut Foongkiatcharoen";
            csndMsg.from = "http://localhost:8081/IPluggableComm";

            Console.WriteLine(childProcAddressess.Count);

            foreach (string childProcAddr in childProcAddressess)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Mother Builder is sending a quit message to Child Builder address: {0}\n", childProcAddr);
                Console.ResetColor();
                csndMsg.to = childProcAddr;
                Console.WriteLine("Trying to send quit msg to child");
                csndMsg.show();
                comm.postMessage(csndMsg);
                Thread.Sleep(200);
            }
            readyMessage.clear();
            childProcAddressess.Clear();
        }

        /*----< Runs as a background thread to check any kind of messages including
         build request and ready message >------------*/

        private void checkMessage()
        {
            while (true)
            {
                Console.WriteLine("Mother builder checks an incoming message from GUI via Comm on thread {0}\n", Thread.CurrentThread.ManagedThreadId);
                CommMessage crcvMsg = comm.getMessage();
                crcvMsg.show();
                //if (crcvMsg.command == "buildRequest" && crcvMsg.from == "http://localhost:8182/IPluggableComm")
                if (crcvMsg.command == "buildRequest" && crcvMsg.from == "http://localhost:9081/IPluggableComm")
                {
                    Console.WriteLine("Mother builder is enqueuing a build request list in form of string in a build request queue on thread {0}\n", Thread.CurrentThread.ManagedThreadId);
                    List<string> brqXMLStringList = new List<string>();
                    brqXMLStringList = crcvMsg.arguments;
                    foreach (string brqXmlString in brqXMLStringList)
                    {
                        buildRequests.enQ(brqXmlString);
                    }
                }
                else if (crcvMsg.command == "spawn")
                {
                    spawnChildProcesses(Int32.Parse(crcvMsg.arguments[0]));
                }
                else if (crcvMsg.command == "ready")
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Mother Builder is enquing a ready message on thread {0}\n", Thread.CurrentThread.ManagedThreadId);
                    Console.ResetColor();
                    readyMessage.enQ(crcvMsg.from);
                }
                else if (crcvMsg.command == "quit" && crcvMsg.from == "http://localhost:8182/IPluggableComm")
                {
                    sendQuitMsgToChildProc();
                    childPort += 20;
                }
                else
                    TestUtilities.putLine(string.Format("No incoming messages on thread {0}\n", Thread.CurrentThread.ManagedThreadId));
            }
        }

        /*----< Examines whether there's a build request in the build request queue,
        and then dequeue a build request list in form of string from build request queue >------------*/

        private void checkBuildRequestQueue()
        {
            while (true)
            {
                TestUtilities.putLine(string.Format("Mother Builder checks the build request in the Build Request queue on thread {0}\n", Thread.CurrentThread.ManagedThreadId));
                if (buildRequests.size() != 0)
                {
                    string procAddress = readyMessage.deQ();
                    Console.Write("\nAddress: {0}", procAddress);
                    if (!string.IsNullOrEmpty(procAddress))
                        childProcAddressess.Add(procAddress);
                    CommMessage csndMsg = new CommMessage(CommMessage.MessageType.reply);
                    csndMsg.command = "buildRequest";
                    csndMsg.author = "Theerut Foongkiatcharoen";
                    csndMsg.to = procAddress;
                    csndMsg.from = "http://localhost:" + motherPort + "/IPluggableComm";

                    /*----< Dequeues a build request list in form of string
                     from build request queue and stores it in message argument >----*/

                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Mother Builder is dequing the build request in the Build Request queue on thread {0}\n", Thread.CurrentThread.ManagedThreadId);
                    Console.ResetColor();
                    csndMsg.arguments.Add(buildRequests.deQ());

                    /*----< Sends build request to the partiular child process >----*/

                    Console.WriteLine("Trying to send msg to child process");
                    csndMsg.show();
                    comm.postMessage(csndMsg);
                    csndMsg.show();
                }
                else
                    TestUtilities.putLine(string.Format("There're no build requests in the Build Request queue. on thread {0}\n", Thread.CurrentThread.ManagedThreadId));
            }
        }
        
        /*----< Spawns the number of child processes as many as the number in parameter given by 
         calling createProcess function and sends the process ID and portnumber as arguments to them >----*/

        private void spawnChildProcesses(int procCount)
        {
            for (int i = 1; i <= procCount; ++i)
            {
                if (createProcess(i, childPort))
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(" - Spawning Child Builder {0} succeeded\n", i);
                    Console.ResetColor();
                    childPort += 1;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(" - Spawning Child Builder {0} failed\n", i);
                    Console.ResetColor();
                }
            }
        }
        
        /*----< starts a process resource by specifying the name of an application and
        a set of command-line arguments, and associates the resource with a new Process component >----*/

        public static bool createProcess(int pID, int port)
        {
            Console.WriteLine("portno = {0}\n", port);
            string fileName = "..\\ChildProc\\bin\\debug\\ChildProc.exe";
            string commandline = pID.ToString();
            string portno = port.ToString();

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(fileName);
                startInfo.Arguments = commandline + " " + portno;
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory("../SpawnProc/");
            int processCount = 0;
            if (args.Count() == 0)
            {
                Console.Write("\n Please enter number of processes to create on command line ");
                SpawnProc spawnProc = new SpawnProc();
                processCount = Int32.Parse(Console.ReadLine());
                spawnProc.startMotherBuilder(processCount);
            }
            else
            {
                processCount = Int32.Parse(args[0]);
                SpawnProc spawnProc = new SpawnProc();
                spawnProc.startMotherBuilder(processCount);
            }
            Console.Write("\n  Press key to exit");
            Console.ReadKey();
            Console.Write("\n  ");
        }
    }
}
