using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHatsuwa.Data
{
    public class Event
    {
        public int MeetingId { get; set; }
		public MeetingStatus Status { get; set; }
		public IEnumerable<EventUser> Users { get; set; }
        public IEnumerable<EventQueueItem> QueueItems { get; set; }
    }
}
