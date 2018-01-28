////////////////////////////////////////////////////////////////////////////////////
// MPCommService.cs - service for MessagePassingComm                              //
// ver 2.1                                                                        //
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
 * Started this project with C# Console Project wizard
 * - Added references to:
 *   - System.ServiceModel
 *   - System.Runtime.Serialization
 *   - System.Threading;
 *   - System.IO;
 *   
 * Package Operations:
 * -------------------
 * This package defines a single class:

 * - Comm which implements, using Sender and Receiver instances, the public methods:
 *   -------------------------------------------------------------------------------
 *   - postMessage      : send CommMessage instance to a Receiver instance
 *   - getMessage       : retrieves a CommMessage from a Sender instance
 *   - postFile         : called by a Sender instance to transfer a file
 *    
 * The Package also implements the class TestPCommService with public methods:
 * ---------------------------------------------------------------------------
 * - testSndrRcvr()     : test instances of Sender and Receiver
 * - testComm()         : test Comm instance
 * - compareMsgs        : compare two CommMessage instances for near equality
 * - compareFileBytes   : compare two files byte by byte
 *   
 * Maintenance History:
 * --------------------
 * ver 2.1 : 28 Oct 2017
 * - Edited postFile function by adding a parameter, string fileStorage,
 to postFile function, and send it as an argument to openFileForWrite
 * - second release
 * ver 2.0 : 19 Oct 2017
 * - renamed namespace and several components
 * - eliminated IPluggable.cs
 * ver 1.0 : 14 Jun 2017
 * - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MessagePassingComm
{


    ///////////////////////////////////////////////////////////////////
    // Comm class combines Receiver and Sender

    public class Comm
    {
        private Receiver rcvr = null;
        private Sender sndr = null;

        /*----< constructor >------------------------------------------*/
        /*
         * - starts listener listening on specified endpoint
         */
        public Comm(string baseAddress, int port)
        {
            rcvr = new Receiver();
            rcvr.start(baseAddress, port);
            sndr = new Sender(baseAddress, port);
        }
        /*----< post message to remote Comm >--------------------------*/

        public void postMessage(CommMessage msg)
        {
            sndr.postMessage(msg);
        }
        /*----< retrieve message from remote Comm >--------------------*/

        public CommMessage getMessage()
        {
            return rcvr.getMessage();
        }
        /*----< called by remote Comm to upload file >-----------------*/

        public bool postFile(string filename, string fileStorage)
        {
            return sndr.postFile(filename, fileStorage);
        }

        /*----< called by remote Comm to upload file >-----------------*/

        public List<string> getFileName(string pattern)
        {
            return sndr.getFileName(pattern);
        }

        /*----< called by remote Comm to download file >-----------------*/

        public bool getFile(string filename, string fileStorage)
        {
            return sndr.getFile(filename, fileStorage);
        }
    }
    ///////////////////////////////////////////////////////////////////
    // TestPCommService class - tests Receiver, Sender, and Comm

    public class TestPCommService
    {
        private const string clientFileStorage = "../../../GUI/Storage";
        private const string serviceFileStorage = "../../../RepoMock/RepoStorage";
        /*----< collect file names from client's FileStore >-----------*/

        public List<string> getClientFileList()
        {
            List<string> names = new List<string>();
            string[] files = Directory.GetFiles(clientFileStorage);
            foreach (string file in files)
            {
                names.Add(Path.GetFileName(file));
            }
            return names;
        }
        /*----< compare CommMessages property by property >------------*/
        /*
         * - skips threadId property
         */
        public bool compareMsgs(CommMessage msg1, CommMessage msg2)
        {
            bool t1 = (msg1.type == msg2.type);
            bool t2 = (msg1.to == msg2.to);
            bool t3 = (msg1.from == msg2.from);
            bool t4 = (msg1.author == msg2.author);
            bool t5 = (msg1.command == msg2.command);
            //bool t6 = (msg1.threadId == msg2.threadId);
            bool t7 = (msg1.errorMsg == msg2.errorMsg);
            if (msg1.arguments.Count != msg2.arguments.Count)
                return false;
            for (int i = 0; i < msg1.arguments.Count; ++i)
            {
                if (msg1.arguments[i] != msg2.arguments[i])
                    return false;
            }
            return t1 && t2 && t3 && t4 && t5 && /*t6 &&*/ t7;
        }
        /*----< compare binary file's bytes >--------------------------*/

        public bool compareFileBytes(string filename)
        {
            TestUtilities.putLine(string.Format("testing byte equality for \"{0}\"", filename));

            string fileSpec1 = Path.Combine(clientFileStorage, filename);
            string fileSpec2 = Path.Combine(serviceFileStorage, filename);
            try
            {
                byte[] bytes1 = File.ReadAllBytes(fileSpec1);
                byte[] bytes2 = File.ReadAllBytes(fileSpec2);
                if (bytes1.Length != bytes2.Length)
                    return false;
                for (int i = 0; i < bytes1.Length; ++i)
                {
                    if (bytes1[i] != bytes2[i])
                        return false;
                }
            }
            catch (Exception ex)
            {
                TestUtilities.putLine(string.Format("\n  {0}\n", ex.Message));
                return false;
            }
            return true;
        }
        public static bool verbose { get; set; }
        /*----< test Sender and Receiver classes >---------------------*/

        public static bool testSndrRcvr()
        {
            TestPCommService testComService = new TestPCommService();
            TestUtilities.vbtitle("testing Sender & Receiver");

            bool test = true;
            Receiver rcvr = new Receiver();
            rcvr.start("http://localhost", 8080);
            Sender sndr = new Sender("http://localhost", 8080);

            CommMessage sndMsg = new CommMessage(CommMessage.MessageType.request);
            sndMsg.command = "show";
            sndMsg.author = "Jim Fawcett";
            sndMsg.to = "http://localhost:8080/IPluggableComm";
            sndMsg.from = "http://localhost:8080/IPluggableComm";

            sndr.postMessage(sndMsg);
            CommMessage rcvMsg;
            // get connection message
            rcvMsg = rcvr.getMessage();
            if (verbose)
                rcvMsg.show();
            // get first info message
            rcvMsg = rcvr.getMessage();
            if (verbose)
                rcvMsg.show();
            if (!testComService.compareMsgs(sndMsg, rcvMsg))
                test = false;
            TestUtilities.checkResult(test, "sndMsg equals rcvMsg");
            TestUtilities.putLine();

            sndMsg.type = CommMessage.MessageType.closeReceiver;
            sndr.postMessage(sndMsg);
            rcvMsg = rcvr.getMessage();
            if (verbose)
                rcvMsg.show();
            if (!testComService.compareMsgs(sndMsg, rcvMsg))
                test = false;
            TestUtilities.checkResult(test, "Close Receiver");
            TestUtilities.putLine();

            sndMsg.type = CommMessage.MessageType.closeSender;
            if (verbose)
                sndMsg.show();
            sndr.postMessage(sndMsg);

            TestUtilities.putLine("last message received\n");
            return test;
        }
#if (TEST_SNDRANDRCVR)
static void Main(string[] args)
{
    verbose = true;
    TestUtilities.vbtitle("testing Message-Passing Communication", '=');

    /*----< uncomment to see Sender & Receiver testing >---------*/
    TestUtilities.checkResult(testSndrRcvr(), "Sender & Receiver");
    TestUtilities.putLine();

    TestUtilities.putLine("Press key to quit\n");
    if (verbose)
        Console.ReadKey();
}     
#endif
    }
}
