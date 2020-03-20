using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradingHatsuwa.Data;

namespace TradingHatsuwa.HubProxy
{
	public interface IMeetingHubProxy
    {
        Task<Meeting> Create(Meeting meeting);
        Task<bool> Update(Meeting meeting);
        Task<bool> Delete(Meeting meeting);
		Task<IEnumerable<Meeting>> GetMeetings();
		Task<IEnumerable<User>> GetParticipants(int meetingId);
		Task<bool> IsEvaluated(int meetingId, int userId);
		Task<bool> RegisterRatings(int meetingId, Dictionary<int, Dictionary<EvaluationItem, int/*rating*/>> userRatings, int userId);
		Task<IEnumerable<Result>> GetResults(int meetingId);
		Task<IEnumerable<Award>> GetAwards(int meetingId);
		IDisposable OnCreate(Action<Meeting> onData);
        IDisposable OnUpdate(Action<Meeting> onData);
        IDisposable OnDelete(Action<Meeting> onData);
    }

    public class MeetingHubProxy : IMeetingHubProxy
    {
        private IHubProxy _proxy;

        public MeetingHubProxy(IHubProxy proxy)
        {
            _proxy = proxy;
        }

        public async Task<Meeting> Create(Meeting meeting) => await _proxy.Invoke<Meeting>(args: meeting);
        public async Task<bool> Update(Meeting meeting) => await _proxy.Invoke<bool>(args: meeting);
        public async Task<bool> Delete(Meeting meeting) => await _proxy.Invoke<bool>(args: meeting);
		public async Task<IEnumerable<Meeting>> GetMeetings() => await _proxy.Invoke<IEnumerable<Meeting>>();
		public async Task<IEnumerable<User>> GetParticipants(int meetingId) => await _proxy.Invoke<IEnumerable<User>>(args: meetingId);
		public async Task<bool> IsEvaluated(int meetingId, int userId) => await _proxy.Invoke<bool>(args: new object[] { meetingId, userId });
		public async Task<bool> RegisterRatings(int meetingId, Dictionary<int, Dictionary<EvaluationItem, int/*rating*/>> userRatings, int userId) => await _proxy.Invoke<bool>(args: new object[] {meetingId, userRatings, userId});
		public async Task<IEnumerable<Result>> GetResults(int meetingId) { return await _proxy.Invoke<IEnumerable<Result>>(args: meetingId); }
		public async Task<IEnumerable<Award>> GetAwards(int meetingId) { return await _proxy.Invoke<IEnumerable<Award>>(args: meetingId); }
		public IDisposable OnCreate(Action<Meeting> onData) => _proxy.On(onData);
        public IDisposable OnUpdate(Action<Meeting> onData) => _proxy.On(onData);
        public IDisposable OnDelete(Action<Meeting> onData) => _proxy.On(onData);
    }
}
