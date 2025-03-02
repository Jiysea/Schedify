// User
public enum UserRoles
{
    Admin,
    Organizer,
    Attendee,
    Provider
}

// Event
public enum EventStatus
{
    Draft,
    Open,
    Ongoing,
    Completed,
    Cancelled,
    Postponed
}

// Booking
public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Refunded
}

// Message
public enum MessageStatus
{
    Sent,
    Failed,
    Deleted
}

// Resource
public enum ResourceType
{
    Venue,
    Equipment,
    Furniture,
    Catering,
    Personnel
}

// ActivityLog
public enum ActivityLogType
{
    Created,
    Updated,
    Deleted,
}

// Resource Cost Type
public enum ResourceCostType
{
    PerHour, // Venues, Personnels
    PerDay, // Venues, Personnels
    PerHead, // Personnels
    FixedRate, // Venues, Personnels
    PerUnit, // Equipments, Furnitures
    PerServing, // Caterings
    InBulk, // Equipments, Furnitures, Caterings
}

public enum FurnitureMaterial
{
    Wood,
    Metal,
    Plastic,
    Glass,
    Fabric
}