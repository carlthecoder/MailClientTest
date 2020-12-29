using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public sealed class MailService : IMailService, IConnectionObserver
    {
        private IMailConnection connection;
        private CancellationTokenSource tokenSource;

        public ObservableCollection<MailInfo> MailInfos { get; } = new ObservableCollection<MailInfo>();
        public IList<MailBody> MailBodies { get; } = new List<MailBody>();

        TaskScheduler scheduler;

        public MailService(TaskScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public void GetAllMail(ConnectionDetails connectionDetails)
        {
            connection = ConnectionBase.CreateConnection(connectionDetails, scheduler);
            if (connection == null)
                return;

            connection.Register(this);
            tokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    connection.DownloadMailInfo(tokenSource.Token);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Mail retrieval cancelled - {exception}");
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void GetMailForInfo(MailInfo info)
        {
            connection.DownloadMailBody(info, tokenSource.Token);
        }

        public void CancelOperation()
        {
            HandleCancellation();
            ClearCaches();
        }

        private void HandleCancellation()
        {
            tokenSource?.Cancel();
            tokenSource?.Dispose();
            connection.Unregister(this);
        }

        private void ClearCaches()
        {
            MailBodies.Clear();
            MailInfos.Clear();
        }

        void IConnectionObserver.NewInfoAdded(MailInfo info)
        {
            App.Current.Dispatcher.InvokeAsync(() => MailInfos.Add(info));
        }

        void IConnectionObserver.NewBodyAdded(MailBody body)
        {
            App.Current.Dispatcher.InvokeAsync(() => MailBodies.Add(body));
        }
    }
}