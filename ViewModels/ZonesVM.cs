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
    internal class ZonesVM : ObservableObject
    {
    
        CodePlug _cp;

        Zone? _selectedZone;

        ICommand? _addZoneCommand;
        ICommand? _deleteZoneCommand;
        ICommand? _addChannelsCommand;
        ICommand? _deleteChannelsCommand;

        public ZonesVM()
        {
            // this constructor only exists to allow Window.DataContext to reference the class
            _cp = new CodePlug();
        }

        public ZonesVM(CodePlug cp)
        {
            _cp = cp;
        }

        public Zone? SelectedZone
        {
            get { return _selectedZone;  }
            set {
                _selectedZone = value;
                RaisePropertyChanged("SelectedZone");
                RaisePropertyChanged("AvailableChannels");
            }
        }

        public IEnumerable<Zone> Zones
        {
            get { return _cp.Zones; }
        }
        public IEnumerable<Channel> AvailableChannels
        {
            get {
                if (SelectedZone == null)
                    return new List<Channel>();

                return _cp.Channels.Except(SelectedZone.Channels); }
        }

        public ICommand AddZoneCommand
        {
            get
            {
                if (_addZoneCommand == null)
                {
                    _addZoneCommand = new RelayCommand(
                        AddZone,
                        param => _cp.Zones.Count() < 68
                    );
                }
                return _addZoneCommand;
            }
        }

        public ICommand DeleteZoneCommand
        {
            get
            {
                if (_deleteZoneCommand == null)
                {
                    _deleteZoneCommand = new RelayCommand(
                        DeleteZone,
                        param => _cp.Zones.Count > 0
                    );
                }
                return _deleteZoneCommand;
            }
        }
        public ICommand AddChannelsCommand
        {
            get
            {
                if (_addChannelsCommand == null)
                {
                    _addChannelsCommand = new RelayCommand(
                        AddChannels,
                        param => AvailableChannels.Count() > 0 && SelectedZone != null && SelectedZone.Channels.Count() < 80
                    );
                }
                return _addChannelsCommand;
            }
        }

        public ICommand DeleteChannelsCommand
        {
            get
            {
                if (_deleteChannelsCommand == null)
                {
                    _deleteChannelsCommand = new RelayCommand(
                        DeleteChannels,
                        param => SelectedZone != null && SelectedZone.Channels.Count() > 0
                    );
                }
                return _deleteChannelsCommand;
            }
        }


        private void AddZone(object? o)
        {
            if (_cp != null)
            {
                _cp.Zones.Add(new Zone("Empty Zone"));
                RaisePropertyChanged("Zones");
            }
        }

        private void DeleteZone(object? o)
        {
            if (_cp != null && o != null)
            {
                var c = o as Zone;
                if (_cp.Zones.Remove(c))
                {
                    if (SelectedZone == c)
                        SelectedZone = null;
                    RaisePropertyChanged("Zones");
                }
            }
        }

        private void AddChannels(object? o)
        {
            var selectedItems = o as IList;
            if (selectedItems != null && SelectedZone != null)
            {
                foreach (var item in selectedItems)
                {
                    if (item != null && SelectedZone.Channels.Count() < 80)
                        SelectedZone.Channels.Add(item as Channel);
                }
                RaisePropertyChanged("AvailableChannels");
            }
        }

        private void DeleteChannels(object? o)
        {
            var selectedItems = o as IList;
            if (selectedItems != null && SelectedZone != null)
            {
                while (selectedItems.Count > 0)
                {
                    var c = selectedItems[0] as Channel;
                    selectedItems.RemoveAt(0);
                    if (c != null)
                        SelectedZone.Channels.Remove(c);
                }
                RaisePropertyChanged("AvailableChannels");
            }
        }


    }
}
