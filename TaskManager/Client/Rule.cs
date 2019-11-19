using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.ViewModels;

namespace TaskManager.Client
{
    public class Rule
    {
        public Rule(string BindedProcessName, string ResourceType, double Threshold, char Condition, string NotificationType)
        {
            this.BindedProcessName = BindedProcessName;
            this.ResourceType = ResourceType;
            this.Threshold = Threshold;
            this.Condition = Condition;
            this.NotificationType = NotificationType;
            Flagged = false; 
        }

        public bool isTriggered(Proc proc)
        {
            Console.WriteLine(); 
            switch (Condition) 
            {
                case '<': return SmallerThan(proc);
                case '>': return GreaterThan(proc);
            }
            return false; 
        }

        public bool SmallerThan(Proc proc)
        {
            switch (ResourceType)
            {
                case "Cpu": return Threshold < Convert.ToDouble(proc.Cpu); 
                case "Ram": return Threshold < Convert.ToDouble(proc.Ram); 
            }
            return false; 
        }

        public bool GreaterThan(Proc proc)
        {
            switch (ResourceType)
            {
                case "Cpu": return Threshold > Convert.ToDouble(proc.Cpu);
                case "Ram": return Threshold > Convert.ToDouble(proc.Ram);
            }
            return false; 
        }

        public string BindedProcessName { get; set; }
        public string ResourceType { get; set; }
        public double Threshold { get; set; }
        public char Condition { get; set; }
        public string NotificationType { get; set; }
        public bool Flagged { get; set; }
    }
}
