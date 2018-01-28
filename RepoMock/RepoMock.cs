////////////////////////////////////////////////////////////////////////////////////////////
// RepoMock.cs - Mock Repository for Federation Message-Passing Communication,            //
// implemented with Windows Communication Foundation (WCF).                               //
// ver 1.0                                                                                //
//                                                                                        //
// Environment : C# Console Application                                                   //
// Platform    : Windows 10 Home x64, Lenovo IdeaPad 700, Visual Studio 2017              //
// Application : Process Pool prototype for CSE681-SMA, Fall 2017                         //  
// Author: Theerut Foongkiatcharoen, EECS Department, Syracuse University                 //
//         tfoongki@syr.edu                                                               //
// Source: Dr. Jim Fawcett, EECS Department, CST 4-187, Syracuse University               //
//         jfawcett @twcny.rr.com                                                         //
////////////////////////////////////////////////////////////////////////////////////////////
/*
* Package Operations:
* ===================
*
* - RepoMock establishes a connection through Comm channel once it is invoked.
* 
* - RepoMock calls startMotherBuilder function to check if there's an incoming testfile request message from Pool Processes
through checkTestFileRequestMessage function, running as a background thread.
* 
* - If RepoMock receives a testfile request message from Pool Processes,
it will transfer the required testfiles and reply to Pool Processes that it has sent these files by calling 
replyTestfileSentToChildProc function to send a message via MessagePassingComm service.

* s
 * This package defines a single class:
 * - RepoMock which demonstrates sending test files to Pool Processes
 *   ----------------------------------------------------------------
 *   - RepoMock                             : instantiates the RepoMock, sets member variables,
and establishes the connection using Message-Passing Communication, implemented with Windows Communication Foundation (WCF)
 *   - startRepoMock                        : runs chkTestFileMessageThread as a background thread
 *   - checkrequiredBuildFilesMsgFromChild : checks a required build file message sent by child builder
 *   - forwardBuildRequestToMotherBuilder   : forwards a build request message to Mother Builder
 *   - checkFileUploadFromGUI               : checks a file upload message sent by GUI
 *   - checkBuildLogMsg                     : checks a build log message from Child Builder
 *   - checkTestLogMsg                      : c
 *   - replyBuldfileSentToChildProc        : sets a message type to reply, adds required testfiles as a list in message argument,
and sends ia message to a particular child builder via MessagePassingComm service.
*    - checkMessage : check an incoming message
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
* csc RepoMock.cs
* 
* Maintenance History:
* --------------------

* ver 1.1 : Nov 27, 2017
* - Updated a prologue
* - Updated public interface documentation
* - Added checkMessage function
* - Removed checkTestFileRequestMessage function
* ver 1.0 : Oct 29, 2017
* - Updated a prologue
* - Edited checkTestFileRequestMessage function
* - Edited replyBuldfileSentToChildProc function
* - Updated public interface documentation
* - first release
* ver 0.3 : Oct 28, 2017
* - Edited checkTestFileRequestMessage function
* - Edited replyBuldfileSentToChildProc function
* - Updated public interface documentation
* ver 0.2 : Oct 27, 2017
* - Added checkTestFileRequestMessage function
* - Added replyBuldfileSentToChildProc function
* - Added startRepoMock function
* - Updated public interface documentation
* ver 0.1 : Oct 26, 2017
* - Added checkTestFileRequestMessage function
* - Added startRepoMock function
* - Added RepoMock constructor
* - Added a prologue
* - Added public interface documentation
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessagePassingComm;

namespace RepoMock
{
    public class RepoMock
    {
        private Comm comm;
        private string _childPort;
        private int repoPort = 9081;
        private string _repoStoragePath;
        private List<string> _requiredBuildFileNameList = new List<string>();
        Thread chkMessageThread = null;

        /*--------< Instantiates the RepoMock, sets member variables
        and establishes the connection using Message-Passing Communication
        implemented with Windows Communication Foundation (WCF) >--------*/

        public RepoMock()
        {
            comm = new Comm("http://localhost", repoPort);
            _repoStoragePath = Directory.GetCurrentDirectory() + "/Storage";
        }

        /*--------< Runs chkTestFileMessageThread as a background threadd >--------*/

        public void startRepoMock()
        {
            Console.Title = "RepoMock";
            chkMessageThread = new Thread(checkMessage);
            chkMessageThread.Start();
        }

        private void checkMessage()
        {
            while (true)
            {
                Console.WriteLine("RepoMock is checking an incoming message via MessagePassingComm service on thread {0}", Thread.CurrentThread.ManagedThreadId);
                CommMessage crcvMsg = comm.getMessage();
                if (crcvMsg.command == "requiredBuildFiles")
                {
                    checkRequiredBuildFilesMsgFromChild(crcvMsg);
                }
                else if (crcvMsg.command == "BuildLog")
                {
                    checkBuildLogMsg(crcvMsg);
                }
                else if (crcvMsg.command == "TestLog" && crcvMsg.from == "http://localhost:9050/IPluggableComm")
                {
                    checkTestLogMsg(crcvMsg);
                }
                else if (crcvMsg.command == "uploadingFiles" && crcvMsg.from == "http://localhost:8182/IPluggableComm")
                {
                    checkFileUploadFromGUI(crcvMsg);
                }
                else if (crcvMsg.command == "buildRequest" && crcvMsg.from == "http://localhost:8182/IPluggableComm")
                {
                    forwardBuildRequestToMotherBuilder(crcvMsg);
                }
                else
                    TestUtilities.putLine(string.Format("There's no incoming message on thread {0}\n", Thread.CurrentThread.ManagedThreadId));
            }
        }

        /*----< Checks a required build file message from child >----*/

        void checkRequiredBuildFilesMsgFromChild(CommMessage crcvMsg)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("RepoMock has received a buildfile request message from child via MessagePassingComm service on thread {0}", Thread.CurrentThread.ManagedThreadId);
            Console.ResetColor();
            crcvMsg.show();
            _childPort = crcvMsg.arguments[0];
            if (_childPort != null)
                crcvMsg.arguments.RemoveAt(0);
            _requiredBuildFileNameList = crcvMsg.arguments;
            if (crcvMsg.from == "http://localhost:" + _childPort + "/IPluggableComm")
            {
                foreach (string requestedTestFile in _requiredBuildFileNameList)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Transfering a {0} to Child Process on thread {1}", requestedTestFile, Thread.CurrentThread.ManagedThreadId);
                    Console.ResetColor();
                    replyBuldfileSentToChildProc(requestedTestFile);
                }
            }
        }

        /*----< Checks a build log message from Child >----*/

        void checkBuildLogMsg(CommMessage crcvMsg)
        {
            string buildLog;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("RepoMock has received a buildlog" +
                              " message from child via MessagePassingComm service on thread {0}", Thread.CurrentThread.ManagedThreadId);
            Console.ResetColor();
            crcvMsg.show();
            _childPort = crcvMsg.arguments[0];
            if (_childPort != null)
                crcvMsg.arguments.RemoveAt(0);
            buildLog = crcvMsg.arguments[0];
            if (crcvMsg.from == "http://localhost:" + _childPort + "/IPluggableComm")
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("RepoMock has obtained a {0} from child process on thread {1}\n\n\n", buildLog, Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(1500);
                readFile(buildLog);
                Console.ResetColor();
            }
        }

        /*----< Checks a test log message from Test Harness >----*/
        void checkTestLogMsg(CommMessage crcvMsg)
        {
            string testLog;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("RepoMock has received a testlog message from child via MessagePassingComm service on thread {0}", Thread.CurrentThread.ManagedThreadId);
            Console.ResetColor();
            crcvMsg.show();
            testLog = crcvMsg.arguments[0];
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("RepoMock has obtained a {0} from TestHarness on thread {1}\n\n\n", testLog, Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(1500);
            readFile(testLog);
            Console.ResetColor();
        }

        /*----< Checks a file upload message from GUI >----*/

        void checkFileUploadFromGUI(CommMessage crcvMsg)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("RepoMock has received a file upload message from GUI via MessagePassingComm service on thread {0}", Thread.CurrentThread.ManagedThreadId);
            Console.ResetColor();
            crcvMsg.show();
            List<string> fileUploadList = new List<string>();
            fileUploadList = crcvMsg.arguments;
            foreach (string fileUpload in fileUploadList)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("RepoMock has obtained a {0} from GUI on thread {1}", fileUpload, Thread.CurrentThread.ManagedThreadId);
                Console.ResetColor();
            }
        }

        /*----< Forwards a build request message to Mother Builder >----*/

        void forwardBuildRequestToMotherBuilder(CommMessage crcvMsg)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("RepoMock is forwarding a build request list in form of string to MotherBuilder on thread {0}\n", Thread.CurrentThread.ManagedThreadId);
            Console.ResetColor();
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg = crcvMsg;
            csndMsg.to = "http://localhost:8081/IPluggableComm";
            csndMsg.from = "http://localhost:9081/IPluggableComm";
            comm.postMessage(csndMsg);
        }

        /*----< Sets a message type to reply, adds required testfiles as a list in message argument
        and sends ia message to a particular child builder via MessagePassingComm service >----*/

        private void replyBuldfileSentToChildProc(string buildFileName)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.reply);
            csndMsg.command = "TestFileSent";
            csndMsg.author = "Theerut Foongkiatcharoen";
            csndMsg.from = "http://localhost:9081/IPluggableComm"; //repo
            csndMsg.to = "http://localhost:" + _childPort + "/IPluggableComm";
            csndMsg.arguments.Add(buildFileName);
            comm.postMessage(csndMsg);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Replying a child builder address: {0} that it has sent the required testfiles: {1}", csndMsg.to, buildFileName);
            Console.ResetColor();
        }

        private void readFile(string filename)
        {
            int counter = 0;
            string line;

            // Read the file and display it line by line.  
            System.IO.StreamReader file =
                new System.IO.StreamReader(_repoStoragePath + "/" + filename);
            while ((line = file.ReadLine()) != null)
            {
                System.Console.WriteLine(line);
                counter++;
            }

            file.Close();
            //System.Console.WriteLine("There were {0} lines.", counter);
            // Suspend the screen.  
            //System.Console.ReadLine();
        }

        //private void replyFilesStoredinRepoToGUI(List<string> fileStored)
        //{
        //    CommMessage csndMsg = new CommMessage(CommMessage.MessageType.reply);
        //    csndMsg.command = "FilesStoredinRepo";
        //    csndMsg.author = "Theerut Foongkiatcharoen";
        //    csndMsg.from = "http://localhost:9081/IPluggableComm"; //repo
        //    csndMsg.to = "http://localhost:8182/IPluggableComm";
        //    csndMsg.arguments = fileStored;
        //    comm.postMessage(csndMsg);
        //    Console.BackgroundColor = ConsoleColor.Black;
        //    Console.ForegroundColor = ConsoleColor.Green;
        //    Console.WriteLine("Replying a GUI {0} that it has sent files stored in its directory: {1}", csndMsg.to, fileStored);
        //    Console.ResetColor();
        //}
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory("../RepoMock/");
            RepoMock repo = new RepoMock();
            repo.startRepoMock();
           
        } 

    }
}
