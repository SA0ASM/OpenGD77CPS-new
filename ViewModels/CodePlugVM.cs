using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using OpenGD77CPS.Models;
using System.Windows.Shapes;
using System.Xml;
using System.Diagnostics.Eventing.Reader;

namespace OpenGD77CPS.ViewModels
{
    class CodePlugVM : ObservableObject
    {
        #region Fields

        CodePlug _cp;
        byte[] _codeplugData = new byte[0];

        String _path = "";

        ICommand? _loadCodePlugCommand;
        ICommand? _saveCodePlugCommand;
        ICommand? _readCodePlugCommand;
        ICommand? _writeCodePlugCommand;

        ICommand? _listChannelsCommand;
        ICommand? _listZonesCommand;
        ICommand? _listContactsCommand;
        ICommand? _listContactGroupsCommand;

        ChannelListWindow? _channelListWindow;
        ZonesWindow? _zonesWindow;
        ContactsWindow? _contactsWindow;
        ContactGroupsWindow? _contactGroupsWindow;

        #endregion

        public CodePlugVM()
        {
            _cp = new CodePlug();
        }

        public CodePlugVM(CodePlug cp)
        {
            _cp = cp;
        }

        #region Public Properties/Commands

        public String LoadedFilename
        {
            get { return _path; }
            set
            {
                _path = value;
                RaisePropertyChanged("LoadedFilename");
            }
        }

        public GeneralSettings Settings
        {
            get { return _cp.Settings; }
        }

        public IEnumerable<Channel> Channels
        {
            get { return _cp.Channels; }
        }

        public IEnumerable<Zone> Zones
        {
            get { return _cp.Zones; }
        }

        public IEnumerable<Contact> Contacts
        {
            get { return _cp.Contacts; }
        }

        public IEnumerable<ContactGroup> ContactGroups
        {
            get { return _cp.ContactGroups; }
        }

        public ICommand LoadCodePlugCommand
        {
            get { 
                if (_loadCodePlugCommand == null)
                {
                    _loadCodePlugCommand = new RelayCommand(
                        param => LoadCodePlug(),
                        param => true
                    );
                }
                return _loadCodePlugCommand; }
        }

        public ICommand SaveCodePlugCommand
        {
            get
            {
                if (_saveCodePlugCommand == null)
                {
                    _saveCodePlugCommand = new RelayCommand(
                        SaveCodePlug,
                        param => _codeplugData.Length == 0x20000
                    );
                }
                return _saveCodePlugCommand;
            }
        }

        public ICommand ReadCodePlugCommand
        {
            get
            {
                if (_readCodePlugCommand == null)
                {
                    _readCodePlugCommand = new RelayCommand(
                        param => ReadCodePlug(),
                        param => true
                    );
                }
                return _readCodePlugCommand;
            }
        }

        public ICommand WriteCodePlugCommand
        {
            get
            {
                if (_writeCodePlugCommand == null)
                {
                    _writeCodePlugCommand = new RelayCommand(
                        param => WriteCodePlug(),
                        param => _codeplugData.Length == 0x20000
                    );
                }
                return _writeCodePlugCommand;
            }
        }

        public ICommand ListChannelsCommand
        {
            get
            {
                if (_listChannelsCommand == null)
                {
                    _listChannelsCommand = new RelayCommand(
                        param => ListChannels(),
                        param => _codeplugData.Length == 0x20000
                    );
                }
                return _listChannelsCommand;
            }
        }

        public ICommand ListZonesCommand
        {
            get
            {
                if (_listZonesCommand == null)
                {
                    _listZonesCommand = new RelayCommand(
                        param => ListZones(),
                        param => _codeplugData.Length == 0x20000
                    );
                }
                return _listZonesCommand;
            }
        }

        public ICommand ListContactsCommand
        {
            get
            {
                if (_listContactsCommand == null)
                {
                    _listContactsCommand = new RelayCommand(
                        param => ListContacts(),
                        param => _codeplugData.Length == 0x20000
                    );
                }
                return _listContactsCommand;
            }
        }

        public ICommand ListContactGroupsCommand
        {
            get
            {
                if (_listContactGroupsCommand == null)
                {
                    _listContactGroupsCommand = new RelayCommand(
                        param => ListContactGroups(),
                        param => _codeplugData.Length == 0x20000
                    );
                }
                return _listContactGroupsCommand;
            }
        }

        #endregion

        #region Private Helpers

        private void LoadCodePlug()
        {
            String file = "";

            // ask to save or reload the current codeplug
            if (LoadedFilename != "")
            {


            }

            if (file == "")
            {
                // let the user select a file
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CodePlug Files (*.g77)|*.g77";
                openFileDialog.Title = "Select a file to open";
                if (openFileDialog.ShowDialog() == true)
                {
                    file = openFileDialog.FileName;
                }
                else
                {
                    // user cancelled load operation!
                    return;
                }
            }

            byte[] data = new byte[0];
            if (File.Exists(file))
            {
                // read and parse all the data
                data = File.ReadAllBytes(file);

                // validate file data
                if (data.Length != 0x20000)
                {
                    MessageBox.Show("File does not contain valid codeplug data!", "OpenGD77 CPS", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // load the data into codeplug
                MemoryStream stream = new MemoryStream(data);
                if (!_cp.Load(stream))
                {
                    MessageBox.Show("Error loading the codeplug!", "OpenGD77 CPS", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            
            // save the codeplug data
            _codeplugData = data;
            LoadedFilename = file;

            // redraw the ui elements
            RaisePropertyChanged("Settings");
            RaisePropertyChanged("Zones");
            RaisePropertyChanged("Channels");
            RaisePropertyChanged("Contacts");
            RaisePropertyChanged("ContactGroups");

        }

        public void SaveCodePlug(object o)
        {
            if (o != null)
                LoadedFilename = o.ToString();

            // update codeplug data
            if (_codeplugData == null || _codeplugData.Length != 0x20000)
            {
                MessageBox.Show("No loaded codeplug data! Please open an existing codeplug file or read from radio.", "OpenGD77 CPS", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            } else {
                if (!_cp.Save(new MemoryStream(_codeplugData)))
                {
                    MessageBox.Show("Error preparing codeplug data!", "OpenGD77 CPS", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // save to already opened file_
            String path = LoadedFilename;
            if (File.Exists(path))
            {
                // ask to confirm file overwrite?
            }
            else
            {
                // let the user select a file
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CodePlug Files (*.g77)|*.g77";
                saveFileDialog.Title = "Select where to save the codeplug";
                if (saveFileDialog.ShowDialog() != true)
                    return; // user cancelled save operation!
                path = saveFileDialog.FileName;
            }

            // write the codeplug to file
            try
            {
                var fileStream = File.Open(path, FileMode.Create, FileAccess.Write);
                fileStream.Write(_codeplugData);
                fileStream.Close();
                MessageBox.Show("CodePlug saved successfully!", "OpenGD77 CPS");
                LoadedFilename = path;

            } catch (Exception e)
            {
                MessageBox.Show("Error saving CodePlug!\r\nException: " + e.Message, "OpenGD77 CPS", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReadCodePlug()
        {
            var res = false;

            // ask to save existing plug first?
            if (_codeplugData.Length > 0 && LoadedFilename != "")
            {

            }

            // read data from radio
            var serial = new SerialComms();
            SerialComms.RadioInfo radioInfo = serial.identifyRadio();
            var data = serial.readCodeplug();

            // load codeplug from data
            if (data.Length == 0x20000)
            {
                res = _cp.Load(new MemoryStream(data));
            }
            else{
                MessageBox.Show("Error reading codeplug from radio!", "OpenGD77 CPS", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (res)
            {
                // save codeplug data
                _codeplugData = data;

                // redraw the ui elements 
                RaisePropertyChanged("Settings");
                RaisePropertyChanged("Zones");
                RaisePropertyChanged("Channels");
                RaisePropertyChanged("Contacts");
                RaisePropertyChanged("ContactGroups");

                LoadedFilename = "";
                MessageBox.Show("Codeplug read successfully!", "OpenGD77 CPS");
            }
            else
                MessageBox.Show("Error in codeplug read from radio!", "OpenGD77 CPS", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void WriteCodePlug()
        {

            // update codeplug data
            if (_codeplugData == null || _codeplugData.Length != 0x20000)
            {
                MessageBox.Show("No loaded codeplug data! Please open an existing codeplug file or read from radio.", "OpenGD77 CPS", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                if (!_cp.Save(new MemoryStream(_codeplugData)))
                {
                    MessageBox.Show("Error preparing codeplug data!", "OpenGD77 CPS", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // write codeplug to radio
            var serial = new SerialComms();
            SerialComms.RadioInfo radioInfo = serial.identifyRadio();
            if (serial.writeCodeplug(_codeplugData))
                MessageBox.Show("Codeplug written successfully!", "OpenGD77 CPS");
            else
                MessageBox.Show("Error writing codeplug to radio!", "OpenGD77 CPS", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    public void ListChannels()
        {
            if (_channelListWindow == null || PresentationSource.FromVisual(_channelListWindow) == null)
            {
                _channelListWindow = new ChannelListWindow();
                _channelListWindow.DataContext = new ChannelListVM(_cp);
                _channelListWindow.Show();
            }

            if (_channelListWindow.WindowState == WindowState.Minimized)
                _channelListWindow.WindowState = WindowState.Normal;

            _channelListWindow.Focus();
        }
        
        public void ListContacts()
        {
            if (_contactsWindow == null || PresentationSource.FromVisual(_contactsWindow) == null)
            {
                _contactsWindow = new ContactsWindow();
                _contactsWindow.DataContext = new ContactsVM(_cp);
                _contactsWindow.Show();
            }

            if (_contactsWindow.WindowState == WindowState.Minimized)
                _contactsWindow.WindowState = WindowState.Normal;

            _contactsWindow.Focus();
        }

        public void ListContactGroups()
        {
            if (_contactGroupsWindow == null || PresentationSource.FromVisual(_contactGroupsWindow) == null)
            {
                _contactGroupsWindow = new ContactGroupsWindow();
                _contactGroupsWindow.DataContext = new ContactGroupsVM(_cp);
                _contactGroupsWindow.Show();
            }

            if (_contactGroupsWindow.WindowState == WindowState.Minimized)
                _contactGroupsWindow.WindowState = WindowState.Normal;

            _contactGroupsWindow.Focus();
        }

        public void ListZones()
        {
            if (_zonesWindow == null || PresentationSource.FromVisual(_zonesWindow) == null)
            {
                _zonesWindow = new ZonesWindow();
                _zonesWindow.DataContext = new ZonesVM(_cp);
                _zonesWindow.Show();
            }

            if (_zonesWindow.WindowState == WindowState.Minimized)
                _zonesWindow.WindowState = WindowState.Normal;

            _zonesWindow.Focus();
        }

        #endregion
    }
}
