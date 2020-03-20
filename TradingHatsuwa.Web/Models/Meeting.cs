using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TradingHatsuwa.Data;

namespace TradingHatsuwa.Web.Models
{
    public class Meeting
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Tickets { get; set; }
        [Required]
        public int Seconds { get; set; }
        [Required]
        public int Coupons { get; set; }
        [Required]
        public int IdleSeconds { get; set; }
        [Required]
        public MeetingStatus Status { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        
        public virtual User CreatedBy { get; set; }
        public bool Deleted { get; set; }

        /// <summary>
        /// 参加者
        /// </summary>
        public virtual ICollection<User> Members { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}