using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGD77CPS.Models
{
    public class ContactGroup : ObservableObject
    {
        static int max_contact = 1024;
        static int max_name_len =  16;

        ObservableUniqueCollection<Contact> _contacts = new ObservableUniqueCollection<Contact>();

        String _name;

        public ContactGroup(String name)
        {
            Name = name;
        }

        public String Name
        {
            get { return _name; }
            set
            {
                String n = value;

                // truncate names
                if (n.Length > max_name_len)
                    n = n.Substring(0, max_name_len);
                _name = n;
                RaisePropertyChanged("Name");
            }
        }

        public void AddContact(Contact Contact)
        {
            if (!_contacts.Contains(Contact))
            {
                if (Contacts.Count > max_contact)
                    throw new Exception($"Cannot add Contact to ContactGroup '{Name}', ContactGroup is full!");

                _contacts.Add(Contact);
                RaisePropertyChanged("Contacts");
            }
        }

        public void RemoveContact(Contact Contact)
        {

            _contacts.Remove(Contact);
            RaisePropertyChanged("Contacts");
        }

        public ObservableUniqueCollection<Contact> Contacts
        {
            get { return _contacts; }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
