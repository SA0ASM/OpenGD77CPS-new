using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using OpenGD77CPS.ViewModels;

namespace OpenGD77CPS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ICommand command = (ICommand)e.Parameter;

            ((CodePlugVM)this.DataContext).LoadCodePlugCommand.Execute(null);

            if (command != null)
                command.Execute(null);
        }
        void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ICommand command = (ICommand)e.Parameter;

            ((CodePlugVM)this.DataContext).SaveCodePlugCommand.Execute(null);

            if (command != null)
                command.Execute(null);
        }
        void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ICommand command = (ICommand)e.Parameter;

            ((CodePlugVM)this.DataContext).SaveCodePlugCommand.Execute("");

            if (command != null)
                command.Execute(null);
        }

        void CanAlwaysExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }


    }
}