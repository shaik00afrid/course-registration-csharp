    # Limitations and Follow-up Risks

    - This is an in-memory, single-process prototype. State is lost on restart and a lock cannot coordinate multiple application instances.
    - Prerequisite eligibility is a Boolean test seam, not a real prerequisite evaluator.
    - The service models one offering only; there is no catalog, identity, authorization, persistence, frontend, or HTTP API.
    - Waitlist cancellation, audit history, notifications, and operational observability are out of scope.
    - Learner IDs are case-sensitive and compared ordinally.
    - The concurrency test validates the in-process capacity invariant; it does not prove distributed consistency.
    - The project has not been built or tested in the authoring environment because the .NET SDK is not installed there. Run the documented commands locally and update this note with the actual result before submission.
