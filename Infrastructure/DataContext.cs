using ApplicationCore.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    public class DataContext : IdentityDbContext<User>
    {


        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<TopicReply> TopicReplys { get; set; }
        public DbSet<TopicNode> TopicNodes { get; set; }
        public DbSet<UserMessage> UserMessages { get; set; }
        public DbSet<UserCollection> UserTopics { get; set; }

        public DbSet<Referkey> Referkeys { get; set; }

        public DbQuery<MergeData> MergeDatas { get; set; }
         public DbQuery<Depalldata> Depalldatas { get; set; }
        public DbQuery<Depstatusdata> Depstatusdatas { get; set; }

        public DbQuery<DepMark> DepMark { get; set; }


        public DbSet<MarkInfo> MarkInfos { get; set; }

        public DbSet<Bonus> Bonuss { get; set; }

        public DbSet<StatusLog> StatusLogs { get; set; }

        public DbSet<MyFile> MyFiles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Topic>().ToTable("Topic");
            modelBuilder.Entity<TopicReply>().ToTable("TopicReply");
            modelBuilder.Entity<TopicNode>().ToTable("TopicNode");
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<UserMessage>().ToTable("UserMessage");
            modelBuilder.Entity<UserCollection>().ToTable("UserCollection");
            modelBuilder.Entity<MyFile>().ToTable("MyFile");
            modelBuilder.Entity<MarkInfo>().ToTable("MarkInfo");
            modelBuilder.Entity<StatusLog>().ToTable("StatusLog");
            modelBuilder.Entity<Bonus>().ToTable("Bonus");
            modelBuilder.Entity<Referkey>().ToTable("Referkey");

            /*modelBuilder.Query<MonthTrend>(v =>
            {
                v.ToView("View_MonthTrend");
            });*/

            modelBuilder.Query<Depalldata>(v =>
            {
                v.ToView("View_Depalldata");
            });

            modelBuilder.Query<Depstatusdata>(v =>
            {
                v.ToView("View_Depstatusdata");
            });

            modelBuilder.Query<DepMark>(v =>
            {
                v.ToView("View_DepMark");
            });

            modelBuilder.Query<MergeData>(v =>
            {
                v.ToView("View_MergeReport");
                /*v.Property(p => p.TopicId).HasColumnName("TopicId");
                v.Property(p => p.TrUserName).HasColumnName("TrUserName");
                v.Property(p => p.TrDep).HasColumnName("TrDep");
                v.Property(p => p.TRealName).HasColumnName("TRealName");
                v.Property(p => p.Title).HasColumnName("Title");
                v.Property(p => p.TopicMark).HasColumnName("TopicMark");
                v.Property(p => p.NodeId).HasColumnName("NodeId");
                v.Property(p => p.Type).HasColumnName("Type");
                v.Property(p => p.UserName).HasColumnName("UserName");
                v.Property(p => p.TRealName).HasColumnName("TRealName");
                v.Property(p => p.ReplyCount).HasColumnName("ReplyCount");
                v.Property(p => p.CreateOn).HasColumnName("CreateOn");
                v.Property(p => p.PassTime).HasColumnName("PassTime");
                v.Property(p => p.FinishTime).HasColumnName("FinishTime");
                v.Property(p => p.TDepartment).HasColumnName("TDepartment");
                v.Property(p => p.ZongbuMark).HasColumnName("ZongbuMark");*/
            });
        }
    }
}
