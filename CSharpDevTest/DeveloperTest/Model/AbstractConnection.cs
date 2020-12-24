using System.Collections.Generic;

namespace DeveloperTest.Model
{
    public abstract class AbstractConnection
    {
        protected readonly IList<IConnectionObserver> observers = new List<IConnectionObserver>();

        public void Register(IConnectionObserver observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
        }

        public void Unregister(IConnectionObserver observer)
        {
            if (observers.Contains(observer))
                observers.Remove(observer);
        }

        protected void NotifyObserversMailInfoAdded(MailInfo info)
        {
            foreach (var observer in observers)
            {
                observer.NewInfoAdded(info);
            }
        }

        protected void NotifyObserversMailBodyAdded(MailBody body)
        {
            foreach (var observer in observers)
            {
                observer.NewBodyAdded(body);
            }
        }
    }
}
