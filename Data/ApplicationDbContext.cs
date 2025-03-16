



using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Schedify.Models;

namespace Schedify.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Image> Images { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventBooking> EventBookings { get; set; }
    public DbSet<EventResource> EventResources { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<BillingAddress> BillingAddresses { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationUser> ConversationUsers { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<ResourceVenue> ResourceVenues { get; set; }
    public DbSet<ResourceEquipment> ResourceEquipments { get; set; }
    public DbSet<ResourceFurniture> ResourceFurnitures { get; set; }
    public DbSet<ResourceCatering> ResourceCaterings { get; set; }
    public DbSet<ResourcePersonnel> ResourcePersonnels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // Configure EventBookings -> Events relationship to restrict cascading delete.
        modelBuilder.Entity<EventBooking>()
            .HasOne(e => e.Event)
            .WithMany(e => e.EventBookings)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure EventResources -> Events relationship to restrict cascading delete.
        modelBuilder.Entity<EventResource>()
            .HasOne(e => e.Event)
            .WithMany(e => e.EventResources)
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

        // Configure Images -> Resources relationship to restrict cascading delete.
        modelBuilder.Entity<Image>()
            .HasOne(e => e.Resource)
            .WithOne(e => e.Image)
            .HasForeignKey<Image>(e => e.ResourceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Images -> Resources relationship to restrict cascading delete.
        modelBuilder.Entity<Image>()
            .HasOne(e => e.User)
            .WithOne(e => e.Image)
            .HasForeignKey<Image>(e => e.UserId)
            .IsRequired(false);

        // Configure ResourceVenues -> Events relationship to restrict cascading delete.
        modelBuilder.Entity<ResourceVenue>()
            .HasOne(e => e.Resource)
            .WithOne(e => e.ResourceVenue)
            .HasForeignKey<ResourceVenue>(e => e.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure ResourceEquipments -> Events relationship to restrict cascading delete.
        modelBuilder.Entity<ResourceEquipment>()
            .HasOne(e => e.Resource)
            .WithOne(e => e.ResourceEquipment)
            .HasForeignKey<ResourceEquipment>(e => e.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure ResourceFurnitures -> Events relationship to restrict cascading delete.
        modelBuilder.Entity<ResourceFurniture>()
            .HasOne(e => e.Resource)
            .WithOne(e => e.ResourceFurniture)
            .HasForeignKey<ResourceFurniture>(e => e.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure ResourceCaterings -> Events relationship to restrict cascading delete.
        modelBuilder.Entity<ResourceCatering>()
            .HasOne(e => e.Resource)
            .WithOne(e => e.ResourceCatering)
            .HasForeignKey<ResourceCatering>(e => e.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure ResourcePersonnels -> Events relationship to restrict cascading delete.
        modelBuilder.Entity<ResourcePersonnel>()
            .HasOne(e => e.Resource)
            .WithOne(e => e.ResourcePersonnel)
            .HasForeignKey<ResourcePersonnel>(e => e.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        // CreatedAt (ActivityLogs)
        modelBuilder.Entity<ActivityLog>()
            .Property(u => u.LoggedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // CreatedAt (AspNetUsers)
        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // UpdatedAt (AspNetUsers)
        modelBuilder.Entity<User>()
            .Property(u => u.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // CreatedAt (BillingAddresses)
        modelBuilder.Entity<BillingAddress>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // UpdatedAt (BillingAddresses)
        modelBuilder.Entity<BillingAddress>()
            .Property(u => u.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // CreatedAt (Conversations)
        modelBuilder.Entity<Conversation>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // UpdatedAt (Conversations)
        modelBuilder.Entity<Conversation>()
            .Property(u => u.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // CreatedAt (ConversationUsers)
        modelBuilder.Entity<ConversationUser>()
            .Property(u => u.JoinedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // CreatedAt (EventBookings)
        modelBuilder.Entity<EventBooking>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // UpdatedAt (EventBookings)
        modelBuilder.Entity<EventBooking>()
            .Property(u => u.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // CreatedAt (EventResources)
        modelBuilder.Entity<EventResource>()
            .Property(u => u.AddedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // CreatedAt (Events)
        modelBuilder.Entity<Event>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // UpdatedAt (Events)
        modelBuilder.Entity<Event>()
            .Property(u => u.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // CreatedAt (Feedbacks)
        modelBuilder.Entity<Feedback>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // UpdatedAt (Feedbacks)
        modelBuilder.Entity<Feedback>()
            .Property(u => u.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // CreatedAt (Messages)
        modelBuilder.Entity<Message>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // UpdatedAt (Messages)
        modelBuilder.Entity<Message>()
            .Property(u => u.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // CreatedAt (Resources)
        modelBuilder.Entity<Resource>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // UpdatedAt (Resources)
        modelBuilder.Entity<Resource>()
            .Property(u => u.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Enums (Event)
        modelBuilder.Entity<Event>()
            .Property(e => e.Status)
            .HasConversion(
                v => v.ToString(),
                v => (EventStatus)Enum.Parse(typeof(EventStatus), v))
            .HasMaxLength(15);

        // Enums (EventBooking)
        modelBuilder.Entity<EventBooking>()
            .Property(e => e.Status)
            .HasConversion(
                v => v.ToString(),
                v => (BookingStatus)Enum.Parse(typeof(BookingStatus), v))
            .HasMaxLength(15);

        // Enums (Message)
        modelBuilder.Entity<Message>()
            .Property(e => e.Status)
            .HasConversion(
                v => v.ToString(),
                v => (MessageStatus)Enum.Parse(typeof(MessageStatus), v))
            .HasMaxLength(15);

        // Enums (Resource)
        modelBuilder.Entity<Resource>()
            .Property(e => e.ResourceType)
            .HasConversion(
                v => v.ToString(),
                v => (ResourceType)Enum.Parse(typeof(ResourceType), v))
            .HasMaxLength(15);

        // Enums (ResourceFurniture)
        modelBuilder.Entity<ResourceFurniture>()
            .Property(e => e.Material)
            .HasConversion(
                v => v.ToString(),
                v => (FurnitureMaterial)Enum.Parse(typeof(FurnitureMaterial), v))
            .HasMaxLength(15);

        // Enums (ActivityLog)
        modelBuilder.Entity<ActivityLog>()
           .Property(e => e.Type)
           .HasConversion(
               v => v.ToString(),
               v => (ActivityLogType)Enum.Parse(typeof(ActivityLogType), v))
           .HasMaxLength(15);



    }
}
