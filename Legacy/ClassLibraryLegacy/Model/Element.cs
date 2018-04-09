using System.ComponentModel;

namespace ClassLibrary.model
{
    /// <summary>
    /// models the control element of a web form
    /// </summary>
    public class Element : INotifyPropertyChanged
    {
        private string type; //supposed to store header of the input field
        private string text; // supposed to store Content of the input field

        //the following attributes may not be set
        private string required;
        private string comment;
        private string placeholder;

        private Element[] subelems;

        //these contain only values for special elements e.g. dropdown list
        private string size;
        private string maxlength;
        private string group_name;//saves radiobutton's group association
        private string list_id;//saves dropdown list's name
        private bool? isSelected;//saves whether eg. radio button or checkbox are selected


        public Element()
        {

        }
        //public attributes
        public event PropertyChangedEventHandler PropertyChanged;
        public Element(string text, string _type, string _required, string _comment, string _placeholder, Element[] _subelems, string _size, string _maxlength, string _groupname, string _list_id)
        {
            Text = text;
            Type = _type;
            Required = _required;
            Comment = _comment;
            Placeholder = _placeholder;
            Subelems = _subelems;
            Size = _size;
            Maxlength = _maxlength;
            Group_name = _groupname;
            List_id = _list_id;

        }
        public override string ToString()
        {
            string s = "Element {";
            s += "Text:" + Text + " , ";
            s += "Type" + Type + " , " + " , ";
            s += "Required" + Required + " , ";
            s += "Comment" + Comment + " , ";
            s += "Placeholder" + Placeholder + " , ";
            if (Subelems != null)
            {
                foreach (Element el in Subelems)
                {
                    s += "Subelem:" + el.ToString() + " , ";
                }
            }
            s += "Size:" + Size + " , ";
            s += "Maxlength:" + Maxlength + " , ";
            s += "Group_name:" + Group_name + " , ";
            s += "ListID:" + List_id + " } ";
            return s;
        }
        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                NotifyPropertyChanged("Type");
            }
        }
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                NotifyPropertyChanged("Text");
            }
        }

        public string Placeholder { get { return placeholder; } set { placeholder = value; NotifyPropertyChanged("List_default"); } }
        public string List_id { get { return list_id; } set { list_id = value; NotifyPropertyChanged("List_id"); } }
        public string Group_name { get { return group_name; } set { group_name = value; NotifyPropertyChanged("Group_name"); } }
        public string Maxlength { get { return maxlength; } set { maxlength = value; NotifyPropertyChanged("Maxlength"); } }
        public string Size { get { return size; } set { size = value; NotifyPropertyChanged("Size"); } }
        public Element[] Subelems { get { return subelems; } set { subelems = value; NotifyPropertyChanged("Subelems"); } }
        public string Comment { get { return comment; } set { comment = value; NotifyPropertyChanged("Comment"); } }
        public string Required { get { return required; } set { required = value; NotifyPropertyChanged("Required"); } }
        public bool? IsSeclected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }

        public string Disabled { get; set; }
        public string Pattern { get; set; }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                handler(this, args);
            }
        }
    }
}
