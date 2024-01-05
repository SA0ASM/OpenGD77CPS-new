using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using OpenGD77CPS.Models;

namespace OpenGD77CPS.ViewModels
{

    internal class ContactsVM : ObservableObject
    {
        CodePlug _cp;

        ICommand? _addContactCommand;
        ICommand? _deleteContactCommand;

        public ContactsVM()
        {
            // this constructor only exists to allow Window.DataContext to reference the class
            _cp = new CodePlug();
        }

        public ContactsVM(CodePlug cp)
        {
            _cp = cp;
        }


        public IEnumerable<Contact> Contacts
        {
            get { return _cp.Contacts; }
        }

        public ICommand AddContactCommand
        {
            get
            {
                if (_addContactCommand == null)
                {
                    _addContactCommand = new RelayCommand(
                        AddContact,
                        param => _cp.Contacts.Count() < 1024
                    );
                }
                return _addContactCommand;
            }
        }

        public ICommand DeleteContactCommand
        {
            get
            {
                if (_deleteContactCommand == null)
                {
                    _deleteContactCommand = new RelayCommand(
                        DeleteContact,
                        param => _cp.Contacts.Count > 0
                    );
                }
                return _deleteContactCommand;
            }
        }

        private void AddContact(object? o)
        {
            if (_cp != null)
            {
                _cp.AddContact(new Contact("Empty Contact", 0));
                RaisePropertyChanged("Contacts");
            }
        }
        private void DeleteContact(object? o)
        {
            if (_cp != null && o != null)
            {
                var c = o as Contact;
                if (_cp.RemoveContact(c))
                {
                    RaisePropertyChanged("Contacts");
                }
            }
        }

    }
}
