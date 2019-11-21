using SimpleMvvmToolkit.Express;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;
using TaskManager.Client;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TaskManager.ViewModels
{
    public class MainViewModel : ViewModelBase<MainViewModel>
    {
        Toast toast; 
        private Dispatcher Dispatch;
        ObservableCollection<Proc> processList; 
        ObservableCollection<Rule> rulesList;  
        private ICommand _deleteRuleCommand;
        private ICommand _addNewRule;
        private Json Json; 
        public Proc newRuleProcess;
        private ResourceCounter ResourceCounter; 

        // Variables pour le XAML --> Pas MVVM ? Incapable de binder seulement le contenu du ComboBoxItem...
        public ComboBoxItem newRuleResourceType;
        public ComboBoxItem newRuleCondition;
        public ComboBoxItem newRuleNotificationType;
        public string newRuleThreshold;

        public MainViewModel()
        {
            toast = new Toast();
            Json = new Json();
            ResourceCounter = new ResourceCounter(); 
            Dispatch = Application.Current.Dispatcher;
            rulesList = Json.GetRulesListFromJson();
            processList = new ObservableCollection<Proc>();
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
                List<Process> Processes = Process.GetProcesses().GroupBy(p => p.ProcessName).Select(group => group.First()).ToList();
                foreach (Process Process in Processes)
                {
                    List<String> ProcessInstancesNames = GetInstancesNames(Process);
                    Task.Run(() => PopulateList(ProcessInstancesNames)); // Affiche les informations des processus dans le XAML.
                }
                Dispatch.Invoke(() => {
                    ResourceCounter.Update(processList);
                    TotalRam = ResourceCounter.TotalRam;
                    TotalCpu = ResourceCounter.TotalCpu; 
                    UpdateProcessesRuleBinding();
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
            BitmapSource Icon = null;

            try
            {
                Id = (int)(new PerformanceCounter("Process", "ID Process", ProcessName, true).RawValue);
                Icon = GetApplicationIcon(Id); 
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

            return new Proc(ProcessName, FormattedCpu, FormattedRam, Icon, Id);  // TODO: ICONE 
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

        private BitmapSource GetApplicationIcon(int Id)
        {
            BitmapSource Icon = null;
            try
            {
                Icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                         (System.Drawing.Icon.ExtractAssociatedIcon(Process.GetProcessById(Id).MainModule.FileName).ToBitmap)().GetHbitmap(),
                         IntPtr.Zero,
                         Int32Rect.Empty,
                         BitmapSizeOptions.FromEmptyOptions());
                Icon.Freeze();
            }
            catch { }; 
            return Icon;
        }

        /*
         * Logique de la gestion des règles
         */

        public void UpdateProcessesRuleBinding() 
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
            NotifyTriggeredRules();
        }

        public void NotifyTriggeredRules() 
        {
            foreach (Rule Rule in rulesList)
            {
                if (!Rule.Flagged)  // Si la règle n'a pas déjà été affichée (Pour éviter le spam). 
                {
                    if (!IsThresholdHit(Rule)) // Si la règle a été brisée. 
                    {
                        int Index = rulesList.IndexOf(Rule);
                        string message = Rule.GetActivatedMessage(); 
                        toast.ShowMessage(message, Rule.NotificationType); // Afficher la toast avec le message
                        Rule.Flagged = true; //Marquer la règle comme affichée 
                        RulesList[Index].Active = Rule.Active;
                        Console.WriteLine(Rule.Active);
                    }
                }
            }
        }

        public void AddNewRule()
        {
            string name = newRuleProcess.Name;
            string type = newRuleResourceType.Content.ToString();
            char condition = newRuleCondition.Content.ToString() == "Plus grand que" ? '>' : '<'; // Refactor pls
            string threshold = newRuleThreshold;
            string notification = newRuleNotificationType.Content.ToString();

            Rule r = new Rule(name, type, Math.Round(Convert.ToDouble(threshold),1), condition, notification);
            AddToRulesList(r); 
        }

        public void AddToRulesList(Rule Rule)
        {
            Dispatch.Invoke(() => { rulesList.Add(Rule); });
            Json.AddToJsonRuleList(Rule); 
        }

        public void DeleteRule(Rule param)
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
        
        public bool IsThresholdHit(Rule Rule)
        {
            try
            {
                Proc Process = processList.Where(x => x.Name.Equals(Rule.BindedProcessName)).First();  
                return Rule.isTriggered(Process);
            }
            catch { }
            return false; 
        }

        // Afficher les notifications 

        public void ShowToast(String message, String type)
        {
            Toast toast = new Toast();
            toast.ShowMessage(message, type);
        }

        /* 
         * Bindings avec le XAML 
         */
         
        // Collections 
        public ObservableCollection<Proc> ProcessList
        {
            get { return processList; }
            set { processList = value; NotifyPropertyChanged("ProcessList"); }
        }

        public ObservableCollection<Rule> RulesList
        {
            get { return rulesList; }
            set { rulesList = value; NotifyPropertyChanged("RulesList"); }
        }

        //Compteurs
        public string TotalCpu
        {
            get { return "CPU (" + ResourceCounter.TotalCpu + "%)"; }
            set { ResourceCounter.TotalCpu = value; NotifyPropertyChanged("TotalCpu"); }
        }

        public string TotalRam
        {
            get { return "RAM (" + ResourceCounter.TotalRam + "Mb)"; }
            set { ResourceCounter.TotalRam = value; NotifyPropertyChanged("TotalRam"); }
        }

        // Commandes
        public ICommand CommandDeleteRule
        {
            get { return _deleteRuleCommand ?? (_deleteRuleCommand = new DelegateCommand<Rule>(DeleteRule));}
        }

        public ICommand BtnAddNewRule
        {
            get{ return _addNewRule ?? (_addNewRule = new DelegateCommand(AddNewRule)); }
        }

        // Création d'un règle
        public Proc GetRuleSelectedProcess 
        {
            get { return newRuleProcess;  }
            set { newRuleProcess = value; }
        }
        public string GetRuleThreshold
        {
            get { return newRuleThreshold; }
            set { newRuleThreshold = value; NotifyPropertyChanged("GetRuleThreshold");}
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
    }
}