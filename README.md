# IMessagePassingCommService
A Build Server prototype for use in Software Development Federation. Used a process pool and message passing communication to enable concurrent operation.

In general, software development is complex, especially as applications, teams, and deployment infrastructure grow in complexity themselves. Also, software projects involve lots of files consist of a great number of packages and million lines of codes so keeping track of all of these becomes an intolerable burden, and it is arduous and time-consuming when developers attempt to merge their accumulated code changes. These factors combined makes it harder to deliver updates to customers quickly. Hence, with the Remote Build Server concept that efficiently and strongly supports continuous integration, it considerably enhances software quality and development process, and developers can catch bugs early, spend less time debugging, and deliver software more rapidly.
Our Federation of servers contains Client Mock, Repository Mock, Test Harness Mock, and Build Server, each providing a dedicated service for continuous integration, and top level of eleven major packages are the following: Client Mock, Repository Mock, Test Harness Mock, Sender, Parser, Builder, File Manager, Logger, File Handler, Environment Manager, and Build. Each package is accurately described in the Partition section. In addition, users of the Remote Build Server are Developers, Project Managers, Quality Assurance (QA), SMA Instructor and Teaching Assistants. How they will interact with the application is described in detail. Rational and technical explanation for each user is also provided in Uses section. Moreover, the major activities of the Build Server are well represented in the Activities Diagram section, and critical issues regarding performance and need and value arise, but ultimate solutions are provided in detail.