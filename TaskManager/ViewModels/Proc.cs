using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.ViewModels
{
    public class Proc
    {
        public string Name { get; set; }
        public double Cpu { get; set; }
        public long Ram { get; set; }
        public Icon Icon { get; set; }


        public Proc(String name, double cpu, long ram, Icon Icon)
        {
            this.Name = name;
            this.Cpu = cpu;
            this.Ram = ram;
            this.Icon = Icon; 
        }

        public String GetInfos()
        {
            return $"Process {Name} uses {Cpu}MB of CPU and {Ram}MB of RAM"; 
        }
    }
}
