using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FBKWebService
{
    public class Factory
    {
        bool isMock;
        string language;

        public Factory()
        {
            isMock = false;
            language = "de";
        }
        public Factory(bool v, string t)
        {
            isMock = v;
            language = t;
        }

        /// <summary>
        /// returns either an instance of the Mock-State class or of the Real-State class, depending on the value of the property isMock.
        /// </summary>
        /// <returns>class instance of the two classes' parent class.</returns>
        public ServiceClass GetImplementation()
        {
            if (isMock)
            {
                return new Implementation_Mock(language);
            }
            else
            {
                if (!language.StartsWith("de"))
                    return new Implementation_Translation(language);
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