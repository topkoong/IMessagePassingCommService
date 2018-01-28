////////////////////////////////////////////////////////////////////////////////////
// TestHarness.cs - Runs tests by loading dlls and invoking test()                //              
// ver 1.0                                                                        //
//                                                                                //
// Platform    : Windows 10 Home x64, Lenovo IdeaPad 700, Visual Studio 2017      //
// Environment : C# Class Library                                                 //
// Application : Build Server for CSE681-SMA, Fall 2017                           //  
// Author: Theerut Foongkiatcharoen, EECS Department, Syracuse University         //
//         tfoongki@syr.edu                                                       //
// Source: Ammar Salman, EECS Department, Syracuse University                     //
//         assalman@syr.edu                                                       //
////////////////////////////////////////////////////////////////////////////////////
/*
* Package Operations:
* ===================
*
* - TestHarness establishes a connection through Comm channel once it is invoked.
*  Builder creates a folder named Storage using createDirectory function.
* - TestHarness checks any kind of messages through checkMessage function because a checkMessage function is 
directly designated for performing a background process on background Thread.
* - TestHarness checks if there's a test request and DllFilesSent message through checkMessage function using MessagePassingComm service.
* - If there's a test request message, TestHarness will deserializes TestRequest.xml 
using Serialization package through xmlDeserialization function.
* - Then, it will ask for required files, matching a test request after deserialization.
* - After that, TestHarness will check if there's a DllFilesSent message. If so, it will test the test libraries.
* - Finally, TestHarness creates a TestLog file and sends it to RepoMock via MessagePassingComm service.
* 
* Public Interface 
* ---------------- 
* createDirectory    : creates a folder named Storage
* startTestHarness   : runs chkMessageThread as a background thread
* checkMessage       : runs as a background thread to check any kind of messages including TestRequest and DLLFIleSent message
* xmlDeserialization : deserializes an XML document and constructs objects from the given path and filename
* TestAllFiles      :  tests all dll files from the path given by calling LoadAndTest function
* LoadAndTest       :  tests a dll file from the path given and logs the test result.
* replyRequestedDllFilesToChildProc  :  sets a message type to reply, adds required dll files as a list in message argument
and sends ia message to a particular child builder via MessagePassingComm service
* sendTestLogToRepo  : sends a testlog to Repo
* 
* Required Files:
* ---------------
* TestHarness.cs, CommunicatorBase.cs, Environment.cs
*
* Build Command:
* ---------------
* csc Executive.cs ...
* 
* Maintenance History:
* --------------------
* ver 1.1 : Nov 30, 2017
* - Added startTestHarness function
* ver 1.0 : Oct 7, 2017
* - Updated public interface documentation
* - Edited comments
* - Changed public string variables to private variables
* - Added Test stubs
* - first release
* ver 0.6 : Oct 6, 2017
* - Updated processMessage function
* - Added TestAllFiles function
* - Fixed LoadAndTest function
* - Updated public interface documentation
* ver 0.5 : Oct 5, 2017
* - Added LoadAndTest function
* - Updated public interface documentation
* ver 0.4 : Oct 4, 2017
* - Updated processMessage function
* - Updated xmlDeserialization function
* - Updated public interface documentation
* - first release
* ver 0.3 : Oct 2, 2017
* - Fixed classes that need to be serialized
* - Fixed xmlDeserialization function
* - Updated public interface documentation
* ver 0.2 : Sep 21, 2017
* - Updated processMessage function
* ver 0.1 : Sep 20, 2017
* - Added classes that need to be serialized
* - Added copyFileType function
* - Added a prologue
* - Added public interface documentation
* - Added processMessage function
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MessagePassingComm;
using Serialization;

namespace TestHarness
{
    public class TestHarness
    {
        private StringWriter _LogBuilder;
        private bool _innerResult;
        private bool _invocationResult;
        private Comm comm;
        private string _childPort;
        private int thPort = 9050;
        private List<string> _requiredTestFileNameList = new List<string>();
        private string _testHarnessStoragePath;
        private string _testRequestFileName;
        private string _fullpath;
        private List<string> _files = new List<string>();
        Thread chkMessageThread = null;

        public string Log
        {
            get { return _LogBuilder.ToString(); }
        }

        public bool InnerResult
        {
            get { return _innerResult; }
        }

        public bool InvocationResult { get { return _invocationResult; } }
        List<Type> testDrivers = new List<Type>();

        public string TestRequestFileName
        {
            get { return _testRequestFileName; }
            set { _testRequestFileName = value; }
        }

        public List<string> Files
        {
            get { return _files; }
            set { _files = value; }
        }

        public TestHarness()
        {
            Console.WriteLine("TestHarness instance has been invoked");
            comm = new Comm("http://localhost", thPort);
            _testHarnessStoragePath = Directory.GetCurrentDirectory() + "/Storage/";
            createDirectory(_testHarnessStoragePath);
            _LogBuilder = new StringWriter();
        }

        /*--------< A class that needs to be deserialized >--------------*/
        [XmlRoot("TestRequest")]
        public class TestRequest
        {
            [XmlElement("TestLibrary")] public List<string> TestLibraries;
        }

        /*----< Creates a folder named Storage >----*/

        public void createDirectory(string storagePath)
        {
            if (!Directory.Exists(storagePath))
                Directory.CreateDirectory(storagePath);
            _fullpath = Path.GetFullPath(storagePath);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("TestHarness has created a folder located in {0} \r\n", _fullpath);
            Console.ResetColor();
        }
        /*--------< runs chkMessageThread as a background thread >----*/
        public void startTestHarness()
        {
            Console.Title = "TestHarness";
            chkMessageThread = new Thread(checkMessage);
            chkMessageThread.Start();
        }
        /*--------< runs as a background thread to check any kind of messages including TestRequest and DLLFIleSent message >----*/
        private void checkMessage()
        {
            while (true)
            {
                Console.WriteLine("TestHarness is checking an incoming message via MessagePassingComm service on thread {0}", Thread.CurrentThread.ManagedThreadId);
                CommMessage crcvMsg = comm.getMessage();
                if (crcvMsg.command == "TestRequest")
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("TestHarness has received a TestRequest" +
                                      " message from child via MessagePassingComm service on thread {0}", Thread.CurrentThread.ManagedThreadId);
                    Console.ResetColor();
                    crcvMsg.show();
                    _childPort = crcvMsg.arguments[0];
                    if (_childPort != null)
                        crcvMsg.arguments.RemoveAt(0);
                    if (crcvMsg.from == "http://localhost:" + _childPort + "/IPluggableComm")
                    {
                        _testRequestFileName = crcvMsg.arguments[0];
                        Thread.Sleep(300);
                        xmlDeserialization<string>(_testHarnessStoragePath, _testRequestFileName);
                        replyRequestedDllFilesToChildProc(_files);
                    }
                }
                else if (crcvMsg.command == "DllFilesSent")
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("TestHarness has received dllfiles" +
                                      " message from child via MessagePassingComm service on thread {0}", Thread.CurrentThread.ManagedThreadId);
                    Console.ResetColor();
                    crcvMsg.show();
                    _childPort = crcvMsg.arguments[0];
                    if (_childPort != null)
                        crcvMsg.arguments.RemoveAt(0);
                    if (crcvMsg.from == "http://localhost:" + _childPort + "/IPluggableComm")
                    {
                        Thread.Sleep(1500);
                        TestAllFiles();
                    }
                }
                else
                    TestUtilities.putLine(string.Format("There's no incoming message on thread {0}\r\n", Thread.CurrentThread.ManagedThreadId));
            }
        }


        /*--------< Deserializes an XML document and constructs objects from the given path and filename >--------------*/
        public void xmlDeserialization<T>(string _testHarnessStoragePath, string _testRequestFileName)
        {
            ToAndFromXml xDeserialization = new ToAndFromXml();
            List<TestRequest> newTrq = xDeserialization.FromXmlOld<List<TestRequest>>(_testHarnessStoragePath, _testRequestFileName);
            foreach (TestRequest trq in newTrq)
            {
                foreach (string tlb in trq.TestLibraries)
                    _files.Add(tlb);
            }
        }
        /*--------< Tests all dll files from the path given by calling LoadAndTest function >--------------*/
        public void TestAllFiles()
        {
            if (!Directory.Exists(_testHarnessStoragePath))
                Console.WriteLine("Path is not existed");
            Console.WriteLine("in {0}\r\n", _testHarnessStoragePath);
            string[] files = Directory.GetFiles(_testHarnessStoragePath, "*.dll");
            string testLogFileName = "TestLog" + _childPort + ".txt";
            foreach (string file in files)
            {
                Console.WriteLine("in {0}\r\n", file);
                LoadAndTest(file);
                Console.Write("\r\n\r\n  Log:\r\n{0}", Log);
                System.IO.File.AppendAllText(_testHarnessStoragePath + "/" + testLogFileName, Log);
            }
            sendTestLogToRepo(testLogFileName);
        }
        /*--------< Tests a dll file from the path given and logs the test result. >--------------*/
        public bool LoadAndTest(string Path)
        {
            _invocationResult = false;
            _innerResult = false;
            Console.Write("\r\n  loading: \"{0}\"", Path);
            // save the original output stream for Console
            TextWriter _old = Console.Out;
            // flush whatever was (if anything) in the log builder
            _LogBuilder.Flush();
            Console.Write("\r\n  Testing library {0}", Path);
            Console.Write("\r\n ==========================================================================");
            try
            {
                // set the Console output to print in the LogBuilder
                Console.SetOut(_LogBuilder);
                Console.Write("\r\n\r\n  Loading the assembly ... ");
                Assembly asm = Assembly.LoadFrom(Path);
                Console.Write(" Success \r\n  Checking Types");
                Type[] types = asm.GetTypes();
                foreach (Type type in types)
                {
                    Type itest = type.GetInterface("ITest");
                    MethodInfo testMethod = type.GetMethod("test");
                    if (type.IsClass && testMethod != null && testMethod.ReturnType == typeof(bool))
                    {
                        Console.Write("\r\n    Found '{1}' in {0}", type.ToString(), testMethod.ToString());
                        Console.Write("\r\n  Invoking Test method '{0}'",
                            testMethod.DeclaringType.FullName + "." + testMethod.Name);
                        Console.SetOut(_LogBuilder); // set the Console output to print in the LogBuilder
                        _innerResult = (bool) testMethod.Invoke(Activator.CreateInstance(type), null); // invoke the test method
                        if (_innerResult) Console.Write("\r\n\r\n  Test Passed.\r\n\r\n");
                        else Console.Write("\r\n\r\n  Test Failed.\r\n\r\n");
                        Console.SetOut(_old); // set the Console output back to its orig  inal (StandardOutput that shows on the screen)
                        _invocationResult = true;
                    }
                }
                if (!_invocationResult)
                    Console.Write("\r\n\r\n  Could not find 'bool Test()' in the assembly.\r\n  Make sure it implements ITest\r\n  Test failed");
                return _invocationResult;
            }
            catch (Exception ex)
            {
                Console.Write("\r\n\r\n  Error: {0}", ex.Message);
                // in case of an exception while invoking test, we need to set the console output back to its original i'm setting it after the previous print so that if the invokation
                // of Test() threw an exception, it will show in the Log string
                Console.SetOut(_old);
                _invocationResult = false;
                return _invocationResult;
            }
        }


        /*----< Sets a message type to reply, adds required dll files as a list in message argument
        and sends ia message to a particular child builder via MessagePassingComm service >----*/

        private void replyRequestedDllFilesToChildProc(List<string> dllFileName)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.reply);
            csndMsg.command = "requestedDllFiles";
            csndMsg.author = "Theerut Foongkiatcharoen";
            csndMsg.from = "http://localhost:9050/IPluggableComm"; //testHarness
            csndMsg.to = "http://localhost:" + _childPort + "/IPluggableComm";
            csndMsg.arguments = dllFileName;
            comm.postMessage(csndMsg);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (string dllFile in dllFileName)
                Console.WriteLine("Replying a child builder address: {0} that Test Harness requires dllfiles: {1}", csndMsg.to, dllFile);
            Console.ResetColor();
        }

        /*----< Sends a testlog to Repo >----*/

        private void sendTestLogToRepo(string testLog)
        {
            Console.WriteLine("TestHarness is sending a testlog to Repo Mock\n");
            /*----< the load time of establishing connection to Repo Mock >----*/
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("TestHarness is establishing a connection to Repo Mock\n");
            Console.ResetColor();
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "TestLog";
            csndMsg.author = "Theerut Foongkiatcharoen";
            csndMsg.to = "http://localhost:9081/IPluggableComm";
            csndMsg.from = "http://localhost:9050/IPluggableComm"; //testHarness
            csndMsg.arguments.Add(testLog);
            comm.postMessage(csndMsg);
            Thread.Sleep(500);
            bool testFileTransfer = true;
            testFileTransfer = comm.postFile(testLog, _testHarnessStoragePath);
            string _fullpath = Path.GetFullPath(_testHarnessStoragePath);
            if (testFileTransfer)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("TestHarness sending a file: {0} from {1} to RepoMock is successful\n", testLog, _fullpath);
                Console.ResetColor();
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("TestHarness sending a file: {0} from {1} to RepoMock failed\n", testLog, _fullpath);
                Console.ResetColor();
            }
        }
        //----< test stub >------------------------------------------------
#if (TEST_TESTHARNESS)
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory("../TestHarness/");
            TestHarness th = new TestHarness();
            th.startTestHarness();
        }
#endif
    }

}
