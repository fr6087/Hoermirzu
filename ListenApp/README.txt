/**
\mainpage ListenApp Documentation

\section zero Prerequisites

To run ListenApp, you need 
- Visual Studio C# developing environment
- Windows 10 dektop with Cortana. Some functionalities might be available in Windows 8.1+ as well as in Windows Phone Versions, I'm
not guaranteeing that however, since it's not tested.
- to be registered as a user in the webform (the userename and password is needed for most methods, except when running in Mock-Mode)
- there are a number of other registrations that the App needs and that I have already provided in the background. If I will ever shut down those online
services, you will have to create those on your own as follows:
	-# Microsoft Developer Account
	-# LUIS Account
	-# Azure Account
	-# Bot Developer Account
	-# Cortana Developer Account
-microphone permission on your device is preferable. Otherwise you will be only able to run the App in text-mode.

\section first Functionality

ListenApp is the main component of ListenToMe project. It contains a visual representation of the online form ESF and an text or speech input line.
Input is evaluated by sending the phrases to the online Service LUIS or to the online Service Bot with SendMessage(), and then evaluating the response in
dertermineResponse(). The available, enabled input fields at the App as well as radio buttons and dropdown lists are referenced in a Hashtable with their labels
as keys. If the App recognizes a match with one of the labels in the App, it will fill in the recognised field value in the phrase into that very field.
If it is not sure - sometimes there are several inputs with the same label - it will ask the user for disambiguation.
As it is, ListenApp can't upload the data back to the online form. There is a in GO written component formularinstanzservice that has to be addressed for that
and I haven't yet figured out how to do that.

\section second Pecularities

The WCF Service is implemented in Factory Pattern. The facory is able to produce either in Mock-Mode or in Real-Mode output. Mock-Mode is independant from the
availability of the form to be parsed. It reads from a text file called Sections.json which is included in the project. Real-Mode however, does call the domain
of the webform to archieve the same purpose.

If the app is throwing a COMException, the problem is that it can't find the WCF Service on which it depends. The workaround for this is to delete the service 
reference in the app, rebuild the WCF Service project and create a new service reference to the Service1.svc of WCF Service project in this app.


\section third ToDos
- Die Buttons zum Hinzufügen von Tabellenzeilen bei den favorisierten Bildungsanbietern sind nicht ohne weiteres darstellbar. Aufgrund der Tabellenstruktur im
HTML-Code, die sehr schwer mit XPATH auslesbar sein wird, sind die Feldbeschriftungen zugeteilt.
- Testen, ob cutLeadingNumbers in WCFService tatsächlich die führenden Nummern aus den Feldern Gründungsdatum ect. schneidet. Wichtig für Bot.
- Der SeleniumBrowser von WCF Service wird nicht beendet
- Das Formular in Polnisch und Englisch abrufen -> neue Dummyelemente generieren
- Der WCF Service ist extrem spezifisch auf das vorliegende Formular hörig. Da er als Grundlage die Html-Seite parst, hat er keinen zugriff auf die Dahinterliegende Logik,
zum Beispiel die Angular-Direktiven, die dafür sorgen, dass in einem bestimmten Feld eine Summe eingetragen wird, sobald ein anderes ausgefüllt ist. Die
Darstellung der dynamischen Inhalte kann er nicht abbilden, z.B. Berechnungen, wieviele Zeichen noch übrig sind, Fehlermeldungen und Button-Aktionen.
- Ressource Binding läuft für die Checkboxes und RadioButtons nicht glatt
- upLoadToJason() wirft 

\code {.unparsed}
StatusCode: 400, ReasonPhrase: 'BadArgument', Version: 1.1, Content: System.Net.Http.StreamContent, Headers:
{
  Request-Context: appId=cid-v1:26a3540d-a02a-4998-a060-715488fd769b
  Strict-Transport-Security: max-age=31536000; includeSubDomains; preload
  Request-Id: c60c9469-68e3-40a5-bb8e-beaa61870875
  Cache-Control: no-store, proxy-revalidate, no-cache, max-age=0, private
  Date: Thu, 14 Dec 2017 07:51:09 GMT
  X-Frame-Options: SAMEORIGIN
  X-Powered-By: ASP.NET
  X-Content-Type-Options: nosniff
  Pragma: no-cache
  Apim-Request-Id: c60c9469-68e3-40a5-bb8e-beaa61870875
  Content-Length: 152
  Content-Type: application/json; charset=utf-8
}
\endcode

- das Codesnippet in den Bot einbauen, das die Cortana information zur email und username abruft, möglich?
- ändere meine <Seite> läuft nicht für Seiten, die aus mehr als einem Wort bestehen zum Beispiel "Ändere ListenToMe Registereintragungen" läuft, nicht aber "ändere listentome rggistereintraggunggen" und "Ändere ListenToMe Sitz des Antragsstellers"

Kann ich nur zuhause/mit BotframeworkEmulator debuggen:
- die längeren Eingaben funktionieren nicht mehr wenn Cortana auf USA eingestellt ist "Ich möchte die Unternehmensangaben im ESF_2 Formular ausfüllen, z.B."
- dateiupload und helpdialog stürtzen ab
*/

Gelöst:
-Der VoiceCommand "Information" wird im VoiceCommandService abgehandelt. Aus bisher ungeklärter Ursache  erreicht er die Methode "deployVoiceCommand"
nicht. -> ein falscher Key
-Cortana channel nicht integriert - hängt evt. mit den mikrofonberechtigung zusammen? Oder mit den fehlenden Skills im Cortana Notizbuch? Oder dass ich
"ask formbot to help" falsch übersetze? Unternehmensrichtlinie? -> unterschiedliche Gründe kommen hier zusammen. Zum einen scheint Cortana sich hin und wieder nicht mit ihren CloudServices
verbinden zu können (einige Benutzer haben da ähnliches Verhalten beobachtet), woraufhin sie dann den Aufruf-Namen "ListenToMe" natürlich nicht auswerten kann und stattdessen Bing startet. Zum Anderen ist Cortana als
Channel nur für den US-Markt geöffnet.
-Der VoiceCommandService stürzt ab mit
Ausnahme ausgelöst: "System.Collections.Generic.KeyNotFoundException" in System.Private.CoreLib.ni.dll
Handling Voice Command failed System.Collections.Generic.KeyNotFoundException: The given key was not present in the dictionary. -> todo breakpoint debugging?
Passiert in
            loadingPageToEdit = string.Format(
                       cortanaResourceMap.GetValue("LoadingFieldToEdit", cortanaContext).ValueAsString,
                       fieldName);//vormals LoadingTripToDestination
					   -> das lag daran, dass der backgroundtask in den Ressourcen-Strings der App sucht und dort LoadingPageToEdit nicht definiert war.
-MainPage häuft die sections alle auf einem panel an.-> ein Problem, das im WCF-Service seinen Ursprung hatte mit einer nicht zurückgesetzten Variablen
-Navigationservice zuwischen den panels nicht möglich-> wird über PagesCount Variable und Zugriff auf das List<Sections> Model in Form gelöst
-LuisBot wirft bei einer Erzeugung mit LocalizedCulture Fehler /MakeRoot in MessagesController oder buildLocalizedForm stürzt verm. ab
	-> lag daran, dass ein Komma im Confirm() Sting fehlte
-Die Website hat teils gar keine innerHtml bezeichnungen für Felder (Beispiel IBAN) und antwortet immer mit leerem Formular, auch wenn der Benutzername falsch war
	-> lag daran, dass die Website nicht fertig geparst war, lauscht jetzt auf eines der finished-Attibute
-XAML-Unstimmigkeiten: Buttons zum Auswählen der Angebote der Bildungsanbieter fehlen, verschrobenes Layout im Petitionsdetails, Feld ganz unten 1x doppelt überladen
-LuisBot erreicht den unterDialog  helpDialog nicht. Wirft MultipleResumeHandlersException -> context.Wait darf nicht in helpIntent vorkommen
-uploadHeadingsToLuisModel() in MainPage läuft nicht, jsonDatei wird nicht erstellt -> das lag daran dass UWP-Apps keinen Zugriff auf das normale Dateiverwaaltungssystem haben
-Ich möchte die PhraseList dynamisch mit den headings aus dem Formular füllen. Todo Methode für WCF Service schreiben, die ausschließlich headings harvestes
-VoiceCommand Edit verarbeitet keine Parameter
-Wenn ListenToMe über Cortana gestartet wird dann kann es nicht wieder über Cortana ausgeschaltet werden. -> lag an nullPointer-Rootframe