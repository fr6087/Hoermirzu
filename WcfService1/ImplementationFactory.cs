
namespace WcfService1
{
    /// <summary>
    /// This class represents the factory in the factory design pattern. in an abstract way.
    /// reference: http://www.c-sharpcorner.com/article/factory-method-design-pattern-in-c-sharp/
    /// </summary>
    public abstract class ImplementationFactory
    {
        /// <summary>
        /// declaring that there will be a method that's returning either Mock- or Real-State functions
        /// </summary>
        /// <returns></returns>
        public abstract ServiceClass GetImplementation();

    }

    /// <summary>
    /// implements GetImplementation() of the parent class. At instantiation, this class assumes either that the real HTML form has to be used (default) or
    /// that the Mock-State is active (when parameter is true at construction).
    /// </summary>
    public class Service1Factory : ImplementationFactory
    {
        bool isMock;
        string language;

        public Service1Factory()
        {
            isMock = false;
            language = "de";
        }
        public Service1Factory(bool v, string t)
        {
            isMock = v;
            language = t;
        }
        
        /// <summary>
        /// returns either an instance of the Mock-State class or of the Real-State class, depending on the value of the property isMock.
        /// </summary>
        /// <returns>class instance of the two classes' parent class.</returns>
        public override ServiceClass GetImplementation()
        {
            if (isMock)
            {
                return new Implementation_Mock(language);
            } 
            else
            {
                if (!language.StartsWith("de"))
                    return new Implementation_Translating(language);
                return new Implementation_Real(language);
            }
                
        }
        /// <summary>
        /// method that enables a client application to determine which mode is active. For debugging purposes.
        /// </summary>
        /// <returns>the bool value of isMock.</returns>
        public bool GetMode()
        {
            return isMock;
        }
    }
}