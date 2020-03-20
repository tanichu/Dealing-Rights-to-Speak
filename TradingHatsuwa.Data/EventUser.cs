using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHatsuwa.Data
{
    public class EventUser : User
    {
        public int Tickets { get; set; }
        public int Coupons { get; set; }
		public bool Begged { get; set; }

	}
}
