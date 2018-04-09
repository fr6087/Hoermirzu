﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This code was auto-generated by Microsoft.VisualStudio.ServiceReference.Platforms, version 15.0.27130.2024
// 
namespace ListenToMe.ServiceReference1 {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference1.IService1")]
    public interface IService1 {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/Login", ReplyAction="http://tempuri.org/IService1/LoginResponse")]
        System.Threading.Tasks.Task<string> LoginAsync(string username, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/GetForm", ReplyAction="http://tempuri.org/IService1/GetFormResponse")]
        System.Threading.Tasks.Task<string> GetFormAsync(string _username, string _password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/GetInputs", ReplyAction="http://tempuri.org/IService1/GetInputsResponse")]
        System.Threading.Tasks.Task<System.Collections.ObjectModel.ObservableCollection<string>> GetInputsAsync(string username, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/GetSpecificElements", ReplyAction="http://tempuri.org/IService1/GetSpecificElementsResponse")]
        System.Threading.Tasks.Task<System.Collections.ObjectModel.ObservableCollection<string>> GetSpecificElementsAsync(string username, string password, string xpath);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/GetAllElements", ReplyAction="http://tempuri.org/IService1/GetAllElementsResponse")]
        System.Threading.Tasks.Task<System.Collections.ObjectModel.ObservableCollection<ClassLibrary.model.Section>> GetAllElementsAsync(string username, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/ReadJasonFromFile", ReplyAction="http://tempuri.org/IService1/ReadJasonFromFileResponse")]
        System.Threading.Tasks.Task<string> ReadJasonFromFileAsync(string filename);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/GetCSVDropdownString", ReplyAction="http://tempuri.org/IService1/GetCSVDropdownStringResponse")]
        System.Threading.Tasks.Task<System.Collections.ObjectModel.ObservableCollection<string>> GetCSVDropdownStringAsync(string list_id, string filter);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/SerializeToJson", ReplyAction="http://tempuri.org/IService1/SerializeToJsonResponse")]
        System.Threading.Tasks.Task<string> SerializeToJsonAsync(string username, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/UploadSection", ReplyAction="http://tempuri.org/IService1/UploadSectionResponse")]
        System.Threading.Tasks.Task<bool> UploadSectionAsync(ClassLibrary.model.Section section);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/TalkToTheBot", ReplyAction="http://tempuri.org/IService1/TalkToTheBotResponse")]
        System.Threading.Tasks.Task<string> TalkToTheBotAsync(string message);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/JsonForBot", ReplyAction="http://tempuri.org/IService1/JsonForBotResponse")]
        System.Threading.Tasks.Task<string> JsonForBotAsync(string username, string password, short sectionindex);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IService1Channel : ListenToMe.ServiceReference1.IService1, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class Service1Client : System.ServiceModel.ClientBase<ListenToMe.ServiceReference1.IService1>, ListenToMe.ServiceReference1.IService1 {
        
        /// <summary>
        /// Implement this partial method to configure the service endpoint.
        /// </summary>
        /// <param name="serviceEndpoint">The endpoint to configure</param>
        /// <param name="clientCredentials">The client credentials</param>
        static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials);
        
        public Service1Client() : 
                base(Service1Client.GetDefaultBinding(), Service1Client.GetDefaultEndpointAddress()) {
            this.Endpoint.Name = EndpointConfiguration.BasicHttpBinding_IService1.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public Service1Client(EndpointConfiguration endpointConfiguration) : 
                base(Service1Client.GetBindingForEndpoint(endpointConfiguration), Service1Client.GetEndpointAddress(endpointConfiguration)) {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public Service1Client(EndpointConfiguration endpointConfiguration, string remoteAddress) : 
                base(Service1Client.GetBindingForEndpoint(endpointConfiguration), new System.ServiceModel.EndpointAddress(remoteAddress)) {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public Service1Client(EndpointConfiguration endpointConfiguration, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(Service1Client.GetBindingForEndpoint(endpointConfiguration), remoteAddress) {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public Service1Client(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Threading.Tasks.Task<string> LoginAsync(string username, string password) {
            return base.Channel.LoginAsync(username, password);
        }
        
        public System.Threading.Tasks.Task<string> GetFormAsync(string _username, string _password) {
            return base.Channel.GetFormAsync(_username, _password);
        }
        
        public System.Threading.Tasks.Task<System.Collections.ObjectModel.ObservableCollection<string>> GetInputsAsync(string username, string password) {
            return base.Channel.GetInputsAsync(username, password);
        }
        
        public System.Threading.Tasks.Task<System.Collections.ObjectModel.ObservableCollection<string>> GetSpecificElementsAsync(string username, string password, string xpath) {
            return base.Channel.GetSpecificElementsAsync(username, password, xpath);
        }
        
        public System.Threading.Tasks.Task<System.Collections.ObjectModel.ObservableCollection<ClassLibrary.model.Section>> GetAllElementsAsync(string username, string password) {
            return base.Channel.GetAllElementsAsync(username, password);
        }
        
        public System.Threading.Tasks.Task<string> ReadJasonFromFileAsync(string filename) {
            return base.Channel.ReadJasonFromFileAsync(filename);
        }
        
        public System.Threading.Tasks.Task<System.Collections.ObjectModel.ObservableCollection<string>> GetCSVDropdownStringAsync(string list_id, string filter) {
            return base.Channel.GetCSVDropdownStringAsync(list_id, filter);
        }
        
        public System.Threading.Tasks.Task<string> SerializeToJsonAsync(string username, string password) {
            return base.Channel.SerializeToJsonAsync(username, password);
        }
        
        public System.Threading.Tasks.Task<bool> UploadSectionAsync(ClassLibrary.model.Section section) {
            return base.Channel.UploadSectionAsync(section);
        }
        
        public System.Threading.Tasks.Task<string> TalkToTheBotAsync(string message) {
            return base.Channel.TalkToTheBotAsync(message);
        }
        
        public System.Threading.Tasks.Task<string> JsonForBotAsync(string username, string password, short sectionindex) {
            return base.Channel.JsonForBotAsync(username, password, sectionindex);
        }
        
        public virtual System.Threading.Tasks.Task OpenAsync() {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndOpen));
        }
        
        public virtual System.Threading.Tasks.Task CloseAsync() {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginClose(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndClose));
        }
        
        private static System.ServiceModel.Channels.Binding GetBindingForEndpoint(EndpointConfiguration endpointConfiguration) {
            if ((endpointConfiguration == EndpointConfiguration.BasicHttpBinding_IService1)) {
                System.ServiceModel.BasicHttpBinding result = new System.ServiceModel.BasicHttpBinding();
                result.MaxBufferSize = int.MaxValue;
                result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                result.MaxReceivedMessageSize = int.MaxValue;
                result.AllowCookies = true;
                return result;
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
        
        private static System.ServiceModel.EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration) {
            if ((endpointConfiguration == EndpointConfiguration.BasicHttpBinding_IService1)) {
                return new System.ServiceModel.EndpointAddress("http://localhost:65384/Service1.svc/Service1.svc");
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
        
        private static System.ServiceModel.Channels.Binding GetDefaultBinding() {
            return Service1Client.GetBindingForEndpoint(EndpointConfiguration.BasicHttpBinding_IService1);
        }
        
        private static System.ServiceModel.EndpointAddress GetDefaultEndpointAddress() {
            return Service1Client.GetEndpointAddress(EndpointConfiguration.BasicHttpBinding_IService1);
        }
        
        public enum EndpointConfiguration {
            
            BasicHttpBinding_IService1,
        }
    }
}
