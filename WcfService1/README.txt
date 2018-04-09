/**
\mainpage WCF-Service Service1 Dokumentation

\section zero Prerequisites

To run Servce1, you need 
Visual Studio
to be registered as a user in the webform (the userename and password is needed for most methods, except when running in Mock-Mode)

\section first Functionality

The WCF Service is currently named Service1. It provides a Login() method for a specific form of the company innobis using a password and a username on a http-post-method with a specific session cookie.
It also provides a method GetAllElements() that returns a JSON-String listing the form number, all input fields, labels and checkboxes. 
\sa Login()

\section second Pecularities

The WCF Service is implemented in Factory Pattern. The facory is able to produce either in Mock-Mode or in Real-Mode output. Mock-Mode is independant from the
availability of the form to be parsed. It reads from a text file called Sections.json which is included in the project. Real-Mode however, does call the domain
of the webform to archieve the same purpose.

The languages available in Mock-Mode are English, German and Polish. In Real-Mode you can access more than <a href="https://translator.microsoft.com/help/articles/languages/">60 languages available</a> in the Microsoft Text Translator API.
For example, if you want to have the Form delivered in French, open Service1.svc.cs, and modify the line containing ServiceFactory to "ServiceFactory factory = new ServiceFactory(false,"fr")";


\section third ToDos
- TalkToTheBot delivers empty string as answer, why?
- The type 'WcfService1.Service1', provided as the Service attribute value in the ServiceHost directive, or provided in the configuration element system.serviceModel/serviceHostingEnvironment/serviceActivations could not be found. 
*/

