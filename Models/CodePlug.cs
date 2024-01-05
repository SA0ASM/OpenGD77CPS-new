using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;


namespace OpenGD77CPS.Models
{
    internal class CodePlug : ObservableObject
    {
        static int num_zones = 68;
        static int num_channels = 1024;
        static int num_contacts = 1024;

        public GeneralSettings Settings = new GeneralSettings();

        ObservableCollection<Zone> _zones = new ObservableCollection<Zone>();
        ObservableUniqueCollection<Channel> _channels = new ObservableUniqueCollection<Channel>();
        ObservableUniqueCollection<Contact> _contacts = new ObservableUniqueCollection<Contact>();
        ObservableCollection<ContactGroup> _contactGroups = new ObservableCollection<ContactGroup>();

        public CodePlug()
        {
        }

        public void AddZone(Zone zone)
        {
            if (_zones.Count >= num_zones)
                throw new Exception("Cannot add zone, codeplug full!");

            // add any channels in the zone to our channel list
            foreach (var channel in zone.Channels)
                AddChannel(channel);

            _zones.Add(zone);
            RaisePropertyChanged("Zones");
        }

        public bool RemoveZone(Zone zone)
        {
            // removing a zone does not remove the channels in the zone
            var res = _zones.Remove(zone);
            RaisePropertyChanged("Zones");
            return res;
        }

        public void AddChannel(Channel channel)
        {
            if (_channels.Count >= num_channels)
                throw new Exception("Cannot add channel, codeplug full!");

            // check that the new channel number is valid and unique
            var chanLookup = _channels.ToLookup(c => c.Number, c => c);
            if (channel.Number > 0 && channel.Number <= 1024 && !chanLookup.Contains(channel.Number))
            {
                _channels.Add(channel);
            }
            else
            {

                // re-number channel befor adding
                for (int n = 1; n < num_channels; n++)
                {
                    if (!chanLookup.Contains(n))
                    {
                        channel.SetNumber(n);
                        _channels.Add(channel);
                        break;
                    }
                }
            }
            RaisePropertyChanged("Channels");
        }

        public bool RemoveChannel(Channel channel)
        {
            if (!_channels.Contains(channel))
                return false;

            // first, remove channel from all zones
            foreach (Zone z in _zones)
            {
                z.RemoveChannel(channel);
                RaisePropertyChanged("Zones");
            }

            var res = _channels.Remove(channel);
            if (res)
            {
                RaisePropertyChanged("Channels");
            }
            return res;
        }

        public void AddContact(Contact contact)
        {
            if (_contacts.Count >= num_contacts)
                throw new Exception("Cannot add contact, codeplug full!");

            _contacts.Add(contact);
            RaisePropertyChanged("Contacts");
        }

        public bool RemoveContact(Contact contact)
        {
            if (!_contacts.Contains(contact))
                return false;

            // first, remove contact from all groups
            foreach (ContactGroup g in _contactGroups)
            {
                g.RemoveContact(contact);
                RaisePropertyChanged("ContactGroups");
            }

            var res = _contacts.Remove(contact);
            if (res)
            {
                RaisePropertyChanged("Contacts");
            }
            return res;
        }

        public ObservableUniqueCollection<Channel> Channels
        {
            get { return _channels; }
        }

        public ObservableCollection<Zone> Zones
        {
            get { return _zones; }
        }

        public ObservableUniqueCollection<Contact> Contacts
        {
            get { return _contacts; }
        }

        public ObservableCollection<ContactGroup> ContactGroups
        {
            get { return _contactGroups; }
        }

        #region Loading and Saving

        public bool Load(Stream stream)
        {
            // parse the codeplug data
            var res = CPFormatOGD77.Load(stream);
            if (res.Item1)
            {
                // update ourselves
                _contactGroups.Clear();
                _contacts.Clear();
                _channels.Clear();
                _zones.Clear();

                Settings = res.Item2;

                foreach (Channel c in res.Item3)
                    _channels.Add(c);

                foreach (Zone z in res.Item4)
                    _zones.Add(z);

                foreach (Contact c in res.Item5)
                    _contacts.Add(c);

                foreach (ContactGroup c in res.Item6)
                    _contactGroups.Add(c);

                RaisePropertyChanged("ContactGroups");
                RaisePropertyChanged("Contacts");
                RaisePropertyChanged("Channels");
                RaisePropertyChanged("Zones");
            }

            return res.Item1;        
        }

    public bool Save(Stream stream)
        {
            return CPFormatOGD77.Save(stream, Settings, _channels.ToList(), _zones.ToList(), _contacts.ToList(), _contactGroups.ToList());
        }

        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("\nZones: ");
            foreach (var z in _zones)
                sb.AppendLine("\t" + z);
            sb.AppendLine("\nChannels: ");
            foreach (var c in _channels)
                sb.AppendLine("\t" + c);
            return sb.ToString();
        }
    }
}
