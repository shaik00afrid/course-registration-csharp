# AI Prompt and Use Record

## Purpose

AI was used as a development assistant to help design the course enrollment service, identify edge cases, review concurrency handling, and plan unit tests. Suggestions were reviewed and adapted to the requirements of the assessment.

## Prompt 1 — Design and Implementation

**Prompt used:**

"Help design a C# course enrollment service that supports enrolling learners, dropping enrollments, enforcing course capacity, and promoting learners from a FIFO waitlist. Identify important edge cases and suggest suitable data structures."

**Summary of relevant response:**

Use a service class to manage enrollment operations, explicit result statuses to represent outcomes, and data structures suited to membership checks and FIFO processing. Define clear rules for prerequisites, duplicate requests, capacity limits, and waitlist promotion.

**Decision:**

The service uses a `HashSet<string>` to track enrolled and waitlisted learners and a `Queue<string>` to maintain FIFO waitlist order. Learners must satisfy prerequisites before enrollment or waitlisting. Ineligible learners encountered during promotion are removed from the active waitlist, and processing continues to the next learner.

**Files/decisions informed:**

`EnrollmentService.cs`, `EnrollmentModels.cs`, and `docs/design-spec.md`.

**Verification:**

Review the enrollment and drop workflows against the defined rules. Run tests covering capacity, duplicate requests, prerequisite failures, and waitlist promotion.

## Prompt 2 — Unit Test Planning

**Prompt used:**

"Suggest xUnit test cases for a C# course enrollment service covering available capacity, full capacity, duplicate enrollment, prerequisite validation, FIFO waitlist promotion, invalid drops, and concurrent enrollment attempts."

**Summary of relevant response:**

Cover both successful and unsuccessful operations. Verify returned statuses and resulting state, and include a concurrency test to ensure the number of enrolled learners never exceeds the course capacity.

**Decision:**

The suggested scenarios were adapted to the service API and implemented result statuses. The test suite includes enrollment validation, waitlist behavior, drop operations, and concurrent enrollment scenarios.

**Files/decisions informed:**

`tests/CourseRegistration.Tests/EnrollmentServiceTests.cs`.

**Verification:**

Run `dotnet test CourseRegistration.sln` using an environment with the .NET 8 SDK and required package access. Test execution must be confirmed before reporting the tests as passing.

## Prompt 3 — Concurrency and Thread Safety Review

**Prompt used:**

"Review the enrollment service for race conditions involving course capacity, duplicate enrollment, waitlist updates, and learner promotion. Explain how synchronization can protect shared state."

**Summary of relevant response:**

Concurrent requests can cause inconsistent state if capacity checks and enrollment updates are performed independently. Related state changes should be protected by the same synchronization mechanism. A process-local lock does not coordinate multiple service instances or application processes.

**Decision:**

The implementation uses a private lock to protect shared state. Enrollment checks and updates are performed within the lock, and dropping a learner and promoting the next eligible waitlisted learner are handled within the same protected operation.

**Files/decisions informed:**

`EnrollmentService.cs`, `docs/design-spec.md`, and the concurrency tests.

**Verification:**

Review the locking boundaries and run the concurrency test. Confirm that enrollment capacity is maintained and that waitlist promotion does not introduce duplicate enrollment.

## Prompt 4 — Data Structure Review

**Prompt used:**

"Compare using `Queue<string>` and `LinkedList<string>` for a course waitlist where learners are processed in FIFO order. Recommend the simpler option for the current requirements."

**Summary of relevant response:**

A queue naturally supports FIFO processing through enqueue and dequeue operations. A linked list provides additional flexibility for inserting or removing arbitrary nodes, but that flexibility is unnecessary when the implementation only processes the front of the waitlist.

**Decision:**

`Queue<string>` was selected because it directly represents the required FIFO behavior and keeps the implementation simple.

**Files/decisions informed:**

`EnrollmentService.cs` and `docs/design-spec.md`.

**Verification:**

Run the waitlist tests to confirm that learners are promoted in the expected order and that ineligible learners are handled according to the documented policy.

## Independent Review and Evidence

Before submission, perform the following checks:

- Run `dotnet build CourseRegistration.sln`.
- Run `dotnet test CourseRegistration.sln`.
- Run `dotnet run --project src/CourseRegistration/CourseRegistration.csproj`.
- Review the final source code, tests, and documentation.
- Record actual build and test results only after executing the commands.
- Rerun relevant tests after making further code changes.

**Evidence status:** Build and test execution have not yet been confirmed. The solution should be considered verified only after the required commands complete successfully.

## Additional Notes

This document records the AI-assisted design and review activities relevant to the implementation. Any additional prompts or significant changes made during the assessment should be added to this record to maintain an accurate account of AI usage.