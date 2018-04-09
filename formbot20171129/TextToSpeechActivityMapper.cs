using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot
{
    /// <summary>
    /// Activity mapper that automatically populates activity.speak for speech enabled channels.
    /// reference: https://stackoverflow.com/questions/44464575/formflow-prompts-are-not-being-spoken-in-cortana-skills
    /// </summary>
    public class TextToSpeechActivityMapper:IMessageActivityMapper
    {
        public IMessageActivity Map(IMessageActivity message)
        {
            // only set the speak if it is not set by the developer.
            var channelCapability = new ChannelCapability(Address.FromActivity(message));

            if (channelCapability.SupportsSpeak() && string.IsNullOrEmpty(message.Speak))
            {
                message.Speak = message.Text;

                // set InputHint to ExpectingInput if text is a question
                var isQuestion = message.Text?.EndsWith("?");
                if (isQuestion.GetValueOrDefault())
                {
                    message.InputHint = InputHints.ExpectingInput;
                }
            }

            return message;
        }
    }
}