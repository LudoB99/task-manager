using SimpleMvvmToolkit.Express;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TaskManager.ViewModels
{
    class SystemTrayViewModel : ViewModelBase<SystemTrayViewModel>
    {
        public ICommand ShowTMWindowCommand => new DelegateCommand(ShowTMWindow);
        public ICommand QuitApplicationCommand => new DelegateCommand(QuitApplication);

        bool opened = false;

        private void ShowTMWindow()
        {
            if (!opened)
            {
                opened = true;
                var window = new MainWindow();
                window.Show();
            }
        }

        private void QuitApplication()
        {
            opened = false;
            Application.Current.Shutdown();
        }
    }
}
