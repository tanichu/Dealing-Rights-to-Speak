using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHatsuwa.Data
{
    public class Meeting
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Tickets { get; set; }
        public int Seconds { get; set; }
        public int Coupons { get; set; }
        public int IdleSeconds { get; set; }
        public MeetingStatus Status { get; set; }
        public int CreatedBy { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
