////////////////////////////////////////////////////////////////////////////////////
// Receiver.cs - service for MessagePassingComm                                   //
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
* - Receiver which implements the public methods:
 *   ---------------------------------------------
 *   - start            : creates instance of ServiceHost which services incoming messages
 *   - postMessage      : Sender proxies call this message to enqueue for processing
 *   - getMessage       : called by Receiver application to retrieve incoming messages
 *   - close            : closes ServiceHost
 *   - openFileForWrite : opens a file for storing incoming file blocks
 *   - writeFileBlock   : writes an incoming file block to storage
 *   - closeFile        : closes newly uploaded file
 *   - getFileName      : gets filenames stored in Receiver Storage
 *   
 * Maintenance History:
 * --------------------
 * ver 2.2 : 27 Nov 2017
 * - Added getFileName function
 * ver 2.1 : 28 Oct 2017
 * - Edited openFileForWrite function by adding a parameter, string fileStorage,
 to it
 * - Updated a prologue
 * - Updated public interface documentation
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
using System.IO;
using System.ServiceModel;
using System.Threading;

namespace MessagePassingComm
{
    ///////////////////////////////////////////////////////////////////
    // Receiver class - receives CommMessages and Files from Senders
    public class Receiver : IMessagePassingComm
    {
        private static string serviceFileStorage = Directory.GetCurrentDirectory() + "/Storage/";
        public static SWTools.BlockingQueue<CommMessage> rcvQ { get; set; } = null;
        ServiceHost commHost = null;
        FileStream fs = null;
        string lastError = "";

        /*----< constructor >------------------------------------------*/

        public Receiver()
        {
            if (rcvQ == null)
                rcvQ = new SWTools.BlockingQueue<CommMessage>();
        }
        /*----< create ServiceHost listening on specified endpoint >---*/
        /*
         * baseAddress is of the form: http://IPaddress or http://networkName
         */
        public void start(string baseAddress, int port)
        {
            string address = baseAddress + ":" + port.ToString() + "/IPluggableComm";
            TestUtilities.putLine(string.Format("starting Receiver on thread {0}", Thread.CurrentThread.ManagedThreadId));
            createCommHost(address);
        }
        /*----< create ServiceHost listening on specified endpoint >---*/
        /*
         * address is of the form: http://IPaddress:8080/IPluggableComm
         */
        public void createCommHost(string address)
        {
            WSHttpBinding binding = new WSHttpBinding();
            Uri baseAddress = new Uri(address);
            commHost = new ServiceHost(typeof(Receiver), baseAddress);
            commHost.AddServiceEndpoint(typeof(IMessagePassingComm), binding, baseAddress);
            commHost.Open();
        }
        /*----< enqueue a message for transmission to a Receiver >-----*/

        public void postMessage(CommMessage msg)
        {
            msg.threadId = Thread.CurrentThread.ManagedThreadId;
            TestUtilities.putLine(string.Format("sender enqueuing message on thread {0}", Thread.CurrentThread.ManagedThreadId));
            rcvQ.enQ(msg);
        }
        /*----< retrieve a message sent by a Sender instance >---------*/

        public CommMessage getMessage()
        {
            CommMessage msg = rcvQ.deQ();
            if (msg.type == CommMessage.MessageType.closeReceiver)
                close();
            return msg;
        }
        /*----< close ServiceHost >------------------------------------*/

        public void close()
        {
            //TestUtilities.putLine("closing receiver - please wait");
            //Console.Out.Flush();
            commHost.Close();
        }

        /*---< called by Sender's proxy to open file on Receiver >-----*/

        public bool openFileForWrite(string name)
        {
            try
            {
                //serviceFileStorage = fileStorage;
                string writePath = Path.Combine(serviceFileStorage, name);
                fs = File.OpenWrite(writePath);
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }

        public byte[] openFileForRead(string name)
        {
                //serviceFileStorage = fileStorage;
                string writePath = Path.Combine(serviceFileStorage, name);
                fs = File.OpenRead(writePath);
                long bytesRemaining;
                bytesRemaining = fs.Length;
                long bytesToRead;
                byte[] blk;
                while (true)
                {
                    bytesToRead =  bytesRemaining;
                    blk = new byte[bytesToRead];
                    long numBytesRead = fs.Read(blk, 0, (int)bytesToRead);
                    bytesRemaining -= numBytesRead;

                    if (bytesRemaining <= 0)
                        break;
                }
              
                return blk;
            }
        /*----< write a block received from Sender instance >----------*/

        public bool writeFileBlock(byte[] block)
        {
            try
            {
                fs.Write(block, 0, block.Length);
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }
        /*----< close Receiver's uploaded file >-----------------------*/

        public void closeFile()
        {
            fs.Close();
        }
        /*----< get filenames stored in Receiver Storage  >-----------------------*/
        public List<string> getFileName(string pattern)
        {
            string path = serviceFileStorage;
            string[] files = System.IO.Directory.GetFiles(path, pattern);
            List<string> storedFileList = new List<string>();
            foreach (string file in files)
            {
                string filename = Path.GetFileName(file);
                storedFileList.Add(filename);
            }
            return storedFileList;
        }
#if (TEST_RCVR)
static void Main(string[] args)
{
    verbose = true;
    TestUtilities.vbtitle("testing Message-Passing Communication", '=');

    /*----< uncomment to see Sender & Receiver testing >---------*/
    Receiver rcvr = new Receiver();
    rcvr.start("http://localhost", 8080);
    rcvr.getMessage();
}     
#endif
    }
}