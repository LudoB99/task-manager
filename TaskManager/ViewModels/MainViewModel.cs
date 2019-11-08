using SimpleMvvmToolkit.Express;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        Process[] proc; 
        //public ICommand ShowToastCommand => new DelegateCommand(ShowToast);

        public MainViewModel()
        {
            GetAllProcesses(); 
        }

        public void GetAllProcesses() 
        {
            proc = Process.GetProcesses().OrderBy(p => p.ProcessName).ToArray();

            Console.WriteLine("System found {0} processes.", proc.Length);

            foreach (Process p in proc) 
            {
                Console.WriteLine("Process: {0} ID: {1}", p.ProcessName, p.Id);
            }
        }
    }
}
  

