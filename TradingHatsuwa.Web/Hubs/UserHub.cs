using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using TradingHatsuwa.Web.Helpers;
using TradingHatsuwa.Web.Models;
using System;

namespace TradingHatsuwa.Web
{
    [HubName("UserHub")]
    public class UserHub : Hub
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public Data.User CreateOrUpdate(Data.User user)
        {
            var dbUser = db.Users.SingleOrDefault(i =>
                i.FacebookProfileId == user.FacebookProfileId &&
                i.GuestId == user.GuestId);

            if (dbUser == null)
            {
                // Insert
                var newUser = new User
                {
                    FacebookProfileId = user.FacebookProfileId,
                    GuestId = user.GuestId,
                    Name = user.Name,
                };
                db.Users.Add(newUser);
                db.SaveChanges();

                return newUser.ToXferData();
            }
            else
            {
                if (user.Name != dbUser.Name)
                {
                    // Update
                    dbUser.Name = user.Name;
                    db.SaveChanges();
                }

                return dbUser.ToXferData();
            }
        }

        public IEnumerable<Data.User> GetUsers()
        {
            var users = db.Users.Where(i => !i.Deleted).ToList();
            return users.Select(i => i.ToXferData());
        }

		public IEnumerable<Data.Result> GetResults(int userId)
		{
			var dbUser = db.Users.SingleOrDefault(i => i.Id == userId && !i.Deleted);
			if (dbUser == null) return null;

			var list = new List<Data.Result>();
			foreach (var item in Enum.GetValues(typeof(Data.EvaluationItem)))
			{
				double? score = null;
				if (db.Evaluations.Where(x => x.User.Id == userId && x.EvaluationItem == (Data.EvaluationItem)item).Any())
				{
					score = db.Evaluations.Where(x => x.User.Id == userId && x.EvaluationItem == (Data.EvaluationItem)item).Average(y => y.Rating);
				}
				list.Add(new Data.Result() {
					EvaluationItem = (Data.EvaluationItem)item,
					Score = score
				});
			}
			return list.ToArray();
		}
    }
}