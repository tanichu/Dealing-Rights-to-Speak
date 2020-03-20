using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace TradingHatsuwa.Web.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Index]
        [MaxLength(256)]
        public string FacebookProfileId { get; set; }
        [Index]
        public Guid GuestId { get; set; }
        public string Name { get; set; }
        public bool Deleted { get; set; }

        /// <summary>
        /// 参加した会議
        /// </summary>
        public virtual ICollection<Meeting> Meetings { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}