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
        List<Process> Processes; // Liste de tous les processus actifs. 
        private Dispatcher Dispatch;
        ObservableCollection<Proc> processList; // Liste des processus dans le XAML.

        public MainViewModel()
        {
            Console.WriteLine("Démarrage");
            Dispatch = Application.Current.Dispatcher; // Instancie le dispatcher qui gère la concurrence. 
            processList = new ObservableCollection<Proc>(); // Instancie la liste des processus dans le XAML.
            Start(); 
        }
        public void Start() 
        {
            Thread Thread = new Thread (UpdateAllProcesses);
            Thread.Start();
        }

        public void UpdateAllProcesses()
        {
            while (true)
            {
                Processes = Process.GetProcesses().GroupBy(p => p.ProcessName).Select(group => group.First()).ToList();
                foreach (Process Process in Processes)
                {
                    List<String> ProcessInstancesNames = GetInstancesNames(Process);
                    Task.Run(() => PopulateList(ProcessInstancesNames)).Wait(); // Affiche les informations des processus dans le XAML.
                }
            }
        }

        public void PopulateList(List <String> InstanceNames) 
        {
            foreach (String InstanceName in InstanceNames)
            {
                Proc Process = GetProcessInfos(InstanceName);
                Dispatch.Invoke(() => UpdateList(Process)); /*UpdateList(Process)*/  // Affiche les informations d'un processus dans le XAML. 
            }
        }
        public void UpdateList(Proc NewProcess) 
        {
            Proc OldProcess = processList.FirstOrDefault(name => name.Name == NewProcess.Name); // Retoune le processus dans la liste. 
            if (OldProcess != null) 
            {
                OldProcess = NewProcess;
                return; 
            }
            processList.Add(NewProcess); //Si le processus n'est pas dans la liste, on l'ajoute. 
            Console.WriteLine("Nouveau processus: {0}", NewProcess.Name);
        }

        public Proc GetProcessInfos(String ProcessName) 
        {
            return new Proc(ProcessName, 0, 0, null); 
        }

        public List<String> GetInstancesNames(Process Proc) // Retourne tous les noms d'instances (String) d'un processus (Process). 
        {
            return new PerformanceCounterCategory("Process")
                    .GetInstanceNames().Where(instanceName => instanceName.StartsWith(Proc.ProcessName)).ToList();
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
  

