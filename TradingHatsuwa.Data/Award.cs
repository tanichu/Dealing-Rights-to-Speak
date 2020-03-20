using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHatsuwa.Data
{
	public class Award
	{
		public EvaluationItem EvaluationItem { get; set; }
		public IEnumerable<AwardUser> Users { get; set; }
	}

	public class AwardUser
	{
		public User User { get; set; }
		public double Score { get; set; }
	}
}
