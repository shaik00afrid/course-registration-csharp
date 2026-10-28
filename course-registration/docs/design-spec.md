# Design Specification: Course Enrollment and Waitlist

## 1. Scope and Service Boundary

This prototype implements enrollment, dropping, and waitlist promotion for a single course offering.

`EnrollmentService` manages the in-memory state and exposes the following operations:

- `Enroll`: enrolls an eligible learner or adds them to the waitlist when capacity is full.
- `Drop`: removes an enrolled learner and promotes the next eligible learner, if available.
- `SetEligibility`: records whether a learner meets the prerequisites.
- `GetSnapshot`: returns a snapshot of the current enrollment and waitlist state.

The console application demonstrates the service behavior. It is not intended to be a production API.

## 2. Data and API Design

- `EnrollmentService(int capacity)`: creates a course offering with a fixed, positive capacity.
- `SetEligibility(learnerId, meetsPrerequisites)`: records the learner's prerequisite status.
- `Enroll(learnerId)`: returns an `EnrollmentResult` containing a status and message.
- `Drop(learnerId)`: returns a `DropResult` containing a status, message, and optional promoted learner ID.
- `GetSnapshot()`: returns an `OfferingSnapshot` containing capacity, enrolled learner IDs, and waitlisted learner IDs.

### Data structures

- `HashSet<string> _enrolled`: tracks enrolled learners and supports efficient membership checks.
- `Queue<string> _waitlist`: maintains FIFO order for waitlisted learners.
- `HashSet<string> _waitlisted`: prevents duplicate waitlist entries.
- `Dictionary<string, bool> _eligibility`: stores each learner's current prerequisite eligibility.
- `object _gate`: synchronizes access to shared state.

The queue is appropriate because the service processes waitlisted learners in arrival order.

## 3. Assumptions and Policies

1. Course capacity is a positive integer and remains fixed for the lifetime of the service.
2. Learner IDs must be non-empty. Comparisons are ordinal and case-sensitive.
3. Prerequisite eligibility is represented by a Boolean supplied by the caller. A missing eligibility record is treated as ineligible.
4. Only eligible learners can enroll or join the waitlist.
5. Duplicate enrollment requests return `AlreadyEnrolled` or `AlreadyWaitlisted` without modifying state.
6. When an enrolled learner drops the course, the service processes the waitlist in FIFO order. Each examined learner is removed from the active queue. The first eligible learner is promoted; ineligible learners are skipped and removed from the active waitlist.
7. Dropping a learner who is not enrolled returns `NotEnrolled` and does not change enrollment or waitlist state.
8. A learner cannot be both enrolled and waitlisted at the same time.
9. Learner eligibility can change through `SetEligibility`. Eligibility is checked when an enrollment request is processed and when a waitlisted learner is considered for promotion.

## 4. Invariants

The implementation should maintain these conditions:

- The enrolled learner count is never negative and never exceeds capacity.
- No learner appears more than once in the enrolled collection or active waitlist.
- Enrolled and waitlisted learner membership remains disjoint.
- A successful promotion fills the seat released by a drop.
- If no eligible waitlisted learner exists, the released seat remains available.
- Invalid requests and rejected duplicate requests do not mutate enrollment or waitlist state.
- All state transitions preserve the capacity limit.

## 5. Concurrency and Thread Safety

A single private lock protects shared state accessed by `Enroll`, `Drop`, `SetEligibility`, and `GetSnapshot`.

Enrollment validation and state updates occur inside the critical section so that concurrent requests cannot both claim the same final seat. Dropping an enrolled learner and promoting the next eligible waitlisted learner occur within the same protected operation, preventing another request from taking the freed seat before promotion is completed.

`GetSnapshot` also acquires the lock so that the returned snapshot represents a consistent view of the service state.

The lock is shared across these methods; it is not a separate lock for each operation. This provides straightforward synchronization for the current in-memory prototype, but it serializes operations on the same service instance.

The concurrency test should verify that simultaneous enrollment requests never cause the enrolled count to exceed capacity.

## 6. Persistence and Multi-Instance Considerations

The lock provides synchronization only within a single service instance in one process. It does not coordinate multiple instances or application processes.

The current implementation stores state in memory, so enrollment and waitlist data are lost when the process exits.

A production implementation shared across multiple application instances would require persistent storage, transactional capacity enforcement, and appropriate database constraints or locking to prevent duplicate enrollment and capacity violations.

## 7. Tradeoffs and Known Limitations

- Enrollment and waitlist state are held in memory and are not durable.
- Prerequisite checks are simulated rather than integrated with an authoritative course or prerequisite system.
- There is no waitlist cancellation operation.
- There is no audit history, authentication, authorization, or production API layer.
- The service supports only one course offering.
- Eligibility is represented by a Boolean rather than a configurable policy engine.
- The lock serializes operations on a single service instance and may limit throughput under heavy contention.
- The prototype does not provide cross-process coordination or distributed concurrency control.

## 8. Validation

Before submission, validate the implementation with:

- `dotnet build CourseRegistration.sln`
- `dotnet test CourseRegistration.sln`
- `dotnet run --project src/CourseRegistration/CourseRegistration.csproj`

Confirm that the tests cover capacity limits, duplicate requests, prerequisite validation, FIFO promotion, invalid drops, and concurrent enrollment.

Build and test success should be recorded only after the commands have actually completed successfully.