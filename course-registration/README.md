# Course Registration — C# Assessment

A minimal .NET 8 console application implementing enrollment, drop, and FIFO waitlist promotion for one course offering. It uses an in-memory store and xUnit tests.

## Prerequisites

- .NET 8 SDK
- NuGet package restore access for the test dependencies

## Run the demonstration

From the repository root:

```bash
dotnet run --project src/CourseRegistration/CourseRegistration.csproj
```

The sample enrolls two learners, puts a third on the waitlist, drops one enrolled learner, and demonstrates promotion.

## Run tests

```bash
dotnet restore CourseRegistration.sln
dotnet build CourseRegistration.sln
dotnet test CourseRegistration.sln
```

## Key behavior

- Only learners whose prerequisite eligibility is set to `true` can enroll or waitlist.
- Missing eligibility is treated as ineligible.
- Duplicate enrollment and waitlist requests return explicit statuses without adding records.
- A drop promotes the first currently eligible waitlisted learner.
- Ineligible learners encountered during promotion are removed from the active waitlist and skipped.
- State-changing operations are protected by a single lock.

## Deliverables

- `docs/design-spec.md` — service boundary, data design, assumptions, invariants, concurrency and tradeoffs.
- `docs/ai-use-record.md` — prompts, AI contribution decisions, and verification steps.
- `docs/limitations.md` — limitations and follow-up risks.
- `tests/CourseRegistration.Tests/EnrollmentServiceTests.cs` — focused automated tests.

## Verification status

The authoring environment used to assemble these files did not have the .NET SDK installed. The commands above must be run in a .NET 8 environment before submission. Do not claim a successful build/test until the actual output confirms it.

## Scorecard

The assessment prompt references `Course-Registration-Rubric-Score.md`, but that file was not supplied. This project therefore does not fabricate its rubric or score. Use the supplied scorecard when available and attach evidence from the actual test/build run.
