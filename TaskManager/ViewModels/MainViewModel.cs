using SimpleMvvmToolkit.Express;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace TaskManager.ViewModels
{
    public class MainViewModel : ViewModelBase<MainViewModel>
    {
        private Notifier notifier;
        private String message;

        public ICommand ShowToastCommand => new DelegateCommand(ShowToast);

        public MainViewModel()
        {
            notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new PrimaryScreenPositionProvider(
                    corner: Corner.BottomRight,
                    offsetX: 10,
                    offsetY: 10);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(3),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
        }
        public void ShowToast()
        {
            notifier.ShowInformation("Bonjour"); 
        }
    }
}
