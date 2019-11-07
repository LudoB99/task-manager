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
        Toast toast;
        public ICommand ShowToastCommand => new DelegateCommand(ShowToast);

        public MainViewModel()
        {
            toast = new Toast();
        }
        public void ShowToast() //Changer la fonction pour passer un paramètre message. 
        {
            //GET MESSAGE; 
            toast.showToast("ALLO");
        }
    }
}
  

