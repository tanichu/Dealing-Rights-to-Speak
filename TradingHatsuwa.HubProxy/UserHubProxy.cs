using Microsoft.AspNet.SignalR.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradingHatsuwa.Data;

namespace TradingHatsuwa.HubProxy
{
	public interface IUserHubProxy
    {
        Task<User> CreateOrUpdate(User user);
        Task<IEnumerable<User>> GetUsers();
		Task<IEnumerable<Result>> GetResults(int userId);
	}

    public class UserHubProxy : IUserHubProxy
    {
        private IHubProxy _proxy;

        public UserHubProxy(IHubProxy proxy)
        {
            _proxy = proxy;
        }

        public async Task<User> CreateOrUpdate(User user) => await _proxy.Invoke<User>(args: user);
        public async Task<IEnumerable<User>> GetUsers()
        {
            //await Task.Delay(1000); // TODO: delete
            return await _proxy.Invoke<IEnumerable<User>>();

        }
		public async Task<IEnumerable<Result>> GetResults(int userId) { return await _proxy.Invoke<IEnumerable<Data.Result>>(args: userId); }
	}
}
