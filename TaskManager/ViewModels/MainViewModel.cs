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
        private string totalRam;
        private string totalCpu; 
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
                Dispatch.Invoke(() => { TotalRam = UpdateRamCounter(); });
                Dispatch.Invoke(() => { TotalCpu = UpdateCpuCounter(); });
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
            int Id = 0;
            Icon Ico = null;

            try
            {
                Id = (int)(new PerformanceCounter("Process", "ID Process", ProcessName, true).RawValue);
                //Ico = Icon.ExtractAssociatedIcon((Process.GetProcessById(Id).ProcessName)); 
                RamCounter = new PerformanceCounter("Process", "Working Set", ProcessName);
                CpuCounter = new PerformanceCounter("Process", "% Processor Time", ProcessName);
                Cpu = CpuCounter.NextValue() / Environment.ProcessorCount;
                Thread.Sleep(1000); // Laisser le temps au PerformanceCounter de calculer...
                Ram = RamCounter.NextValue();
                Cpu = CpuCounter.NextValue() / Environment.ProcessorCount;
                FormattedRam = FormatRam(Ram);
                FormattedCpu = FormatCpu(Cpu);
            }
            catch { Console.WriteLine("Process {0} died :(", ProcessName); }

            return new Proc(ProcessName, FormattedCpu, FormattedRam, Ico, Id);  // TODO: ICONE 
        }

        public String FormatRam(Double Ram) 
        {
            Ram = Ram / 1024 / 1024;
            return Math.Round(Ram, 1).ToString(); 
        }

        public String FormatCpu(Double Cpu) 
        {
            return Math.Round(Cpu, 1).ToString(); 
        }

        public List<String> GetInstancesNames(Process Proc) // Retourne tous les noms d'instances (String) d'un processus (Process). 
        {
            return new PerformanceCounterCategory("Process")
                    .GetInstanceNames().Where(instanceName => instanceName.StartsWith(Proc.ProcessName)).ToList();
        }
        public string UpdateRamCounter()
        {
            double counter = 0;
            foreach (Proc process in processList)
            {
                try
                { 
                    counter += Convert.ToDouble(process.Ram);
                }
                catch { }; 
               
            }
            return Math.Round(counter, 2).ToString(); 
        }

        public string UpdateCpuCounter()
        {
            double counter = 0;
            foreach (Proc process in processList)
            {
                try
                {
                    if (!process.Cpu.Equals("0") && !process.Name.Equals("Idle")) { //Idle prend ce qui est disponible? 
                        counter += Convert.ToDouble(process.Cpu);
                        Console.WriteLine("Conteur: {0}", counter);
                    }
                }
                catch { };
            }
            return Math.Round(counter, 2).ToString();
        }

        public string TotalCpu
        {
            get { return "CPU (" + totalCpu + "%)"; }
            set { totalCpu = value; NotifyPropertyChanged("TotalCpu"); }
        }

        public string TotalRam
        {
            get { return "RAM (" + totalRam + "Mb)";  }
            set { totalRam = value; NotifyPropertyChanged("TotalRam"); }
        }

        public ObservableCollection<Proc> ProcessList
        {
            get { return processList; }
            set { processList = value;  NotifyPropertyChanged("ProcessList"); }
        }


        public void ShowToast(String message, String type) {
            Toast toast = new Toast();
            toast.ShowMessage(message, type); 
        }
    }
}
  

