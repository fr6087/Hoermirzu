

namespace ListenToMe.Model
{
    /// <summary>
    /// class for storing arguments. used by App to bind launch arguments (e.g. from Cortana)
    /// </summary>
    class ListenToMeVoiceCommand
    {

        
            /// <summary>
            /// name of the voice command
            /// </summary>
            public string voiceCommand;
            /// <summary>
            /// mode in which the command was delivered. Possible values: speech and text
            /// </summary>
            public string commandMode;
            /// <summary>
            /// contains the spokenText, if command was used in speech mode
            /// </summary>
            public string textSpoken;
            /// <summary>
            /// contains arguments the command may have. e.g. ListenToMe edit my ConfirmationsPage may result in confirmationspage as destination
            /// </summary>
            public string destination;

            /// <summary>
            /// Set up the voice command struct with the provided details about the voice command.
            /// Oriented around the "showTripToDestination" VCD command (See AdventureWorksCommands.xml)
            /// </summary>
            /// <param name="voiceCommand">The voice command (the Command element in the VCD xml) </param>
            /// <param name="commandMode">The command mode (whether it was voice or text activation)</param>
            /// <param name="textSpoken">The raw voice command text.</param>
            /// <param name="destination">The destination parameter.</param>
            public ListenToMeVoiceCommand(string voiceCommand, string commandMode, string textSpoken, string destination)
            {
                this.voiceCommand = voiceCommand;
                this.commandMode = commandMode;
                this.textSpoken = textSpoken;
                this.destination = destination;
            }
        
    }
}
