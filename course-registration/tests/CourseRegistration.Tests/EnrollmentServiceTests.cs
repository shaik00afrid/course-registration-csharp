using CourseRegistration;
using Xunit;

namespace CourseRegistration.Tests;

public sealed class EnrollmentServiceTests
{
    private static EnrollmentService CreateService(int capacity, params string[] eligibleLearners)
    {
        var service = new EnrollmentService(capacity);
        foreach (var learner in eligibleLearners)
            service.SetEligibility(learner, true);
        return service;
    }

    [Fact]
    public void Enroll_WithAvailableCapacity_EnrollsEligibleLearner()
    {
        var service = CreateService(2, "A");

        var result = service.Enroll("A");

        Assert.Equal(EnrollmentStatus.Enrolled, result.Status);
        Assert.Contains("A", service.GetSnapshot().EnrolledLearnerIds);
    }

    [Fact]
    public void Enroll_WhenFull_AddsEligibleLearnerToWaitlist()
    {
        var service = CreateService(1, "A", "B");
        service.Enroll("A");

        var result = service.Enroll("B");

        Assert.Equal(EnrollmentStatus.Waitlisted, result.Status);
        Assert.Equal(new[] { "B" }, service.GetSnapshot().WaitlistedLearnerIds);
        Assert.Single(service.GetSnapshot().EnrolledLearnerIds);
    }

    [Fact]
    public void Enroll_DuplicateRequests_DoNotDuplicateRecords()
    {
        var service = CreateService(1, "A", "B");
        Assert.Equal(EnrollmentStatus.Enrolled, service.Enroll("A").Status);
        Assert.Equal(EnrollmentStatus.AlreadyEnrolled, service.Enroll("A").Status);

        Assert.Equal(EnrollmentStatus.Waitlisted, service.Enroll("B").Status);
        Assert.Equal(EnrollmentStatus.AlreadyWaitlisted, service.Enroll("B").Status);

        var snapshot = service.GetSnapshot();
        Assert.Single(snapshot.EnrolledLearnerIds);
        Assert.Single(snapshot.WaitlistedLearnerIds);
    }

    [Fact]
    public void Drop_PromotesEarliestEligibleWaitlistedLearner()
    {
        var service = CreateService(1, "A", "B", "C");
        service.Enroll("A");
        service.Enroll("B");
        service.Enroll("C");

        var result = service.Drop("A");

        Assert.Equal(DropStatus.Dropped, result.Status);
        Assert.Equal("B", result.PromotedLearnerId);
        Assert.Equal(new[] { "B" }, service.GetSnapshot().EnrolledLearnerIds);
        Assert.Equal(new[] { "C" }, service.GetSnapshot().WaitlistedLearnerIds);
    }

    [Fact]
    public void Enroll_WhenPrerequisiteNotMet_DoesNotChangeState()
    {
        var service = CreateService(1, "A");
        service.SetEligibility("X", false);

        var result = service.Enroll("X");

        Assert.Equal(EnrollmentStatus.PrerequisiteNotMet, result.Status);
        Assert.Empty(service.GetSnapshot().EnrolledLearnerIds);
        Assert.Empty(service.GetSnapshot().WaitlistedLearnerIds);
    }

    [Fact]
    public void Drop_SkipsIneligibleLearnerAtFrontOfWaitlist()
    {
        var service = CreateService(1, "A", "B", "C");
        service.Enroll("A");
        service.Enroll("B");
        service.Enroll("C");
        service.SetEligibility("B", false);

        var result = service.Drop("A");
        var snapshot = service.GetSnapshot();

        Assert.Equal("C", result.PromotedLearnerId);
        Assert.Equal(new[] { "C" }, snapshot.EnrolledLearnerIds);
        Assert.Empty(snapshot.WaitlistedLearnerIds);
    }

    [Fact]
    public void Drop_InvalidOrRepeatedDrop_DoesNotChangeState()
    {
        var service = CreateService(1, "A");
        service.Enroll("A");

        var firstDrop = service.Drop("A");
        var afterFirstDrop = service.GetSnapshot();
        var repeatedDrop = service.Drop("A");
        var invalidDrop = service.Drop(" ");

        Assert.Equal(DropStatus.Dropped, firstDrop.Status);
        Assert.Equal(DropStatus.NotEnrolled, repeatedDrop.Status);
        Assert.Equal(DropStatus.InvalidLearnerId, invalidDrop.Status);
        Assert.Equal(afterFirstDrop, service.GetSnapshot());
    }

    [Fact]
    public void Constructor_RejectsNonPositiveCapacity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EnrollmentService(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EnrollmentService(-1));
    }

    [Fact]
    public async Task ConcurrentEnrollments_NeverExceedCapacity()
    {
        const int capacity = 5;
        const int learnerCount = 100;
        var service = new EnrollmentService(capacity);

        for (var i = 0; i < learnerCount; i++)
            service.SetEligibility($"L{i}", true);

        var tasks = Enumerable.Range(0, learnerCount)
            .Select(i => Task.Run(() => service.Enroll($"L{i}")));

        await Task.WhenAll(tasks);

        var snapshot = service.GetSnapshot();
        Assert.Equal(capacity, snapshot.EnrolledLearnerIds.Count);
        Assert.Equal(learnerCount - capacity, snapshot.WaitlistedLearnerIds.Count);
        Assert.Equal(learnerCount, snapshot.EnrolledLearnerIds.Count + snapshot.WaitlistedLearnerIds.Count);
    }

    [Fact]
    public void BlankLearnerId_IsRejected()
    {
        var service = new EnrollmentService(1);

        Assert.Equal(EnrollmentStatus.InvalidLearnerId, service.Enroll(" ").Status);
        Assert.Equal(DropStatus.InvalidLearnerId, service.Drop("").Status);
        Assert.Empty(service.GetSnapshot().EnrolledLearnerIds);
        Assert.Empty(service.GetSnapshot().WaitlistedLearnerIds);
    }
}
