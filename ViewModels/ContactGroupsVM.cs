using OpenGD77CPS.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenGD77CPS.ViewModels
{
    internal class ContactGroupsVM : ObservableObject
    {
    
        CodePlug _cp;

        ContactGroup? _selectedContactGroup;

        ICommand? _addContactGroupCommand;
        ICommand? _deleteContactGroupCommand;
        ICommand? _addContactsCommand;
        ICommand? _deleteContactsCommand;

        public ContactGroupsVM()
        {
            // this constructor only exists to allow Window.DataContext to reference the class
            _cp = new CodePlug();
        }

        public ContactGroupsVM(CodePlug cp)
        {
            _cp = cp;
        }

        public ContactGroup? SelectedContactGroup
        {
            get { return _selectedContactGroup;  }
            set {
                _selectedContactGroup = value;
                RaisePropertyChanged("SelectedContactGroup");
                RaisePropertyChanged("AvailableContacts");
            }
        }

        public IEnumerable<ContactGroup> ContactGroups
        {
            get { return _cp.ContactGroups; }
        }
        public IEnumerable<Contact> AvailableContacts
        {
            get {
                if (SelectedContactGroup == null)
                    return new List<Contact>();

                return _cp.Contacts.Except(SelectedContactGroup.Contacts); }
        }

        public ICommand AddContactGroupCommand
        {
            get
            {
                if (_addContactGroupCommand == null)
                {
                    _addContactGroupCommand = new RelayCommand(
                        AddContactGroup,
                        param => _cp.ContactGroups.Count() < 76
                    );
                }
                return _addContactGroupCommand;
            }
        }

        public ICommand DeleteContactGroupCommand
        {
            get
            {
                if (_deleteContactGroupCommand == null)
                {
                    _deleteContactGroupCommand = new RelayCommand(
                        DeleteContactGroup,
                        param => _cp.ContactGroups.Count > 0
                    );
                }
                return _deleteContactGroupCommand;
            }
        }
        public ICommand AddContactsCommand
        {
            get
            {
                if (_addContactsCommand == null)
                {
                    _addContactsCommand = new RelayCommand(
                        AddContacts,
                        param => AvailableContacts.Count() > 0 && SelectedContactGroup != null && SelectedContactGroup.Contacts.Count() < 32
                    );
                }
                return _addContactsCommand;
            }
        }

        public ICommand DeleteContactsCommand
        {
            get
            {
                if (_deleteContactsCommand == null)
                {
                    _deleteContactsCommand = new RelayCommand(
                        DeleteContacts,
                        param => SelectedContactGroup != null && SelectedContactGroup.Contacts.Count() > 0
                    );
                }
                return _deleteContactsCommand;
            }
        }


        private void AddContactGroup(object? o)
        {
            if (_cp != null)
            {
                _cp.ContactGroups.Add(new ContactGroup("Empty Group"));
                RaisePropertyChanged("ContactGroups");
            }
        }
        private void DeleteContactGroup(object? o)
        {
            if (_cp != null && o != null)
            {
                var c = o as ContactGroup;
                if (_cp.ContactGroups.Remove(c))
                {
                    if (SelectedContactGroup == c)
                        SelectedContactGroup = null;
                    RaisePropertyChanged("ContactGroups");
                }
            }
        }

        private void AddContacts(object? o)
        {
            var selectedItems = o as IList;
            if (selectedItems != null && SelectedContactGroup != null)
            {
                foreach (var item in selectedItems)
                {
                    if (item != null && SelectedContactGroup.Contacts.Count() < 32)
                        SelectedContactGroup.Contacts.Add(item as Contact);
                }
                RaisePropertyChanged("AvailableContacts");
            }
        }

        private void DeleteContacts(object? o)
        {
            var selectedItems = o as IList;
            if (selectedItems != null && SelectedContactGroup != null)
            {
                while (selectedItems.Count > 0)
                {
                    var c = selectedItems[0] as Contact;
                    selectedItems.RemoveAt(0);
                    if (c != null)
                        SelectedContactGroup.Contacts.Remove(c);
                }
                RaisePropertyChanged("AvailableContacts");
            }
        }


    }
}
