
using System;

namespace ClassLibrary.model
{
    /// <summary>
    /// stores a section of the form
    /// </summary>
    public class Section
    {
        /*private string formNumber;
        private string name;
        private string text;
        private string number;*/
        private Element[] inputsAndHeadings;

        private string section;

        public Section(string section)
        {
            this.section = section;


        }

        public Section()
        {
        }

        public Section(Element[] inputs)
        {
            this.inputsAndHeadings = inputs;
        }
        /*
        public string FormNumber { get => formNumber; set => formNumber = value; }
        public string Number { get => number; set => number = value; }
        public string Text { get => text; set => text = value; }
        public string Name { get => name; set => name = value; }
        */
        public Element[] InputsAndHeadings { get => inputsAndHeadings; set => inputsAndHeadings = value; }

        public int Count()
        {
            return inputsAndHeadings.Length;
        }
    }
}
