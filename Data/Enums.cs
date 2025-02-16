// User
public enum UserRoles
{
    Admin,
    Organizer,
    Attendee
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
    Venue_Space,
    Equipment_Hardware,
    Furniture_Fixtures,
    Catering_Food,
    Personnel_Staffing,
    Emergency_Medical
}

// ActivityLog
public enum ActivityLogType
{
    Created,
    Updated,
    Deleted,
}