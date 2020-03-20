using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHatsuwa.Data
{
    public class User
    {
        public int Id { get; set; }
        public string FacebookProfileId { get; set; }
        public Guid GuestId { get; set; }
        public string Name { get; set; }
    }
}
