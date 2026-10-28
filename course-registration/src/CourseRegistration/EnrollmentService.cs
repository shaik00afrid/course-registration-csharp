
namespace CourseRegistration;

/// <summary>
/// Thread-safe in-memory enrollment service for one course offering.
/// </summary>
public sealed class EnrollmentService
{
    private readonly object _gate = new();
    private readonly int _capacity;

    private readonly HashSet<string> _enrolled =
        new(StringComparer.Ordinal);

    private readonly Queue<string> _waitlist = new();

    private readonly HashSet<string> _waitlisted =
        new(StringComparer.Ordinal);

    private readonly Dictionary<string, bool> _eligibility =
        new(StringComparer.Ordinal);

    public EnrollmentService(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                "Capacity must be greater than zero.");

        _capacity = capacity;
    }

    public void SetEligibility(
        string learnerId,
        bool meetsPrerequisites)
    {
        if (string.IsNullOrWhiteSpace(learnerId))
            throw new ArgumentException(
                "Learner ID cannot be empty.",
                nameof(learnerId));

        lock (_gate)
        {
            _eligibility[learnerId] = meetsPrerequisites;
        }
    }

    public EnrollmentResult Enroll(string learnerId)
    {
        if (string.IsNullOrWhiteSpace(learnerId))
            return new(
                EnrollmentStatus.InvalidLearnerId,
                "Learner ID is required.");

        lock (_gate)
        {
            if (_enrolled.Contains(learnerId))
                return new(
                    EnrollmentStatus.AlreadyEnrolled,
                    "Learner is already enrolled.");

            if (_waitlisted.Contains(learnerId))
                return new(
                    EnrollmentStatus.AlreadyWaitlisted,
                    "Learner is already on the waitlist.");

            if (!_eligibility.TryGetValue(
                    learnerId, out var eligible) || !eligible)
            {
                return new(
                    EnrollmentStatus.PrerequisiteNotMet,
                    "Learner does not satisfy the prerequisites.");
            }

            if (_enrolled.Count < _capacity)
            {
                _enrolled.Add(learnerId);

                return new(
                    EnrollmentStatus.Enrolled,
                    "Enrollment successful.");
            }

            _waitlist.Enqueue(learnerId);
            _waitlisted.Add(learnerId);

            return new(
                EnrollmentStatus.Waitlisted,
                "Course is full; learner added to the waitlist.");
        }
    }

    public DropResult Drop(string learnerId)
    {
        if (string.IsNullOrWhiteSpace(learnerId))
            return new(
                DropStatus.InvalidLearnerId,
                "Learner ID is required.");

        lock (_gate)
        {
            if (!_enrolled.Remove(learnerId))
                return new(
                    DropStatus.NotEnrolled,
                    "Learner is not currently enrolled.");

            var promotedLearnerId = PromoteNextEligible();

            return new(
                DropStatus.Dropped,
                promotedLearnerId is null
                    ? "Enrollment dropped; no eligible waitlisted learner was available."
                    : $"Enrollment dropped; learner '{promotedLearnerId}' was promoted.",
                promotedLearnerId);
        }
    }

    public OfferingSnapshot GetSnapshot()
    {
        lock (_gate)
        {
            return new OfferingSnapshot(
                _capacity,
                _enrolled.ToArray(),
                _waitlist.ToArray());
        }
    }

    private string? PromoteNextEligible()
    {
        while (_waitlist.Count > 0)
        {
            // Remove the earliest learner from the FIFO queue.
            var learnerId = _waitlist.Dequeue();

            // No longer on the active waitlist, whether promoted or skipped.
            _waitlisted.Remove(learnerId);

            // Skip learners who are no longer eligible.
            if (!_eligibility.TryGetValue(
                    learnerId, out var eligible) || !eligible)
            {
                continue;
            }

            // Promote the first eligible learner.
            _enrolled.Add(learnerId);

            return learnerId;
        }

        // No eligible waitlisted learner was found.
        return null;
    }
}
