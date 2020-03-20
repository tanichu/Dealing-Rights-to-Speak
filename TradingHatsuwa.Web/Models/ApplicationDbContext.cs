using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace TradingHatsuwa.Web.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("ApplicationDbContext")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<EventUser> EventUsers { get; set; }
        public DbSet<EventQueueItem> EventQueueItems { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Meetings)
                .WithMany(m => m.Members)                
                .Map(um => um.ToTable("UserMeetingRelations"));
            
        }
    }
}