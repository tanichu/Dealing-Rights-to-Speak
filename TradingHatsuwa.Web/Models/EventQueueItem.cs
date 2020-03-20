using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace TradingHatsuwa.Web.Models
{
    public class EventQueueItem
    {
        [Key]
        public int Id { get; set; }
        [Key]
        public virtual Meeting Meeting { get; set; }

        public virtual User User { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        [Required]
        public bool RandomlySelected { get; set; }

    }
}