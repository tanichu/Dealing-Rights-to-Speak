using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using TradingHatsuwa.Web.Helpers;
using TradingHatsuwa.Web.Models;

namespace TradingHatsuwa.Web
{
	[HubName("MeetingHub")]
    public class MeetingHub : Hub
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public Data.Meeting Create(Data.Meeting meeting)
        {
            var dbUser = db.Users.SingleOrDefault(i => i.Id == meeting.CreatedBy);
            if (dbUser == null)
            {
                return null;
            }

            // Insert
            var newMeeting = new Meeting
            {
                Name = meeting.Name,
                Tickets = meeting.Tickets,
                Seconds = meeting.Seconds,
                Coupons = meeting.Coupons,
                IdleSeconds = meeting.IdleSeconds,
                Status = Data.MeetingStatus.Waiting,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dbUser,
            };
            db.Meetings.Add(newMeeting);
            db.SaveChanges();

            var m = newMeeting.ToXferData();
            Clients.Others.Create(m);
            return m;
        }

        public bool Update(Data.Meeting meeting)
        {
            var dbUser = db.Users.SingleOrDefault(i => i.Id == meeting.CreatedBy);
            if (dbUser == null)
            {
                return false;
            }

            var dbMeeting = db.Meetings.SingleOrDefault(i => i.Id == meeting.Id && i.CreatedBy.Id == dbUser.Id);
            if (dbMeeting == null)
            {
                return false;
            }

            // Update
            // status
            dbMeeting.Status = meeting.Status;
            db.SaveChanges();

            Clients.Others.Update(dbMeeting.ToXferData());
            return true;
        }

        public bool Delete(Data.Meeting meeting)
        {
            var dbUser = db.Users.SingleOrDefault(i => i.Id == meeting.CreatedBy);
            if (dbUser == null)
            {
                return false;
            }

            var dbMeeting = db.Meetings.SingleOrDefault(i => i.Id == meeting.Id && i.CreatedBy.Id == dbUser.Id);
            if (dbMeeting == null)
            {
                return false;
            }

            // Delete
            dbMeeting.Deleted = true;
            db.SaveChanges();

            Clients.Others.Delete(dbMeeting.ToXferData());
            return true;
        }

		public IEnumerable<Data.Meeting> GetMeetings()
        {
            var meetings = db.Meetings
                .Where(i => !i.Deleted)
                .OrderByDescending(i => i.CreatedAt).ToList();
            return meetings.Select(i => i.ToXferData());
        }

		public IEnumerable<Data.User> GetParticipants(int meetingId)
		{
			var dbMeeting = db.Meetings.SingleOrDefault(i => i.Id == meetingId && !i.Deleted);
			if (dbMeeting == null) return null;
			return dbMeeting.Members.Select(i => i.ToXferData());
		}

		public bool IsEvaluated(int meetingId, int userId)
		{
			if (db.Evaluations.Any(i => i.Meeting.Id == meetingId && i.CreatedBy.Id == userId))
			{
				return true; // 登録済み
			}
			return false;
		}

		public bool RegisterRatings(int meetingId, Dictionary<int, Dictionary<Data.EvaluationItem, int/*rating*/>> userRatings, int userId)
        {
            var dbMeeting = db.Meetings.SingleOrDefault(i => i.Id == meetingId && !i.Deleted);
            if (dbMeeting == null)
                return false;

            var dbUser = db.Users.SingleOrDefault(i => i.Id == userId && !i.Deleted);
            if (dbUser == null)
                return false;

            if (db.Evaluations.Any(i => i.Meeting.Id == dbMeeting.Id && i.CreatedBy.Id == dbUser.Id))
            {
                return false; // 登録済み
            }

            if (dbMeeting.Status == Data.MeetingStatus.InMeeting)
            {
                var maxEndTime = db.EventQueueItems.Where(i => i.Meeting == dbMeeting)?.Max(i => i.EndTime);
                if (!maxEndTime.HasValue) return false;

                if (DateTime.UtcNow > maxEndTime)
                {
                    // 会議クローズ処理
                    dbMeeting.Status = Data.MeetingStatus.Closed;
                    db.SaveChanges();
                }
            }

            var list = new List<Evaluation>();
			foreach (var member in dbMeeting.Members)
			{
				if (!userRatings.ContainsKey(member.Id))
					return false;

				foreach (var item in Enum.GetValues(typeof(Data.EvaluationItem)))
				{
					var ei = (Data.EvaluationItem)item;
					if (!userRatings[member.Id].ContainsKey(ei))
						return false;

					var e = new Evaluation
					{
						Meeting = dbMeeting,
						CreatedBy = dbUser,
						EvaluationItem = ei,
						Rating = userRatings[member.Id][ei],
						User = member,
					};
					list.Add(e);
				}
			}

			db.Evaluations.AddRange(list);
            db.SaveChanges();

            return true;
        }

		public IEnumerable<Data.Result> GetResults(int meetingId)
		{
			var dbMeeting = db.Meetings.SingleOrDefault(i => i.Id == meetingId && !i.Deleted);
			if (dbMeeting == null) return null;

			var list = new List<Data.Result>();
			foreach (var item in Enum.GetValues(typeof(Data.EvaluationItem)))
			{
				double? score = null;
				if (db.Evaluations.Where(x => x.Meeting.Id == meetingId && x.EvaluationItem == (Data.EvaluationItem)item).Any())
				{
					score = db.Evaluations.Where(x => x.Meeting.Id == meetingId && x.EvaluationItem == (Data.EvaluationItem)item).Average(y => y.Rating);
				}
				list.Add(new Data.Result()
				{
					EvaluationItem = (Data.EvaluationItem)item,
					Score = score
				});
			}
			return list.ToArray();
		}

		public IEnumerable<Data.Award> GetAwards(int meetingId)
		{
			var dbMeeting = db.Meetings.SingleOrDefault(i => i.Id == meetingId && !i.Deleted);
			if (dbMeeting == null) return null;

			var list = new List<Data.Award>();
			foreach (var item in Enum.GetValues(typeof(Data.EvaluationItem)))
			{
				var users = new List<Data.AwardUser>();
				var dbEvaluations = db.Evaluations.Where(x => x.Meeting.Id == meetingId && x.EvaluationItem == (Data.EvaluationItem)item);
				if (dbEvaluations.Any())
				{
					// Userごとの平均点のリスト
					var scores = new List<Data.AwardUser>();
					foreach (var e in dbEvaluations.GroupBy(y => new { y.Meeting, y.EvaluationItem, y.User }))
					{
						scores.Add(new Data.AwardUser { User = e.Key.User.ToXferData(), Score= dbEvaluations.Where(x => x.User.Id == e.Key.User.Id).Average(y => y.Rating) });
					}
					// 平均点の最大値
					var max = scores.Max(y => y.Score);
					// リストから最大値と同じ平均点を持つユーザーを探す
					foreach (var e in scores.Where(x => x.Score == max))
					{
						var dbUser = db.Users.SingleOrDefault(i => i.Id == e.User.Id && !i.Deleted);
						if (dbUser != null)
						{
							users.Add(new Data.AwardUser() { User = e.User, Score = max });
						}
					}
				}

				list.Add(new Data.Award()
				{
					EvaluationItem = (Data.EvaluationItem)item,
					Users = users.ToArray()
				});
			}
			return list.ToArray();
		}
	}
}