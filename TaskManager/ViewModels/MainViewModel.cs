using SimpleMvvmToolkit.Express;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;

namespace TaskManager.ViewModels
{
    public class MainViewModel : ViewModelBase<MainViewModel>
    {
        Toast toast;
        List<Process> proc;
        private Dispatcher dispatcher;
        ObservableCollection<Proc> processList;

        public MainViewModel()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            processList = new ObservableCollection<Proc>();
            proc = Process.GetProcesses().GroupBy(p => p.ProcessName).Select(group => group.First()).ToList();
            Start();
        }

        public void Start() 
        {
            InstanciateProcesses();  // Start le thread, peut faire .wait() pour être sur que c'est fini. 
        }

        public void InstanciateProcesses()
        { 
            Icon Ico = null;
            foreach (Process proc in proc)
            {
                List<String> instancesNames = new PerformanceCounterCategory("Process")
                    .GetInstanceNames().Where(instanceName => instanceName.StartsWith(proc.ProcessName)).ToList();
                foreach (String instance in instancesNames)
                {
                    processList.Add(new Proc(instance, 0, 0, null));
                }
            }
        }

        public ObservableCollection<Proc> ProcessList
        {
            get { return processList;  }
            set { processList = value; NotifyPropertyChanged("ProcessName"); }
        }

        public void ShowToast() {
            Toast toast = new Toast();
            toast.showToast("coucou"); 
        }

        //Ico = Icon.ExtractAssociatedIcon(proc[i].MainModule.FileName); // Va chercher l'icône du processus. --> Ça chie toujours...
    }
}
  

