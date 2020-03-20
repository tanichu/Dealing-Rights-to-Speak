using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace TradingHatsuwa.Web.Models
{
    public class EventUser
    {
        [Key]
        public int Id { get; set; }
        [Key]
        public virtual Meeting Meeting { get; set; }
        [Key]
        public virtual User User { get; set; }

        [Required]
        public int Tickets { get; set; }
        [Required]
        public int Coupons { get; set; }
		[Required]
		public bool Begged { get; set; }
	}
}