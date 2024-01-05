using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using OpenGD77CPS.Models;

namespace OpenGD77CPS.ViewModels
{
    internal class ChannelEditVM : ObservableObject
    {
        CodePlug _cp;

        Channel? _currentChannel;


        public ChannelEditVM()
        {
            //throw new Exception("this constructor only exists to allow Window.DataContext to reference the class");
            _cp = new CodePlug();
        }

        public ChannelEditVM(CodePlug cp, Channel channel)
        {
            _cp = cp;
            _currentChannel= channel;
        }

        public IEnumerable<Contact> Contacts
        { get { return _cp.Contacts; } }

        public IEnumerable<ContactGroup> ContactGroups
        { get { return _cp.ContactGroups; } }

        public Channel? Channel
        {
            get { return _currentChannel; }
            set
            {
                if (value != _currentChannel)
                {
                    _currentChannel = value;
                    RaisePropertyChanged("Channel");
                }
            }
        }

    }
}
