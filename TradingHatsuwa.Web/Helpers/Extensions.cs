using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TradingHatsuwa.Web.Models;

namespace TradingHatsuwa.Web.Helpers
{
    public static class Extensions
    {

        public static Data.User ToXferData(this User user)
        {
            return new Data.User
            {
                Id = user.Id,
                FacebookProfileId = user.FacebookProfileId,
                GuestId = user.GuestId,
                Name = user.Name,
            };
        }

        public static Data.EventUser ToXferData(this EventUser eventUser)
        {
            return new Data.EventUser
            {
                Id = eventUser.User.Id,
                FacebookProfileId = eventUser.User.FacebookProfileId,
                GuestId = eventUser.User.GuestId,
                Name = eventUser.User.Name,
                Tickets = eventUser.Tickets,
                Coupons = eventUser.Coupons,
				Begged = eventUser.Begged,
            };
        }

        public static Data.EventQueueItem ToXferData(this EventQueueItem item)
        {
            return new Data.EventQueueItem
            {
                UserId = item.User.Id,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                RandomlySelected = item.RandomlySelected,
            };
        }

        public static Data.Meeting ToXferData(this Meeting meeting)
        {
            return new Data.Meeting
            {
                Id = meeting.Id,
                Name = meeting.Name,
                Tickets = meeting.Tickets,
                Seconds = meeting.Seconds,
                Coupons = meeting.Coupons,
                IdleSeconds = meeting.IdleSeconds,
                Status = meeting.Status,
                CreatedBy = meeting.CreatedBy?.Id ?? -1,
            };
        }
    }
}