using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using TradingHatsuwa.Data;

namespace TradingHatsuwa.Web.Models
{
    public class Evaluation
    {
        [Key]
        public int Id { get; set; }
        [Key]
        public virtual Meeting Meeting { get; set; }
        [Key]
        public virtual User User { get; set; }
        [Index]
        public EvaluationItem EvaluationItem { get; set; }
        [Required]
        public int Rating { get; set; }
        public virtual User CreatedBy { get; set; }
    }
}