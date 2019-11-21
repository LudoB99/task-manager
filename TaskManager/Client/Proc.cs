using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TaskManager.ViewModels
{
    public class Proc
    {
        public string Name { get; set; }
        public string Cpu { get; set; }
        public string Ram { get; set; }
        public BitmapSource Icon { get; set; }
        public bool Watched { get; set; }
        public int Id { get; set; }


        public Proc(String name, string cpu, string ram, BitmapSource Icon, int Id)
        {
            this.Name = name;
            this.Cpu = cpu;
            this.Ram = ram;
            this.Icon = Icon;
            this.Id = Id;
            Watched = false; //Par défaut le processus n'est pas surveillé.
        }
    }
}
