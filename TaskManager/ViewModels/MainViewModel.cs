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
using TaskManager.Client;
using System.Windows.Controls;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Xml.Linq;
using MessageBox = System.Windows.MessageBox;
using Newtonsoft.Json.Linq;

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
        ObservableCollection<Rule> rulesList; // List de toutes les règles dans le XAML. 
        private ICommand _modifyRulesCommand;
        private ICommand _addNewRule;
        private Json Json; 

        public Proc newRuleProcess;
        public ComboBoxItem newRuleResourceType;
        public string newRuleThreshold;
        public ComboBoxItem newRuleCondition;
        public ComboBoxItem newRuleNotificationType; 

        public MainViewModel()
        {
            toast = new Toast();
            Json = new Json(); 
            Dispatch = Application.Current.Dispatcher; // Instancie le dispatcher qui gère la concurrence. 
            rulesList = Json.GetRulesListFromJson(); // Instancie la liste des règles dans le XAML.
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
                Dispatch.Invoke(() => { 
                    TotalRam = UpdateRamCounter(); 
                    TotalCpu = UpdateCpuCounter();
                    CheckForRules();
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

        public ObservableCollection<Rule> RulesList
        {
            get { return rulesList;  }
            set { rulesList = value; NotifyPropertyChanged("RulesList"); }
        }

        public void CheckForRules() 
        {
            foreach (Proc p in processList)
            {
                foreach (Rule r in rulesList)
                {
                    if (r.BindedProcessName.Equals(p.Name))
                    {
                        p.Watched = true; 
                    }
                }
            }
            ShowTriggeredRules();
        }

        public void ShowTriggeredRules() 
        {
            foreach (Rule r in rulesList)
            {
                if (r.Flagged == false)  // Si la règle n'a pas déjà été affichée.
                {
                    if (ThresholdIsHit(r))
                    {
                        int index = rulesList.IndexOf(r);
                        string message = "La règle " + r.BindedProcessName + " " + r.Condition + " " + r.Threshold + " est activée.";
                        toast.ShowMessage(message, r.NotificationType); // Afficher la toast avec le message
                        r.Flagged = true; //Marquer la règle comme affichée 
                        RulesList[index].Active = r.Active;
                        Console.WriteLine(r.Active);
                    }
                }
            }
        }

        public ICommand ModifyRule
        {
            get 
            {
                Console.WriteLine("Tu as cliqué");
                return _modifyRulesCommand ?? (_modifyRulesCommand = new DelegateCommand<Rule>(Modify)); 
            }
        }

        public ICommand BtnAddNewRule
        {
            get
            {
                return _addNewRule ?? (_addNewRule = new DelegateCommand(AddNewRule));
            }
        }

        public void AddNewRule()
        {
            string name = newRuleProcess.Name;
            string type = newRuleResourceType.Content.ToString();
            char condition = newRuleCondition.Content.ToString().ToCharArray()[0];
            string threshold = newRuleThreshold;
            string notification = newRuleNotificationType.Content.ToString();

            Rule r = new Rule(name, type, Convert.ToDouble(threshold), condition, notification);
            AddToRulesList(r); 
        }

        public void AddToRulesList(Rule r)
        {
            Dispatch.Invoke(() => { rulesList.Add(r); });
            Json.AddToJsonRuleList(r); 
        }

        public void Modify(Rule param)
        {
            try
            {
                Dispatch.Invoke(() => {
                    System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show(
                        "Voulez vous vraiment supprimer " + param.BindedProcessName + "?", "Supprimer une règle", System.Windows.Forms.MessageBoxButtons.YesNo);
                    if (dr == System.Windows.Forms.DialogResult.Yes)
                    {
                        rulesList.Remove(rulesList.Select(x => x).Where(x => x.BindedProcessName.Equals(param.BindedProcessName)).FirstOrDefault());
                        Json.UpdateJsonList(rulesList); 
                    } 
                });
            }
            catch { }
        }

        public void CreateNewRule()
        {
            Console.WriteLine("Je crée une nouvelle règle.");
        }
        
        public bool ThresholdIsHit(Rule r)
        {
            try
            {
                Proc p = processList.Where(x => x.Name.Equals(r.BindedProcessName)).First();  // Trouver le processus attaché à la règle
                return r.isTriggered(p);
            }
            catch { }
            return false; 
        }

        public Proc GetRuleSelectedProcess 
        {
            get { return newRuleProcess;  }
            set { newRuleProcess = value; }
        }
        public string GetRuleThreshold
        {
            get { return newRuleThreshold; }
            set 
            { newRuleThreshold = value; NotifyPropertyChanged("GetRuleThreshold");}
        }
        public ComboBoxItem GetRuleResourceType
        {
            get { return newRuleResourceType; }
            set { newRuleResourceType = value; }
        }
        public ComboBoxItem GetRuleSelectedCondition
        {
            get { return newRuleCondition; }
            set { newRuleCondition = value; }
        }
        public ComboBoxItem GetRuleSelectedNotificationType
        {
            get { return newRuleNotificationType; }
            set { newRuleNotificationType = value; }
        }

        public void ShowToast(String message, String type) {
            Toast toast = new Toast();
            toast.ShowMessage(message, type); 
        }
    }
}