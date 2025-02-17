



using Microsoft.EntityFrameworkCore;
using Schedify.Models;

namespace Schedify.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventBooking> EventBookings { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<BillingAddress> BillingAddresses { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationUser> ConversationUsers { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Resource> Resources { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure EventBookings -> Events relationship to restrict cascading delete.
        modelBuilder.Entity<EventBooking>()
            .HasOne(e => e.Event)
            .WithMany(e => e.EventBookings)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Feedbacks -> Events relationship to restrict cascading delete.
        modelBuilder.Entity<Feedback>()
            .HasOne(e => e.Event)
            .WithMany(e => e.Feedbacks)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure ConversationUsers -> Events relationship to restrict cascading delete.
        modelBuilder.Entity<ConversationUser>()
            .HasOne(e => e.Conversation)
            .WithMany(e => e.ConversationUsers)
            .HasForeignKey(e => e.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Messages -> Events relationship to restrict cascading delete.
        modelBuilder.Entity<Message>()
            .HasOne(e => e.Conversation)
            .WithMany(e => e.Messages)
            .HasForeignKey(e => e.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);

        // // Enums (User)
        // modelBuilder.Entity<User>()
        //     .Property(e => e.Role)
        //     .HasConversion(
        //         v => v.ToString(),
        //         v => (UserRoles)Enum.Parse(typeof(UserRoles), v))
        //     .HasMaxLength(15);

        // // Enums (Event)
        // modelBuilder.Entity<Event>()
        //     .Property(e => e.Status)
        //     .HasConversion(
        //         v => v.ToString(),
        //         v => (EventStatus)Enum.Parse(typeof(EventStatus), v))
        //     .HasMaxLength(15);

        // // Enums (EventBooking)
        // modelBuilder.Entity<EventBooking>()
        //     .Property(e => e.Status)
        //     .HasConversion(
        //         v => v.ToString(),
        //         v => (BookingStatus)Enum.Parse(typeof(BookingStatus), v))
        //     .HasMaxLength(15);

        // // Enums (Message)
        // modelBuilder.Entity<Message>()
        //     .Property(e => e.Status)
        //     .HasConversion(
        //         v => v.ToString(),
        //         v => (MessageStatus)Enum.Parse(typeof(MessageStatus), v))
        //     .HasMaxLength(15);

        // // Enums (Resource)
        // modelBuilder.Entity<Resource>()
        //     .Property(e => e.Type)
        //     .HasConversion(
        //         v => v.ToString(),
        //         v => (ResourceType)Enum.Parse(typeof(ResourceType), v))
        //     .HasMaxLength(30);

        // // Enums (ActivityLog)
        // modelBuilder.Entity<ActivityLog>()
        //    .Property(e => e.Type)
        //    .HasConversion(
        //        v => v.ToString(),
        //        v => (ActivityLogType)Enum.Parse(typeof(ActivityLogType), v))
        //    .HasMaxLength(15);
    }
}
