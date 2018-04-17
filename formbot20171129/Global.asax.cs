
using Autofac;
using LuisBot;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http;

namespace SimpleEchoBot 
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            var builder = new ContainerBuilder();
           
            builder
              .RegisterType<TextToSpeechActivityMapper>()
              .AsImplementedInterfaces()
              .SingleInstance();

            builder.Update(Conversation.Container);
        }
    }
}
