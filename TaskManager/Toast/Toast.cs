using System;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace TaskManager.ViewModels
{
    public class Toast
    {
        private Notifier notifier;

        public Toast()
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

        public void ShowMessage(string message, string type)
        {
            switch (type)
            {
                case "Information":
                    ShowInformationToast(message);
                    break;
                case "Success":
                    ShowSuccessToast(message);
                    break;
                case "Warning":
                    ShowWarningToast(message);
                    break;
                case "Error":
                    ShowInformationToast(message);
                    break;
            }
        }

        public void ShowInformationToast(String message)
        {
            notifier.ShowInformation(message);
        }

        public void ShowSuccessToast(String message)
        {
            notifier.ShowSuccess(message);
        }

        public void ShowWarningToast(String message)
        {
            notifier.ShowWarning(message);
        }

        public void ShowErrorToast(String message)
        {
            notifier.ShowError(message);
        }
    }
}
