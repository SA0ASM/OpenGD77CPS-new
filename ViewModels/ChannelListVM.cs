using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using OpenGD77CPS.Models;

namespace OpenGD77CPS.ViewModels
{
    internal class ChannelListVM : ObservableObject
    {
        #region Fields

        CodePlug _cp;

        Window? _editChannelWindow;
        
        ICommand? _editChannelCommand;
        ICommand? _addChannelCommand;
        ICommand? _deleteChannelCommand;

        #endregion

        public ChannelListVM()
        {
            //throw new Exception("this constructor only exists to allow Window.DataContext to reference the class");
            _cp = new CodePlug();
        }

        public ChannelListVM(CodePlug cp)
        {
            _cp = cp;
        }

        #region Public Properties/Commands

        public ObservableCollection<Channel> Channels
        {
            get { return _cp.Channels; }
        }

        public ICommand EditChannelCommand
        {
            get {
                if (_editChannelCommand == null)
                {
                    _editChannelCommand = new RelayCommand(
                        EditChannel,
                        param => _cp.Channels.Count > 0
                    );
                }
                return _editChannelCommand; }
        }

        public ICommand AddChannelCommand
        {
            get { 
                if (_addChannelCommand == null)
                {
                    _addChannelCommand = new RelayCommand(
                        AddChannel,
                        param => _cp.Channels.Count() < 1024
                    );
                }
                return _addChannelCommand; }
        }

        public ICommand DeleteChannelCommand
        {
            get {
                if (_deleteChannelCommand == null)
                {
                    _deleteChannelCommand = new RelayCommand(
                        DeleteChannel,
                        param => _cp.Channels.Count > 0
                    );
                }
                return _deleteChannelCommand; }
        }

        #endregion

        #region Private Helpers
        private void EditChannel(object? o)
        {
            if (o != null)
            {
                if (_editChannelWindow == null || PresentationSource.FromVisual(_editChannelWindow) == null)
                {
                    _editChannelWindow = new ChannelEditWindow();
                    _editChannelWindow.Show();
                }

                _editChannelWindow.DataContext = new ChannelEditVM(_cp, o as Channel);

                if (_editChannelWindow.WindowState == WindowState.Minimized)
                    _editChannelWindow.WindowState = WindowState.Normal;

                _editChannelWindow.Focus();
            }
        }

        private void AddChannel(object? o)
        {
            if (_cp != null)
            {
                _cp.AddChannel(new Channel("Empty Channel", ChannelType.Analogue));
                RaisePropertyChanged("Channels");
            }
        }

        private void DeleteChannel(object? o)
        {
            if (_cp != null && o != null)
            {
                if (_cp.RemoveChannel(o as Channel))
                    RaisePropertyChanged("Channels");
            }
        }
        #endregion
    }
}
