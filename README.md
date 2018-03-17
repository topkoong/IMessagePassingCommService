# Remote Build Server (C#, .NET Framework, WPF, WCF)
## A Build Server prototype for use in Software Development Federation. Used a process pool and message passing communication to enable concurrent operation.

- [x] Package prologues and function prologues are included.
- [x] The public interface documentation — holding a short description of the package operations, the required files to build the package, and the build process and the maintenance history — is included.
- [x] Each function includes some comments to describe its operation.


In general, software development is complex, especially as applications, teams, and deployment infrastructure grow in complexity themselves. Also, software projects involve lots of files consist of a great number of packages and million lines of codes so keeping track of all of these becomes an intolerable burden, and it is arduous and time-consuming when developers attempt to merge their accumulated code changes. These factors combined makes it harder to deliver updates to customers quickly. Hence, with the Remote Build Server concept that efficiently and strongly supports continuous integration, it considerably enhances software quality and development process, and developers can catch bugs early, spend less time debugging, and deliver software more rapidly.
Our Federation of servers contains Client Mock, Repository Mock, Test Harness Mock, and Build Server, each providing a dedicated service for continuous integration, and top level of eleven major packages are the following: Client Mock, Repository Mock, Test Harness Mock, Sender, Parser, Builder, File Manager, Logger, File Handler, Environment Manager, and Build. Each package is accurately described in the Partition section. In addition, users of the Remote Build Server are Developers, Project Managers, Quality Assurance (QA), SMA Instructor and Teaching Assistants. How they will interact with the application is described in detail. Rational and technical explanation for each user is also provided in Uses section. Moreover, the major activities of the Build Server are well represented in the Activities Diagram section, and critical issues regarding performance and need and value arise, but ultimate solutions are provided in detail.

# Application Activities

The client can select files from GUI to upload from local storage to Repository or select files in Repository for packaging into a test library, a test element specifying driver and tested files, added to a build request structure. The GUI will display which selected files client added to build Then, the client builds a build request to generate an XML build request, and the GUI will display the build request structure that the client has just created. After that, the client selects newly generated build requests and send them to the Repository Mock, and the GUI will display that client has transferred the build request structure and XML build request files successfully. Then, the Mother Builder will receive a build request message forwarded by Repo and check the ready message queue to see which child processes are available. Once it found that there’s a child process available, it will send a build request message to that child process. When the child builder receives a build request message, it will deserialize the XML build request and asks repo for the cited files. Once it receives those required files, it will build test libraries if there are more than one test element, generate an XML test request and send the log to Repo and test libraries to Test Harness. Test Harness will then check a test request message, and if there is a test request message, it will deserialize the XML test request and loading test assemblies into application domain, recording pass status, logging execution details, sending test result to Repository. After that, the client can view the test logs and build logs from GUI.

# Partitions

This section will layout the technical breakdown of required functionality for the build server and each task becomes a package and class candidate. Here are the following eleven major packages and build server classes: Client, Main Window(GUI), IMPCommService (Comm), MPCommService (Comm Message), Blocking Queue, Repository Mock, Mother Builder, Child Builder, Build Configure, Test Harness, File Manager, Logger, XML Handler, AppDomainMgr, MpCommService and IMPCommService comprise of multiple packages, meaning I will not go into lower level these two packages. However, we are talking about a Remote Build Server so I will give a clear explanation regarding what all functionalities it will require in detail.

# Client
Client Mock can select files to upload from local storage to Repository or select files for packaging into a test library, a test element specifying driver and tested files, added to a build request structure. Also, it can specify the number of child builders to be started, shut down Pool Processes, and send build request structures to the repository for storage and transmission to the Mother Builder.

## -	Main Window(GUI)
Main Window is implemented with WPF and using message-passing communication (WCF). It gets file lists from the Repository and enables building an XML Build Request by selecting file names, uploading selected files, driver and tested files, as well as browsing to find files to build from the Mock Repository, and provides mechanisms to start the main Builder (mother process), to specify the number of child builders to be started. It also provides the facility to ask the Mother Builder to shut down its Pool Processes, to send build request structures to the repository for storage and transmission to the Build Server, and the capability of repeating that process to add other test libraries to the build request structure. 

## -	IMPCommService (Comm)
IMPCommService is service interface for MPCommService including interface used for message passing and file transfer and class representing serializable messages. It also defines a service contract given that the service contract specifies what operations the service supports. Each method in the IMPCommService corresponds to a specific service operation.

## -	MPCommService (Comm Message)
MPCommService implements service interface. It implements postMessage, getMessage, and postFile functions using Sender and Receiver instances. A postMessage function sends CommMessage instance to a Receiver instance. A getMessage function retrieves a CommMessage from a Sender instance, and a postFile         function is called by a Sender instance to transfer a file.

## -	Blocking Queue
This package implements a generic blocking queue using a Monitor and lock, which is equivalent to using a condition variable with a lock. It also demonstrates communication between two threads using an instance of the queue. If the queue is empty when a reader attempts to dequeue an item, the reader will block until the writing thread enqueues an item. Therefore, waiting is efficient.

## -	Repository Mock
Repository Mock is a storage house of the whole system due to the fact that it is a central location in which data is stored and managed, meaning this is where the source code lives and it contain all the code after the user checks in their changes to it. It is a simple server, running it own process, using our Message-Passing Communication, to send and receive requests and replies. Also, it uses File Manager package to handle file retrieval/storage to accept files including source codes, XML build requests, and Test Libraries.

## -	Mother Builder
Mother Builder provides a queue of build requests and ready messages so that it can manage Child Builder by spawning the specified number of child processes given by client as well as passing them Build Requests when it receives ready messages from them.

## -	Child Builder
Child Builder, spawned by Mother Builder, uses a Message-Passing Communication Service built with WCF to access messages from the Mother Builder process by sending a ready message to inform a mother builder that it is ready to retrieve a build request.
Once the Child Builder receives a build request message, it will load files, matching a retrieved Build Request, from Repository after performing XML deserialization. After that it will build those files into libraries, send them to the Test Harness, and uses to Logger to alert the client whether the build is successful or not and sends the log to Repo.

## -	Build Configure
Since the toolchain, containing Debugger, Compiler, Linker, and another tool for a specific programming language to produce executable files and shared libraries for the target, comes into play immediately after the merge commit happens, Build configure package sets up the environment that supports development and operation tasks and includes a specific set of tool integrations to prepare for build.

## -	Test Harness
Test Harness Mock provides the system for automated integration testing. It runs tests based on test requests and libraries sent by the Child Builder if the build succeeds through a Message-Passing Communication Service built with WCF. Generally, it runs a library consisting of test driver and a small set of tested packages, loading test assemblies into application domain, recording pass status, logging execution details, sending test result to Repository.

## -	AppDomainMgr
It gets the domain manager that was provided by the host when the application domain was initialized so that test harness can use it to load test assemblies.

## -	Logger
Logger is in charge of recording logs for test libraries built by Build server and test executed by Test Harness Mock owing to keeping track of all the test results. The user can review logs to ensure tests passed and troubleshoot when the tests failed. As a result, logging increases efficiency of codes.

## -	File Manager
File manager provides file management capabilities. It creates, delete temp directory, searches for files with a specific file extension or name.

## -	XML Handler
XML Handler is responsible for serialization of objects in XML format and deserialization of an XML file back to an object. Generally, serialization is a process by which an object's state is transformed in some serial data format, such as XML or binary format and is the process of converting an object into a form that can be readily transported. Deserialization, however, is used to convert the byte of data, such as XML or binary data, to object type.

# Critical Issues

## -	Need and Value
Although a remote build server is the need for today’s organization and reduces integration problems allowing us to deliver software more rapidly, the initial cost of implementing this including initial installations and configurations is costly and can create disruption if it makes no difference in minimizing the cost of integration or freeing developers from manual tasks. It is not worth it that the organization spends a significant amount of money on a basic remote build server that is not helpful and does not overcome the learning curve before implementing this, meaning all utilities of application will be useless. For instance, if it does not help us identify our integration problems earlier or it does not save us time and verify code that much, the organization waste the money for nothing. This could be one of the critical issues in terms of ease of use and would rather be federation issues than the developers.
### -	Solution
The business stakeholder should contemplate if it is worth for this type of project or the organization really needs this so they can minimize the cost of integration or freeing developers from manual tasks

## -	Performance
Build Server in the model will have lots of build requests on it, and if something goes wrong, the whole system will go down and crash, and this could be a critical issue. In terms of time consumption, if one task throws an unexpected exception only the process that was running the task will go down instead of having the whole system going down. Similarly, there will be an issue storing and processing that can throw an unexpected exception when the user is sending too many test requests to the Repository Mock. Also, if C++ headers include each other in the absence of an include guard, a file will need to be processed multiple times and can cause significant build delays in large systems. Also, the compiler will endlessly preprocess the header and use CPU time until the computer gives in and halts.
### -	Solution
For the Build Server issue, providing process-pool helps increasing performance and reliability because the process-pool helps dividing tasks on different processes which can be used to efficiently delegate work over multiple CPU cores.
For the Repository Mock issue, using a blocking queue approach to deal with test requests can solve the problem due to the fact that a thread trying to dequeue from an empty queue is blocked until some other thread inserts an item into the queue. A thread trying to enqueue an item in a full queue is blocked until some other thread makes space in the queue, either by dequeuing one or more items or clearing the queue completely. In a similar manner, applying multithreading to this code can enhance performance and concurrency on multi-processor machines by paralyzing the tasks.

## -	Development
Defining a single message structure that works for all messages used in the Federation is quite difficult, and managing Endpoint information for Repository, Mother, and Test Harness.
### -	Solution
A message that contains To and From addresses, Command string or enumeration, List of strings to hold file names, and a string body to hold logs will suffice for all needed operations in case of defining a single message structure that work for all messages used in the Federation. Also, storing Endpoint information in XML file resident with all clients and servers and load at startup can help managing Endpoint information for Repository, Mother Builder, and Test Harness.	



