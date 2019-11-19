using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.ViewModels;

namespace TaskManager.Client
{
    class Rule
    {
        string name { get; set; }
        string bindedProcessName { get; set; }
        string resourceType { get; set; }
        double threshold { get; set; }
        char condition { get; set; }
        string notificationType { get; set; }
    }
}
