using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingHatsuwa.Data;

namespace TradingHatsuwa.HubProxy
{
    public interface IEventHubProxy
    {
        Task<bool> Enter(int meetingId, int userId, bool attended);
        Task<bool> Exit(int meetingId, int userId);
        Task<bool> Start(int meetingId, int userId);
        Task<bool> Beg(int meetingId, int userId, bool begged);
        Task<bool> UseTicket(int meetingId, int userId);
        Task<bool> UseCoupon(int meetingId, int userId, int targetUserId);
        Task<bool> Done(int meetingId, int userId);


        Task<Event> GetEvent(int meetingId);
        //Task<IEnumerable<EventUser>> GetUsers(int meetingId);
        //Task<IEnumerable<EventQueueItem>> GetQueueItems(int meetingId);

        IDisposable OnBeg(Action<Data.Beggar> onData);
        IDisposable OnUpdate(Action<Data.Event> onData);
    }

    public class EventHubProxy : IEventHubProxy
    {
        private IHubProxy _proxy;

        public EventHubProxy(IHubProxy proxy)
        {
            _proxy = proxy;
        }

        public async Task<bool> Enter(int meetingId, int userId, bool attended) => await _proxy.Invoke<bool>(args: new object[] { meetingId, userId, attended });
        public async Task<bool> Exit(int meetingId, int userId) => await _proxy.Invoke<bool>(args: new object[] { meetingId, userId });
        public async Task<bool> Start(int meetingId, int userId) => await _proxy.Invoke<bool>(args: new object[] { meetingId, userId });
        public async Task<bool> Beg(int meetingId, int userId, bool begged) => await _proxy.Invoke<bool>(args: new object[] { meetingId, userId, begged });
        public async Task<bool> UseTicket(int meetingId, int userId) => await _proxy.Invoke<bool>(args: new object[] { meetingId, userId });
        public async Task<bool> UseCoupon(int meetingId, int userId, int targetUserId) => await _proxy.Invoke<bool>(args: new object[] { meetingId, userId, targetUserId });
        public async Task<bool> Done(int meetingId, int userId) => await _proxy.Invoke<bool>(args: new object[] { meetingId, userId });
        public async Task<Event> GetEvent(int meetingId) => await _proxy.Invoke<Event>(args: meetingId);
        //public async Task<IEnumerable<Data.EventUser>> GetUsers(int meetingId) => await _proxy.Invoke<IEnumerable<Data.EventUser>>(args: meetingId);
        //public async Task<IEnumerable<Data.EventQueueItem>> GetQueueItems(int meetingId) => await _proxy.Invoke<IEnumerable<Data.EventQueueItem>>(args: meetingId);
        public IDisposable OnBeg(Action<Data.Beggar> onData) => _proxy.On(onData);
        public IDisposable OnUpdate(Action<Data.Event> onData) => _proxy.On(onData);
    }
}
