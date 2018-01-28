//////////////////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs -  a GUI built using Windows Presentation Foundation (WPF)        //
// Interaction logic for MainWindow.xaml                                                //
// ver 1.0                                                                              //
// Environment : C# Windows Application                                                 //
// Platform    : Windows 10 Home x64, Lenovo IdeaPad 700, Visual Studio 2017            //
// Application : Graphical User Interface (GUI) prototype for CSE681-SMA, Fall 2017     //  
// Author: Theerut Foongkiatcharoen, EECS Department, Syracuse University               //
//         tfoongki@syr.edu                                                             //
// Source: Dr. Jim Fawcett, EECS Department, CST 4-187, Syracuse University             //
//         jfawcett @twcny.rr.com                                                       //
//////////////////////////////////////////////////////////////////////////////////////////
/*
 * 
 * Added references to:
 * - System.ServiceModel
 * - System.Runtime.Serialization
 * - IMessagePassingCommService
 * - MessagePassingCommService
 * - Serialization
/*
 * 
 * Package Operations:
 * -------------------
 * This package defines WPF application processing by the client. It also supports viewing local files.
 * 
 * This package defines three classes:
 * - BuildRequest which serializes an XmlElement: Author, Test
 *   ----------------------------------------------------------
 * - Test which serializes an XmlElement: Testfile
 *   ----------------------------------------------------------
 * - MainWindow which is automatically set with a reference to 
 *   the first Window object to be instantiated in the AppDomain.
 *   ------------------------------------------------------------
 *   - MainWindow                            : instantiates the MainWindow in code during application startup.
 *   - ShowPath                              : shows path in textblock
 *   - AddFile                               : adds an item in a WPF Listbox
 *   - Search                                : recursively search all files that match a certain pattern
 *   - readXML                               : creates XML String 
 *   - FindButton_OnClick                    : starts search on asynchronous delegate's thread
 *   - BuildButton_OnClick                   : creates an XML file and displays in Listbox 2
 *   - ObjSerialization                      : serializes any objects to XML that represents build requests, tests, and testfile
 *   - SendQuitMsgToMBuilder                 : sends a quit message to Mother Builder via MessagePassingComm service
 *   - GetFilesHelper                        : helps GetFile function to get fullpath that matches a certain pattern
 *   - GetFiles                              : finds all the files, matching pattern, in the entire directory tree rooted at
 *   - SendMainBuilderButton_OnClick         : start the main Builder (mother process)
 *   - TextBox1_OnTextInput                  : specifies the number of child builders to be started
 *   - ListBox1_OnPreviewMouseDown           : enables a textbox and build button when any mouse button is depressed on ListBox1 Item.
 *   - ShutDownPoolProcessButton_OnClick     : shuts down the main Builder's Pool Processes
 *   by sending a single quit message to the Mother process.
 *   - TextBox1_OnTextChanged                : enables a SendChildBuilder button when the content of
 *   the text box changes and check if the content is empty or not
 *   - SendChildBuilderButton_OnClick        : checks a textbox for an empty string, an integer or a string to prevent
 *   the user from leaving the textbox empty, as well as entering string.
 *   - onLoad                                : provides mechanisms to start the main Builder (mother process),
 specifying the number of child builders to be started
 *   - onClosing                             : shows a goodbye message
 *   - checkMotherRunning                    : checks if the mother process is running or not
 *   - uploadFilesToRepo_OnClick             : calls sendFilestoRepo function to upload selected files (from local storage) in Listbox1
 *   - sendFilestoRepo                       : uploads *.cs files shown in Listbox1 to Repo
 *   - getFilesFromRepo                      : get *.cs files stored in RepoStorage and displays them in Listbox2
 *   - SendBuildRequestToRepo_OnClick        : sends selected Build Requests on Listbox3 to RepositoryMock
 *   - SendBrqMsgToRepo                      : send a build request message to Repo
 *   - AddTestElementButton_OnClick          : adds a test element to a build request structure and display it to a GUI console
 *   - ListBox2_OnPreviewMouseDown           : enables a textbox and build button when any mouse button is depressed on ListBox2 Item.

 * Required Files:
 * ---------------
 * MainWindow.xaml, IMPCommService.cs, Sender.cs, Receiver.cs, SpawnProc.cs
 * 
 * Build Command:
 * ---------------
 * csc MainWIndow.xaml.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.3 : Dec 1, 2017
 * - Added ListBox2_OnPreviewMouseDown button event
 * - Added AddTestElementButton_OnClick button event
 * ver 1.2 : Nov 27, 2017
 * - Added SendBrqMsgToRepo function
 * - Removed SendMsgToMBuilder function
 * - Updated SendBuildRequestToRepo_OnClick button event
 * ver 1.1 : Nov 26, 2017
 * - Added SendBuildRequestToRepo_OnClick button event
 * - Added getFilesFromRepo
 * - Added sendFilestoRepo
 * - Added uploadFilesToRepo_OnClick button event
 * ver 1.0 : Oct 30, 2017
 * - Added checkMotherRunning function
 * - Added onLoad function
 * - Added onClosing function
 * - Updated public interface documentation
 * - first release
 * ver 0.5 : Oct 29, 2017
 * - Edited SendChildBuilderButton_OnClick button event.
 * - Added TextBox1_OnTextChanged button event
 * - Added ShutDownPoolProcessButton_OnClick
 * - Updated public interface documentation
 * ver 0.4 : Oct 28, 2017
 * - Edited SendChildBuilderButton_OnClick button event
 * - Added ListBox1_OnPreviewMouseDown button event
 * - Updated public interface documentation
 * ver 0.4 : Oct 27, 2017
 * - Edited SendChildBuilderButton_OnClick button event
 * - Updated public interface documentation
 * ver 0.3 : Oct 25, 2017
 * - Added SendMsgToMBuilder function
 * - Added readXML function
 * ver 0.2 : Oct 20, 2017
 * - Added ObjSerialization
 * - Edited BuildButton_OnClick button event
 * - Updated public interface documentation
 * ver 0.1 : Oct 10, 2017
 * - Added ShowPath function
 * - Added AddFile function
 * - Added Search function
 * - Added GetFilesHelper function
 * - Added GetFiles function
 * - Added BuildButton_OnClick button event.
 * - Added FindButton_OnClick button event.
 * - Added SendMainBuilderButton_OnClick button event.
 * - Added TextBox1_OnTextInput event
 * - Added a prologue
 * - Added public interface documentation
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Xml.Serialization;
using MessagePassingComm;
using Serialization;
using SWTools;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using SelectionMode = System.Windows.Controls.SelectionMode;

namespace GUI
{
    /// Interaction logic for MainWindow.xaml
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
    /*--------< Instantiates the MainWindow in code during application startup >--------------*/
    public partial class MainWindow : Window
    {
        private static int port = 8182;
        private Comm comm;
        private string _repoStoragePath;
        private string motherBulderFilePath ="..\\SpawnProc\\bin\\Debug\\SpawnProc.exe";
        private string repoMockFilePath = "..\\RepoMock\\bin\\Debug\\RepoMock.exe";
        private string testHarnessFilePath = "..\\TestHarness\\bin\\Debug\\TestHarness.exe";
        private string _textBox;
        private List<Test> tests = new List<Test>();
        private string _brqXMLFileName;
        private List<string> _foundfiles = new List<string>();
        private string defaultPath;
        private List<string> csFileList = new List<string>();
        private List<string> selectedTestFiles = new List<string>();
        IAsyncResult _cbResult;
        private Process motherProcess = null;
        private Process repoProcess = null;
        private Process testHarnessProcess = null;
        /*----< Private Queue holds ready BuildRequests >----*/
        private SWTools.BlockingQueue<string> buildRequests = new BlockingQueue<string>();
        public string RepoStoragePath
        {
            get { return _repoStoragePath; }
            set { _repoStoragePath = value; }
        }
        /*--------< Instantiates the MainWindow in code during application startup >--------------*/
        public MainWindow()
        {
            InitializeComponent();
            Directory.SetCurrentDirectory("../../../GUI/");
            defaultPath = Directory.GetCurrentDirectory() + "/Storage";
            comm = new Comm("http://localhost", port);
            Search(defaultPath, "*.cs");
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.connect);
            csndMsg.to = "http://localhost:9081/IPluggableComm";
            csndMsg.from = "http://localhost:" + port + "/IPluggableComm";
            comm.postMessage(csndMsg);
            Thread.Sleep(500);
            Dispatcher.Invoke(getFilesFromRepo);
        }
        /*-- invoke on UI thread --------------------------------*/
        /*--------< Shows path in textblock >--------------*/
        void ShowPath(string path)
        {
            textBlock1.Text = path;
        }
        /*-- invoke on UI thread --------------------------------*/
        /*--------< Adds an item to a WPF Listbox >--------------*/
        void AddFile(string file)
        {
            string filename = Path.GetFileName(file);
            listBox1.Items.Insert(0, filename);
        }
        /*-- recursive search for files matching pattern --------*/
        void Search(string path, string pattern)
        {
            /* called on asynch delegate's thread */
            if (Dispatcher.CheckAccess())
                ShowPath(path);
            else
                Dispatcher.Invoke(
                    new Action<string>(ShowPath),
                    System.Windows.Threading.DispatcherPriority.Background,
                    new string[] { path }
                );
            string[] files = System.IO.Directory.GetFiles(path, pattern);
            foreach (string file in files)
            {
                if (Dispatcher.CheckAccess())
                    AddFile(file);
                else
                    Dispatcher.Invoke(
                        new Action<string>(AddFile),
                        System.Windows.Threading.DispatcherPriority.Background,
                        new string[] { file }
                    );
            }
            string[] dirs = System.IO.Directory.GetDirectories(path);
            foreach (string dir in dirs)
                Search(dir, pattern);
        }
        /*--------< Creates XML String >--------*/
        private string readXML(string filename)
        {
            string xmlString = System.IO.File.ReadAllText(defaultPath + "/" + filename);
            return xmlString;
        }
        /*-- Start search on asynchronous delegate's thread -----*/
        private void FindButton_OnClick(object sender, RoutedEventArgs e)
        {
            listBox1.Items.Clear();
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            string path = AppDomain.CurrentDomain.BaseDirectory;
            dlg.SelectedPath = path;
            DialogResult result = dlg.ShowDialog();
            defaultPath = dlg.SelectedPath;
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                path = dlg.SelectedPath;
                string pattern = "*.cs";
                Action<string, string> proc = this.Search;
                _cbResult = proc.BeginInvoke(path, pattern, null, null);
            }
        }
        /*--------< Creates an XML file and displays in Listbox 2 >--------*/
        private void BuildButton_OnClick(object sender, RoutedEventArgs e)
        {
            SendMainBuilderButton.IsEnabled = true;
            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = ObjSerialization(tests);
            listBox3.Items.Add(listBoxItem);
            tests.Clear();
        }
        /*--------< adds a test element to a build request structure and display it to a GUI console>--------*/

        private void AddTestElementButton_OnClick(object sender, RoutedEventArgs e)
        {
            selectedTestFiles.Clear();
            foreach (Object obj in listBox2.SelectedItems)
                selectedTestFiles.Add(obj.ToString());
            Test test = new Test();
            List<string> testFiles = new List<string>();
            foreach (var item in selectedTestFiles)
                testFiles.Add(item);
            test.Testfile = testFiles;
            tests.Add(test);

            object testElement = (object) test;
            XmlSerializerNamespaces nmsp = new XmlSerializerNamespaces();
            nmsp.Add("", "");

            var sb = new StringBuilder();
            try
            {
                var serializer = new XmlSerializer(testElement.GetType());
                using (StringWriter writer = new StringWriter(sb))
                {
                    serializer.Serialize(writer, testElement, nmsp);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n  exception thrown:");
                Console.Write("\n  {0}", ex.Message);
            }
            String newTestElement = sb.ToString();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Adding a Test Element to a build request structure");
            Console.WriteLine("\n====================================================\n\n\n");
            Console.ResetColor();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(newTestElement + "\n\n\n");
            Console.ResetColor();

        }


        /*--------< Serializes any objects to XML that represents build requests >--------------*/

        public string ObjSerialization(List<Test> tests)
        {
            BuildRequest br = new BuildRequest();
            List<BuildRequest> buildRequestLists = new List<BuildRequest>();

            br.Tests = tests;
            buildRequestLists.Add(br);
            ToAndFromXml xSerialization = new ToAndFromXml();
            string buildRequest = "BuildRequest";
            _brqXMLFileName = xSerialization.ToXml(buildRequestLists, defaultPath, buildRequest);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Generating " + _brqXMLFileName + "\n");
            Console.WriteLine("\n====================================================\n\n\n");
            Console.ResetColor();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(readXML((_brqXMLFileName)));
            Console.ResetColor();
            return _brqXMLFileName;
        }
        /*--------< Sends a build request message that contains
         XML String to Mother Builder via MessagePassingComm service >--------------*/
        private void SendBrqMsgToRepo()
        {
            while (buildRequests.size() != 0)
            {
                CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
                csndMsg.command = "buildRequest";
                csndMsg.author = "Theerut Foongkiatcharoen";
                csndMsg.to = "http://localhost:9081/IPluggableComm";
                csndMsg.from = "http://localhost:" + port + "/IPluggableComm";
                csndMsg.arguments.Add(buildRequests.deQ());
                comm.postMessage(csndMsg);
            }
        }
        /*--------< Sends a quit message to Mother Builder via MessagePassingComm service >--------*/
        private void SendQuitMsgToMBuilder()
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "quit";
            csndMsg.author = "Theerut Foongkiatcharoen";
            csndMsg.to = "http://localhost:8081/IPluggableComm";
            csndMsg.from = "http://localhost:" + port + "/IPluggableComm";
            csndMsg.arguments.Clear();
            comm.postMessage(csndMsg);
        }

        /*--------< Sends a spawn message to Mother Builder via MessagePassingComm service >--------*/

        private void SendSpawnMsgToMBuilder(int procCount)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "spawn";
            csndMsg.author = "Theerut Foongkiatcharoen";
            csndMsg.to = "http://localhost:8081/IPluggableComm";
            csndMsg.from = "http://localhost:" + port + "/IPluggableComm";
            csndMsg.arguments.Add(procCount.ToString());
            comm.postMessage(csndMsg);
        }
        // helps GetFile function to get fullpath that matches a certain pattern
        private void GetFilesHelper(string path, string pattern)
        {
            string[] tempFiles = System.IO.Directory.GetFiles(path, pattern);
            for (int i = 0; i < tempFiles.Length; ++i)
            {
                tempFiles[i] = System.IO.Path.GetFullPath(tempFiles[i]);
            }
            _foundfiles.AddRange(tempFiles);
            csFileList.AddRange(tempFiles);

            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                GetFilesHelper(dir, pattern);
            }
        }
        /*
        *  Finds all the files, matching pattern, in the entire 
        *  directory tree rooted at repo.storagePath.
        */
        public void GetFiles(string pattern)
        {
            _foundfiles.Clear();
            GetFilesHelper(_repoStoragePath, pattern);
        }
        /*----< Start the main Builder (mother process) >-----------*/
        private void SendMainBuilderButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                int temp;
                if (int.TryParse(textBox1.Text, out temp))
                {
                    if (temp >= 0 && temp <= 30)
                    {
                        if (motherProcess == null)
                        {
                            checkMotherRunning();
                        }
                        else
                        {
                            string message = "Mother Builder is currently running.";
                            MessageBox.Show(message);
                        }
                    }
                    else
                    {
                        string message = "Number of child builders entered exceeds the limit.\n Please specify the number of child builders between 1 - 30";
                        MessageBox.Show(message);
                    }
                }
                else
                {
                    string message = "You entered text instead of a number\n Please specify the number of child builders as number";
                    MessageBox.Show(message);
                }
            }
            else
            {
                string message = "You did not specify the number of child builders";
                MessageBox.Show(message);
            }
            
        }
        /*-----------< Checks if Mother Builder is running >-----------*/

        private void checkMotherRunning()
        {
            string processCount = textBox1.Text;
            motherProcess = new Process();
            testHarnessProcess = new Process();
            try
            {
                Process.Start(motherBulderFilePath, processCount);
                Process.Start(testHarnessFilePath);
                ShutDownPoolProcessButton.IsEnabled = true;
                SendBrqMsgToRepo();
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
            }
        }
        /*-----------< Specifies the number of child builders to be started>-----------*/
        private void TextBox1_OnTextInput(object sender, TextCompositionEventArgs e)
        {
            _textBox = textBox1.Text;
        }

        /*-----------< Enables a textbox and build button when
        any mouse button is depressed on ListBox1 Item >-----------*/
        private void ListBox1_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            BuildButton.IsEnabled = true;
            textBox1.IsEnabled = true;
        }

        /*-----------< Enables a textbox and build button when
        any mouse button is depressed on ListBox2 Item >-----------*/

        private void ListBox2_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            BuildButton.IsEnabled = true;
            textBox1.IsEnabled = true;
        }
        /*-----------< Shuts down the main Builder's Pool Processes by sending
        a single quit message to the Mother process >-----------*/
        private void ShutDownPoolProcessButton_OnClick(object sender, RoutedEventArgs e)
        {
            SendQuitMsgToMBuilder();
            SendChildBuilderButton.IsEnabled = true;
            SendMainBuilderButton.IsEnabled = false;
            string message = "Please select files to build.";
            MessageBox.Show(message);
            listBox2.Items.Clear();
        }
        /*-----------< Enables a SendChildBuilder button when the content of the text box changes
        and check if the content is empty or not >-----------*/
        private void TextBox1_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                if (textBox1.Text != "e.g. 1")
                    SendChildBuilderButton.IsEnabled = true;
            }
            else
            {
                string message = "Please specify the number of child builders";
                MessageBox.Show(message);
            }
        }
        /*-----------< Checks a textbox for an empty string, an integer or a string to prevent
        the user from leaving the textbox empty, as well as entering string >-----------*/
        private void SendChildBuilderButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                int temp;
                if (int.TryParse(textBox1.Text, out temp))
                {
                    if (temp >= 0 && temp <= 30)
                    {
                        string message = "You specified " + temp + " Child Builder";
                        MessageBox.Show(message);
                        if (motherProcess != null)
                            SendSpawnMsgToMBuilder(temp);
                    }
                    else
                    {
                        string message = "Number of child builders entered exceeds the limit.\n"
                                         + "Please specify the number of child builders between 1 - 30";
                        MessageBox.Show(message);
                    }
                }
                else
                {
                    string message = "You entered text instead of a number\n"
                                     + "Please specify the number of child builders as number";
                    MessageBox.Show(message);
                }
            }
            else
            {
                string message = "You did not specify the number of child builders";
                MessageBox.Show(message);
            }
        }

        ///*-----------< provides mechanisms to start the main Builder (mother process),
        //specifying the number of child builders to be started >-----------*/

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            listBox2.SelectAll();
            AddTestElementButton_OnClick(sender, e);
            listBox2.SelectAll();
            AddTestElementButton_OnClick(sender, e);
            BuildButton_OnClick(sender, e); // Creates BuildRequest.xml file
            listBox2.UnselectAll();
            listBox2.SelectAll();
            AddTestElementButton_OnClick(sender, e);
            BuildButton_OnClick(sender, e); // Creates BuildRequest.xml file
            listBox2.SelectAll();
            AddTestElementButton_OnClick(sender, e);
            BuildButton_OnClick(sender, e); // Creates BuildRequest.xml file
            textBox1.Text = "3";             // Specify number of child processes
            listBox3.SelectAll();
            SendBuildRequestToRepo_OnClick(sender, e);
            SendChildBuilderButton_OnClick(sender, e);

            SendMainBuilderButton_OnClick(sender, e); // Starts Mother Builder

        }
        /*-----------< displays a goodbye message >-----------*/
        private void OnClosing(object sender, CancelEventArgs e)
        {
            string message = "Thank you for grading me.";
            MessageBox.Show(message);
        }

        /*-----------< calls sendFilestoRepo function to upload selected files (from local storage) in Listbox1 >-----------*/

        private void UploadFilesToRepo_OnClick(object sender, RoutedEventArgs e)
        {
            listBox2.Items.Clear();
            List<string> selectedItems = new List<string>();
            foreach (Object obj in listBox1.SelectedItems)
                selectedItems.Add(obj.ToString());
            if (repoProcess == null)
            {
                repoProcess = new Process();
                try
                {
                    Process.Start(repoMockFilePath);
                    sendFilestoRepo(selectedItems);
                    Dispatcher.Invoke(getFilesFromRepo);
                }
                catch (Exception ex)
                {
                    Console.Write("\n  {0}", ex.Message);
                }
            }
            else
            {
                sendFilestoRepo(selectedItems);
                Dispatcher.Invoke(getFilesFromRepo);
            }
        }
        /*-----------< gets *.cs files stored in RepoStorage and displays them in Listbox2 >-----------*/
        private void getFilesFromRepo()
        {
            if (repoProcess == null)
            {
                repoProcess = new Process();
                try
                {
                    Process.Start(repoMockFilePath);
                }
                catch (Exception ex)
                {
                    Console.Write("\n  {0}", ex.Message);
                }
            }
          
            List<string> listBoxItem = new List<string>();
            listBoxItem = comm.getFileName("*.cs");
            listBox2.Items.Clear();

            foreach (string item in listBoxItem)
                listBox2.Items.Add(item);
        }

        /*-----------< upload *.cs files shown in Listbox1 to Repo >-----------*/

        private void sendFilestoRepo(List<string> filenameList)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("GUI is connecting to RepoMock on thread {0}\n", Thread.CurrentThread.ManagedThreadId);
            Console.ResetColor();
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "uploadingFiles";
            csndMsg.author = "Theerut Foongkiatcharoen";
            csndMsg.to = "http://localhost:9081/IPluggableComm";
            csndMsg.from = "http://localhost:" + port + "/IPluggableComm";
            csndMsg.arguments = filenameList;
            comm.postMessage(csndMsg);
            Thread.Sleep(500);
            bool testFileTransfer = true;
            foreach (string filename in filenameList)
            {
                testFileTransfer = comm.postFile(filename, defaultPath);
                string _fullpath = Path.GetFullPath(defaultPath);
                if (testFileTransfer)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("GUI is uploading a file: {0} from {1} to RepoMock is successful\n", filename, _fullpath);
                    Console.ResetColor();
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("GUI is uploading a file: {0} from {1} to RepoMock failed\n", filename, _fullpath);
                    Console.ResetColor();
                }
            }
        }

        /*-----------< sends selected Build Requests on Listbox3 to RepositoryMock >-----------*/

        private void SendBuildRequestToRepo_OnClick(object sender, RoutedEventArgs e)
        {
            List<string> selectedBuildRequests = new List<string>();

            foreach (ListBoxItem item in listBox3.SelectedItems)
            {;
                selectedBuildRequests.Add(item.Content.ToString());
            }

            foreach (string buildRequest in selectedBuildRequests)
            {
                if (motherProcess == null)
                    buildRequests.enQ(readXML(buildRequest));
                else
                {
                    buildRequests.enQ(readXML(buildRequest));
                    SendBrqMsgToRepo();
                }
            }
            sendFilestoRepo(selectedBuildRequests);
        }
    }
}
