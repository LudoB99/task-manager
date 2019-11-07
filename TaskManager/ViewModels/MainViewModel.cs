using SimpleMvvmToolkit.Express;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
namespace TaskManager.ViewModels
{
    public class MainViewModel : ViewModelBase<MainViewModel>
    {
        public ICommand ShowToastCommand => new DelegateCommand(ShowToast);

        public MainViewModel()
        {
            
        }
        public void ShowToast()
        {
            TaskManager.App.Toast toast = new TaskManager.App.Toast(); 
        }
    }
}
