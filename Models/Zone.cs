using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGD77CPS.Models
{
    internal class Zone : ObservableObject
    {
        static int max_channel = 79;
        static int max_name_len = 16;

        ObservableCollection<Channel> channels = new ObservableCollection<Channel>();
        String _name;

        public Zone(String name)
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

        public void AddChannel(Channel channel)
        {
            if (!channels.Contains(channel))
            {
                if (channels.Count > max_channel)
                    throw new Exception($"Cannot add channel to zone '{Name}', zone is full!");

                channels.Add(channel);
                RaisePropertyChanged("Channels");
            }
        }

        public void RemoveChannel(Channel channel) {

            channels.Remove(channel);
            RaisePropertyChanged("Channels");
        }

        public ObservableCollection<Channel> Channels
        {
            get { return channels;  }
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
