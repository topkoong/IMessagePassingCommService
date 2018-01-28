////////////////////////////////////////////////////////////////////////////////////
// ChildProc.cs - demonstrate creation of multiple .net processes                 //
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
* - A child process receives processID and port from command-line arguments set by Mother Builder
when starting the application, and it establishes a connection via MessagePassingComm service once it is invoked.
*
* - Also, it creates a folder named ChildStorage_ and its ID appended which is set by Mother Builder using createDirectory function.
* 
* - A child process calls sendReadyMsgToMBuilder function to send the ready message to Mother Builder via MessagePassingComm service.
* 
* - A child process checks any kind of messages through checkMessage because a checkMessage function is 
directly designated for performing a background process on background Thread.
*
* - A child process examines whether its mother builder has sent a build request to it yet.
through checkMessage function.
*
* - If a mother builder has sent a build request to it, it will send 
a testfile request to RepoMock so it can ask for files by calling sendBuildFileRequestMsgToRepo.
*
* - A child process checks if there's an incoming quit message or not through checkMessage function.
* 
If a mother builder has sent a quit message to it, it will set a message type to closeSende, and 
it will free resources associated with it and close ifself.

 * This package defines three classes:
 * - BuildRequest which serializes an XmlElement: Author, Test
 *   ----------------------------------------------------------
 * - Test which serializes an XmlElement: Testfile
 *   ----------------------------------------------------------
 * - ChildProc which is automatically set with a reference to 
 *   the first Window object to be instantiated in the AppDomain.
 *   ------------------------------------------------------------
 *   - ChildProc                                : instantiates the ChildProc and sets member variables 
 *   - createDirectory                          : creates a folder named ChildStorage_ and appended its ID given by Mother Builder
 *   - sendReadyMsgToMBuilder                   : sends a ready message to Mother Builder 
using Message-Passing Communication, implemented with Windows Communication Foundation (WCF) and runs chkMessageThread as a background thread
 *   - checkMessage                             : runs as a background thread to check any kind of messages including build request and quit message
 *   - sendBuildFileRequestMsgToRepo             : sends a message that contains a Test file request to Repo
so that it can ask for required test files
 *   - xmlDeserialization                       : deserializes an XML document and constructs objects from the given XML String
 *   - Build                                    : uses a process class to generate one dll file for each test and creates a BuildLog in clientStorage.
 *   - getFilesFromRepo                         : downloads the cited files from Repomock after deserialization
 *   - sendXMLFilestoTH                         : sends an XML Test Request to Test Harnes
 *   - sendDLLFilestoTH                         : sends test libraries (DLL files) to Test Harness 
 *   - objSerialization                         : serializes any objects to XML that represents test requests
 *   - getFiles                                 : finds all the files, matching pattern, in the entire directory tree rooted at repo.storagePath.
 *   - getFilesHelper                           : private helper function for getFiles function
 * 
/*
 * Added references to:
 * - System.ServiceModel
 * - System.Runtime.Serialization
 * - IMessagePassingCommService
 * - MessagePassingCommService
 * - Serialization
 * - TestUtilities
/*
* Required Files:
* ---------------
* IMPCommService.cs MPCommService.cs BlockingQueue.cs Sender.cs Receiver.cs SpawnProc.cs ChildProc.cs, TestUtilities.cs
*
* Build Command:
* ---------------
* csc ChildProc.cs
* 
* Maintenance History:
* --------------------
* ver 2.2 : Nov 29, 2017
* - Edited sendDLLFilestoTH function 
* - Added getFiles function
* - Added getFilesHelper function
* - Added TestRequest class
* - Added objSerialization function
* ver 2.1 : Nov 28, 2017
* - Added sendXMLFilestoTH function
* - Added sendDLLFilestoTH function
* - Added getFilesFromRepo function
* - Added buildAllFile function
* - Added Build function
* ver 2.0 : Oct 29, 2017
* - Added checkMessage function
* - Edited xmlDeserialization function
* - Deleted checkBuildRequestMessage function
* - Updated public interface documentation
* - second release
* ver 1.7 : Oct 28, 2017
* - Edited sendBuildFileRequestMsgToRepo
* - Updated public interface documentation
* ver 1.5 : Oct 27, 2017
* - Edited sendBuildFileRequestMsgToRepo
* - Edited sendReadyMsgToMBuilder function
* - Edited checkBuildRequestMessage function
* - Updated public interface documentation
* ver 1.4 : Oct 26, 2017
* - Added sendBuildFileRequestMsgToRepo
* - Edited xmlDeserialization function
* - Edited sendReadyMsgToMBuilder function
* - Edited checkBuildRequestMessage function
* - Updated public interface documentation
* ver 1.3 : Oct 25, 2017
* - Added xmlDeserialization
* - Edited sendReadyMsgToMBuilder
* - Edited checkBuildRequestMessage function
* - Added ChildProc constructor
* - Updated public interface documentation
* ver 1.2 : Oct 24, 2017
* - Added sendReadyMsgToMBuilder
* - Added checkBuildRequestMessage function
* - Updated public interface documentation
* ver 1.1 : Oct 20, 2017
* - Edited Main function provided by Professor Jim Fawcett
* - Added a prologue
* - Added public interface documentation
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using MessagePassingComm;
using Serialization;

namespace ChildProc
{
    /*--------< Classes that need to be serialized >--------------*/

    [XmlRoot("BuildRequest")]
    public class BuildRequest
    {
        [XmlElement("Author")] public string Author;
        [XmlElement("Test")] public List<Test> Tests { get; set; }
    }
    public class Test
    {
        [XmlElement("Testfile")] public List<string> Testfile { get; set; }
    }
    /*--------< A class that need to be serialized >--------------*/
    public class TestRequest
    {
        [XmlElement("TestLibrary")] public List<string> TestLibraries;
    }
    public class ChildProc
    {
        Thread chkMessageThread = null;
        private Comm comm;
        private StringWriter _LogBuilder;
        private string _childStoragePath;
        private string _processID;
        private int _childPort;
        private List<string> _buildFiles = new List<string>();
        private string _testRequestFilename;
        private List<string> _receivedTestFilesList = new List<string>();
        private string _fullpath;
        private string buildRequestContent;
        private List<string> _files = new List<string>();
        private List<Test> _tests = new List<Test>();

        /*--------< Instantiates the ChildProc and sets member variables >--------------*/

        public ChildProc(string pID, int port)
        {
            comm = new Comm("http://localhost", port);
            _processID = pID;

            /*----< Sets a child builder port >----*/

            _childPort = port;

            /*----< Sets a child builder path >----*/

            _childStoragePath = Directory.GetCurrentDirectory() + "/ChildStorage_" + _processID;

            createDirectory(_childStoragePath);
            _LogBuilder = new StringWriter();
        }
        /*----< Creates a folder named ChildStorage_ and appended its ID given by Mother Builder >----*/

        public void createDirectory(string storagePath)
        {
            if (!Directory.Exists(storagePath))
                Directory.CreateDirectory(storagePath);
            _fullpath = Path.GetFullPath(storagePath);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Child builder no. {0} has created a folder located in {1} \n", _processID, _fullpath);
            Console.ResetColor();
        }

        /*----< Sends a ready message to Mother Builder via MessagePassingComm service
        and runs checkMessage function as a background thread >----*/

        public void sendReadyMsgToMBuilder()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Child builder no. {0} is sending a ready message to Mother Builder via MessagePassingComm service", _processID);
            Console.ResetColor();
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "ready";
            csndMsg.author = "Theerut Foongkiatcharoen";
            csndMsg.to = "http://localhost:8081/IPluggableComm";
            csndMsg.from = "http://localhost:" + _childPort + "/IPluggableComm";
            comm.postMessage(csndMsg);

            /*----< Checks if mother builder has sent a build request to it yet >----*/

            chkMessageThread = new Thread(checkMessage);
            chkMessageThread.Start();
        }

        /*----< Runs as a background thread to check any kind of messages
         including build request and quit message >----*/

        private void checkMessage()
        {
            while (true)
            {
                CommMessage crcvMsg = comm.getMessage();
                crcvMsg.show();
                if (crcvMsg.command == "buildRequest" && crcvMsg.from == "http://localhost:8081/IPluggableComm")
                {
                    Console.WriteLine("Child builder no. {0} checks a build request message sent by mother builder via MessagePassingComm service on thread {1}\n ", _processID, Thread.CurrentThread.ManagedThreadId);
                    //sendBuildFileRequestMsgToRepo(crcvMsg.arguments[0]);
                    buildRequestContent = crcvMsg.arguments[0];
                    xmlDeserialization<string>(buildRequestContent);
                    Build(_tests);
                    objSerialization();
                    sendXMLFilestoTH(_testRequestFilename);
                }
                else if (crcvMsg.command == "requestedDllFiles" && crcvMsg.from == "http://localhost:9050/IPluggableComm")
                {
                    List<string> dllFilename = new List<string>();
                    foreach (string file in _files)
                    {
                        dllFilename.Add(Path.GetFileName(file));
                    }
                    sendDLLFilestoTH(dllFilename);
                }
                else if (crcvMsg.command == "quit" && crcvMsg.from == "http://localhost:8081/IPluggableComm")
                {
                    Console.WriteLine("Child builder no. {0} checks a quit message sent by mother builder via MessagePassingComm service on thread {1}\n ", _processID, Thread.CurrentThread.ManagedThreadId);
                    CommMessage csndMsg = new CommMessage(CommMessage.MessageType.closeSender);
                    comm.postMessage(csndMsg);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Child builder no. {0} is closing its sender on thread {1}\n", _processID, Thread.CurrentThread.ManagedThreadId);
                    Console.ResetColor();
                    break;
                }
                else
                    TestUtilities.putLine(string.Format("There's no incoming message on thread {0}\n", Thread.CurrentThread.ManagedThreadId));
            }
            Process currentProcess = new Process();
            currentProcess.Close();
        }

        public string Log
        {
            get { return _LogBuilder.ToString(); }
        }


        /*----< Sends a buildlog to Repo >----*/

        private void sendBuildLogToRepo(string buildLog)
        {
            Console.WriteLine("Child Builder no. {0} is sending a buildlog to Repo Mock\n", _processID);
            /*----< the load time of re-establish connection to Repo Mock >----*/
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Child Builder no. {0} is re-establishing a connection to Repo Mock\n", _processID);
            Console.ResetColor();
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "BuildLog";
            csndMsg.author = "Theerut Foongkiatcharoen";
            csndMsg.to = "http://localhost:9081/IPluggableComm";
            csndMsg.from = "http://localhost:" + _childPort + "/IPluggableComm";
            csndMsg.arguments.Add(_childPort.ToString());
            csndMsg.arguments.Add(buildLog);
            comm.postMessage(csndMsg);
            Thread.Sleep(500);
            bool testFileTransfer = true;
            testFileTransfer = comm.postFile(buildLog, _childStoragePath);
            string _fullpath = Path.GetFullPath(_childStoragePath);
            if (testFileTransfer)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Child Builder no. {0} sending a file: {1} from {2} to RepoMock is successful\n", _processID, buildLog, _fullpath);
                Console.ResetColor();
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("GUI is uploading a file: {0} from {1} to RepoMock failed\n", buildLog, _fullpath);
                Console.ResetColor();
            }
        }



        /*--------< Deserializes an XML document and constructs objects from the given XML string>--------------*/

        public void xmlDeserialization<T>(string xmlString)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Child Builder no. {0} is deserializing a BuildRequest in form of string.\n", _processID);
            Console.ResetColor();
            ToAndFromXml xDeserialization = new ToAndFromXml();
            List<BuildRequest> newBrq = xDeserialization.FromXml<List<BuildRequest>>(xmlString);

            foreach (BuildRequest brq in newBrq)
            {
                foreach (Test testElement in brq.Tests)
                    _tests.Add(testElement);
            }
        }

        /*----< Build *.dll file >----*/
        private void Build(List<Test> testElements)
        {
            string buildLogFileName = "BuildLog" + _childPort + ".txt";
            TextWriter _old = Console.Out; _LogBuilder.Flush(); int testElementCounter = 1;
            foreach (Test test in testElements)
            {
                foreach (string tfs in test.Testfile)
                    _buildFiles.Add(tfs);
                getFilesFromRepo(buildRequestContent);
                Console.Write("\r\nBuilding dll files");
                Console.Write("\r\n==========================================================================");
                string buildFile = "";
                string dllFilename = "";
                dllFilename = "dllFile_" + testElementCounter + "_" + _childPort;
                foreach (string bF in _buildFiles)
                    buildFile = buildFile + " " + bF;
                Console.SetOut(_LogBuilder); Console.WriteLine("\r\nBuilding {0}", buildFile);
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.EnvironmentVariables["PATH"] = "%path%;C:/Windows/Microsoft.NET/Framework64/v4.0.30319";
                p.StartInfo.Arguments = "/Ccsc /target:library /out:" + dllFilename + ".dll " + buildFile;
                p.StartInfo.WorkingDirectory = _childStoragePath;  // Specify relative path
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                string output = p.StandardOutput.ReadToEnd(); string errors = p.StandardError.ReadToEnd();
                p.WaitForExit();
                string buildLog = ""; /*----< Creates a buildLog and stores it in a temporary folder >----*/
                if (p.ExitCode == 0)
                {
                    buildLog += "Build Result: Success\r\n\r\n";
                    buildLog += output; Console.WriteLine(buildLog);
                    buildLog = "";
                }
                else
                {
                    buildLog += "Build Result: Failed: [See error message in BuildLog" + _childPort + ".txt]\r\n\r\n";
                    buildLog += errors; buildLog += output;
                    Console.WriteLine(buildLog);
                    buildLog = "";
                }
                System.IO.File.AppendAllText(_childStoragePath + "/" + buildLogFileName, Log);
                _LogBuilder.Flush(); testElementCounter += 1;
            }
            Console.SetOut(_old); sendBuildLogToRepo(buildLogFileName);
        }

        /*--------< Serializes any objects to XML that represents test requests >--------------*/
        public void objSerialization()
        {
            TestRequest tr = new TestRequest();
            getFiles("*.dll");
            List<TestRequest> testRequestLists = new List<TestRequest>();
            List<string> tl = new List<string>();
            foreach (string file in _files)
            {
                Console.WriteLine("Adding a file :" + Path.GetFileName(file) + "to TestRequest.xml file");
                tl.Add(Path.GetFileName(file));
                tr.TestLibraries = tl;
                testRequestLists.Add(tr);
            }
            ToAndFromXml xSerialization = new ToAndFromXml();
            string testRequest = "TestRequest";
            _testRequestFilename = xSerialization.ToXml(testRequestLists, _childStoragePath, testRequest); // Serialization

        }

        private void getFilesFromRepo(string brqXmlString)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Child Builder no. {0} is connecting to RepoMock on thread {1}\n", _processID, Thread.CurrentThread.ManagedThreadId);
            Console.ResetColor();
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "requiredBuildFiles";
            csndMsg.author = "Theerut Foongkiatcharoen";
            csndMsg.to = "http://localhost:9081/IPluggableComm";
            csndMsg.from = "http://localhost:" + _childPort + "/IPluggableComm";
            //xmlDeserialization<string>(brqXmlString);
            csndMsg.arguments.Add(_childPort.ToString());
            csndMsg.arguments.AddRange(_buildFiles);
            comm.postMessage(csndMsg);

            /*----< the load time of initial connection >----*/

            Thread.Sleep(1000);
            bool testFileTransfer = true;
            foreach (string buildFile in _buildFiles)
            {
                testFileTransfer = comm.getFile(buildFile, _childStoragePath);
                if (testFileTransfer)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Child Builder no. {0} downloading a file: {1} from RepoMock to {2} is successful\n", _processID, buildFile, _fullpath);
                    Console.ResetColor();
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Child Builder no. {0} downloading a file: {1} from RepoMock to {2} failed \n", _processID, buildFile, _fullpath);
                    Console.ResetColor();
                }
            }
        }

        /*----< find all the files in the given path >-----------*/
        /*
        *  Finds all the files, matching pattern, in the entire 
        *  directory tree rooted at repo.storagePath.
        */
        public void getFiles(string pattern)
        {
            _files.Clear();
            getFilesHelper(_childStoragePath, pattern);
        }

        /*----< private helper function for RepoMock.getFiles >--------*/
        private void getFilesHelper(string path, string pattern)
        {
            string[] tempFiles = Directory.GetFiles(path, pattern);
            for (int i = 0; i < tempFiles.Length; ++i)
            {
                tempFiles[i] = Path.GetFullPath(tempFiles[i]);
            }
            _files.AddRange(tempFiles);

            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                getFilesHelper(dir, pattern);
            }
        }

        /*----< sends an XML Test Request to Test Harnes >--------*/

        private void sendXMLFilestoTH(string _testRequestFilename)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Child Builder no. {0} is establishing a connection to Test Harness\n", _processID);
            Console.ResetColor();
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "TestRequest";
            csndMsg.author = "Theerut Foongkiatcharoen";
            csndMsg.to = "http://localhost:9050/IPluggableComm";
            csndMsg.from = "http://localhost:" + _childPort + "/IPluggableComm";
            csndMsg.arguments.Add(_childPort.ToString());
            csndMsg.arguments.Add(_testRequestFilename);
            comm.postMessage(csndMsg);

            /*----< the load time of initial connection >----*/

            Thread.Sleep(300);
            bool testFileTransfer = true;
            testFileTransfer = comm.postFile(_testRequestFilename, _childStoragePath);
            if (testFileTransfer)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Child Builder no. {0} sending a file: {1} from {2} to Test Harness is successful\n", _processID, _testRequestFilename, _fullpath);
                Console.ResetColor();
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Child Builder no. {0} sending a file: {1}  from {2} to Test Harness failed \n", _processID, _testRequestFilename, _fullpath);
                Console.ResetColor();
            }
        }
        /*----< send test libraries (DLL files) to Test Harness >----*/
        private void sendDLLFilestoTH(List<string> dllFilenameList)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Child Builder no. {0} is establishing a connection to Test Harness\n", _processID);
            Console.ResetColor();
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "DllFilesSent";
            csndMsg.author = "Theerut Foongkiatcharoen";
            csndMsg.to = "http://localhost:9050/IPluggableComm";
            csndMsg.from = "http://localhost:" + _childPort + "/IPluggableComm";
            csndMsg.arguments.Add(_childPort.ToString());
            csndMsg.arguments.AddRange(dllFilenameList);
            comm.postMessage(csndMsg);

            /*----< the load time of initial connection >----*/

            Thread.Sleep(500);
            bool testFileTransfer = true;
            foreach (string dllFile in dllFilenameList)
            {
                testFileTransfer = comm.postFile(dllFile, _childStoragePath);
                if (testFileTransfer)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Child Builder no. {0} sending a file: {1} from {2} to Test Harness is successful\n", _processID, dllFile, _fullpath);
                    Console.ResetColor();
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Child Builder no. {0} sending a file: {1} from {2} to Test Harness failed \n", _processID, dllFile, _fullpath);
                    Console.ResetColor();
                }
            }
        }
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory("../ChildProc/");
            string pID = args[0]; // assigns process ID
            int childPort = Int32.Parse(args[1]); // assigns child process port
            ChildProc childProc = new ChildProc(pID, childPort);
            Console.Title = "ChildProc";
            Console.Write("\n  Child Process");
            Console.Write("\n ====================");
            if (args.Count() == 0)
            {
                Console.Write("\n  please enter integer value on command line");
                return;
            }
            else
            {
                Console.Write("\n  Hello from child process #{0}, running on Port: {1}\n\n", args[0], args[1]);
                childProc.sendReadyMsgToMBuilder();
            }
        }
    }
}
