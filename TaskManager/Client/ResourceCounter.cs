using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.ViewModels;

namespace TaskManager.Client
{
    class ResourceCounter
    {
        public string TotalRam { get; set; }
        public string TotalCpu { get; set; }

        public void Update(ObservableCollection<Proc> List) 
        {
            List<string> CpuData = new List<string>(); 
            List<string> RamData = new List<string>(); 
            foreach (Proc Process in List)
            {
                if (!Process.Name.Equals("Idle") && !Process.Cpu.Equals("0")) // :( 
                {
                    CpuData.Add(Process.Cpu);
                }
                if (!Process.Ram.Equals("0")) { RamData.Add(Process.Ram); }
            }
            TotalCpu = Count(CpuData);
            TotalRam = Count(RamData); 
        }

        public string Count(List<String> Data)
        {
            double Counter = 0;
            foreach (String D in Data)
            {
                if (!D.Equals(string.Empty)) {
                    Counter += Convert.ToDouble(D); // ;) 
                }
            }
            return Math.Round(Counter, 2).ToString(); 
        }
    }
}
