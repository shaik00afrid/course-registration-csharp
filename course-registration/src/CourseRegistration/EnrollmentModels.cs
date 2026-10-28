namespace CourseRegistration;

public enum EnrollmentStatus
{
    Enrolled,
    Waitlisted,
    PrerequisiteNotMet,
    AlreadyEnrolled,
    AlreadyWaitlisted,
    InvalidLearnerId
}

public sealed record EnrollmentResult(
    EnrollmentStatus Status,
    string Message,
    string? PromotedLearnerId = null)
{
    public bool Succeeded => Status is EnrollmentStatus.Enrolled or EnrollmentStatus.Waitlisted;
}

public enum DropStatus
{
    Dropped,
    NotEnrolled,
    InvalidLearnerId
}

public sealed record DropResult(
    DropStatus Status,
    string Message,
    string? PromotedLearnerId = null)
{
    public bool Succeeded => Status == DropStatus.Dropped;
}

public sealed record OfferingSnapshot(
    int Capacity,
    IReadOnlyList<string> EnrolledLearnerIds,
    IReadOnlyList<string> WaitlistedLearnerIds);
