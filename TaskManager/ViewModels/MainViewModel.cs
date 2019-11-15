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
        private long totalRam; 
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
            Task.Run(() => UpdateAllProcesses());
        }

        public void UpdateAllProcesses()
        {
            while (true)
            {
                Processes = Process.GetProcesses().GroupBy(p => p.ProcessName).Select(group => group.First()).ToList();
                foreach (Process Process in Processes)
                {
                    List<String> ProcessInstancesNames = GetInstancesNames(Process);
                    Task.Run(() => PopulateList(ProcessInstancesNames)); // Affiche les informations des processus dans le XAML.
                }
                Dispatch.Invoke(() => 
                {
                    TotalRam = UpdateRamCounter();
                });
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
            Proc OldProcess = ProcessList.FirstOrDefault(name => name.Name == NewProcess.Name); // Retoune le processus dans la liste. 
            if (OldProcess != null) 
            {
                int index = processList.IndexOf(OldProcess);
                ProcessList[index].Cpu = NewProcess.Cpu; 
                ProcessList[index].Ram = NewProcess.Ram; 
                return; 
            }
            ProcessList.Add(NewProcess); //Si le processus n'est pas dans la liste, on l'ajoute. 
        }

        public Proc GetProcessInfos(String ProcessName) 
        {
            PerformanceCounter RamCounter; 
            PerformanceCounter CpuCounter;
            double Ram = 0;
            double Cpu = 0;
            String FormattedCpu = String.Empty; 
            String FormattedRam = String.Empty; 

            try
            {
                RamCounter = new PerformanceCounter("Process", "Working Set", ProcessName);
                CpuCounter = new PerformanceCounter("Process", "% Processor Time", ProcessName);
                Cpu = CpuCounter.NextValue();
                Thread.Sleep(500); // Laisser le temps au PerformanceCounter de calculer...
                Ram = RamCounter.NextValue();
                Cpu = CpuCounter.NextValue();

                FormattedRam = Ram.ToString(); //FormatRam(Ram);
                FormattedCpu = FormatCpu(Cpu); 
            }
            catch { Console.WriteLine("Process {0} died :(", ProcessName); }
            
            return new Proc(ProcessName, FormattedCpu, FormattedRam, null);  // TODO: ICONE 
        }

        public String FormatRam(Double Ram) 
        {
            Ram = Ram / 1024 / 1024;
            return Math.Round(Ram, 0).ToString(); 
        }

        public String FormatCpu(Double Cpu) 
        {
            return Math.Round(Cpu, 2).ToString(); 
        }

        public List<String> GetInstancesNames(Process Proc) // Retourne tous les noms d'instances (String) d'un processus (Process). 
        {
            return new PerformanceCounterCategory("Process")
                    .GetInstanceNames().Where(instanceName => instanceName.StartsWith(Proc.ProcessName)).ToList();
        }
        public long UpdateRamCounter()
        {
            long counter = 0; 
            foreach (Proc process in processList)
            {
                counter += Convert.ToInt64(process.Ram); 
            }
            return counter / 1024 / 1024; 
        }

        public long TotalRam
        {
            get { return totalRam;  }
            set { totalRam = value; NotifyPropertyChanged("TotalRam"); }
        }

        public ObservableCollection<Proc> ProcessList
        {
            get { return processList;  }
            set { processList = value;  NotifyPropertyChanged("ProcessList"); }
        }


        public void ShowToast(String message) {
            Toast toast = new Toast();
            toast.showToast(message); 
        }

        //Ico = Icon.ExtractAssociatedIcon(proc[i].MainModule.FileName); // Va chercher l'icône du processus. --> Ça chie toujours...
    }
}
  

