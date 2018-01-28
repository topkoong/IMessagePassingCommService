////////////////////////////////////////////////////////////////////////////////////
// Sender.cs - service for MessagePassingComm                                     //
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
 * - Sender which implements the public methods:
 *   -------------------------------------------
 *   - connect          : opens channel and attempts to connect to an endpoint, 
 *                        trying multiple times to send a connect message
 *   - close            : closes channel
 *   - postMessage      : posts to an internal thread-safe blocking queue, which
 *                        a sendThread then dequeues msg, inspects for destination,
 *                        and calls connect(address, port)
 *   - postFile         : attempts to upload a file in blocks
 *   - getLastError     : returns exception messages on method failure
 *   
 * Maintenance History:
 * --------------------
 * ver 2.2 : 31 Oct 2017
 * - Edited close function by adding try and catch block to prevent an exception from happening
 when a channel has already been closed by the other end.
 * ver 2.1 : 28 Oct 2017
 * - Edited postFile function by adding a parameter, string fileStorage,
 to postFile function, and send it as an argument to openFileForWrite
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
    // Sender class - sends messages and files to Receiver
    public class Sender
    {
        private const long blockSize = 1024;
        private IMessagePassingComm channel;
        private ChannelFactory<IMessagePassingComm> factory = null;
        private SWTools.BlockingQueue<CommMessage> sndQ = null;
        private int port = 0;
        private string fromAddress = "";
        private string toAddress = "";
        Thread sndThread = null;
        int tryCount = 0, maxCount = 10;
        string lastError = "";
        string lastUrl = "";

        /*----< constructor >------------------------------------------*/

        public Sender(string baseAddress, int listenPort)
        {
            port = listenPort;
            fromAddress = baseAddress + listenPort.ToString() + "/IMessagePassingComm";
            sndQ = new SWTools.BlockingQueue<CommMessage>();
            TestUtilities.putLine(string.Format("starting Sender on thread {0}", Thread.CurrentThread.ManagedThreadId));
            sndThread = new Thread(threadProc);
            sndThread.IsBackground = true;
            sndThread.Start();
        }
        /*----< creates proxy with interface of remote instance >------*/

        public void createSendChannel(string address)
        {
            EndpointAddress baseAddress = new EndpointAddress(address);
            WSHttpBinding binding = new WSHttpBinding();
            factory = new ChannelFactory<IMessagePassingComm>(binding, address);
            channel = factory.CreateChannel();
        }
        /*----< attempts to connect to Receiver instance >-------------*/

        public bool connect(string baseAddress, int port)
        {
            toAddress = baseAddress + ":" + port.ToString() + "/IPluggableComm";
            return connect(toAddress);
        }

        /*----< attempts to connect to Receiver instance >-------------*/
        /*
         * - attempts a finite number of times to connect to a Receiver
         * - first attempt to send will throw exception of no listener
         *   at the specified endpoint
         * - to test that we attempt to send a connect message
         */
        public bool connect(string toAddress)
        {
            int timeToSleep = 500;
            createSendChannel(toAddress);
            CommMessage connectMsg = new CommMessage(CommMessage.MessageType.connect);
            while (true)
            {
                try
                {
                    channel.postMessage(connectMsg);
                    tryCount = 0;
                    return true;
                }
                catch (Exception ex)
                {
                    if (++tryCount < maxCount)
                    {
                        TestUtilities.putLine("failed to connect - waiting to try again");
                        Thread.Sleep(timeToSleep);
                    }
                    else
                    {
                        TestUtilities.putLine("failed to connect - quitting");
                        lastError = ex.Message;
                        return false;
                    }
                }
            }
        }
        /*----< closes Sender's proxy >--------------------------------*/

        public void close()
        {
            try
            {
                if (factory != null)
                    factory.Close();
            }
            catch
            {
                Console.WriteLine("Tried to close proxy that has already been closed");
            }
        }
        public List<string> getFileName(string pattern)
        {
            return channel.getFileName(pattern);
        }


        /*----< processing for receive thread >------------------------*/
        /*
         * - send thread dequeues send message and posts to channel proxy
         * - thread inspects message and routes to appropriate specified endpoint
         */
        void threadProc()
        {
            while (true)
            {
                TestUtilities.putLine(string.Format("sender enqueuing message on thread {0}",
                    Thread.CurrentThread.ManagedThreadId));

                CommMessage msg = sndQ.deQ();
                if (msg.type == CommMessage.MessageType.closeSender)
                {
                    TestUtilities.putLine("Sender send thread quitting");
                    break;
                }
                if (msg.to == lastUrl)
                {
                    channel.postMessage(msg);
                }
                else
                {
                    close();
                    if (!connect(msg.to))
                        return;
                    lastUrl = msg.to;
                    channel.postMessage(msg);
                }
            }
        }
        /*----< main thread enqueues message for sending >-------------*/

        public void postMessage(CommMessage msg)
        {
            sndQ.enQ(msg);
        }
        /*----< uploads file to Receiver instance >--------------------*/

        public bool postFile(string fileName, string fileStorage)
        {
            FileStream fs = null;
            long bytesRemaining;

            try
            {
                string path = Path.Combine(fileStorage, fileName);
                fs = File.OpenRead(path);
                bytesRemaining = fs.Length;
                channel.openFileForWrite(fileName);
                while (true)
                {
                    long bytesToRead = Math.Min(blockSize, bytesRemaining);
                    byte[] blk = new byte[bytesToRead];
                    long numBytesRead = fs.Read(blk, 0, (int)bytesToRead);
                    bytesRemaining -= numBytesRead;

                    channel.writeFileBlock(blk);
                    if (bytesRemaining <= 0)
                        break;
                }
                channel.closeFile();
                fs.Close();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
            return true;
        }

        public bool getFile(string fileName, string fileStorage)
        {
            FileStream fs = null;
            try
            {
                string path = Path.Combine(fileStorage, fileName);
                fs = File.OpenWrite(path);
                byte[] blk = channel.openFileForRead(fileName);
                channel.closeFile();
                fs.Write(blk, 0, blk.Length);
                fs.Close();

            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
            return true;
        }
#if (TEST_SNDR)
static void Main(string[] args)
{
    Sender sndr = new Sender("http://localhost", 8080);
    sndr.postMessage(sndMsg);
}     
#endif
    }
}