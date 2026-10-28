using CourseRegistration;

var service = new EnrollmentService(capacity: 5);
service.SetEligibility("learner-1", true);
service.SetEligibility("learner-2", true);
service.SetEligibility("learner-3", true);
service.SetEligibility("learner-4", true);
service.SetEligibility("learner-5", true);
service.SetEligibility("learner-6", false);
service.SetEligibility("learner-7", true);
service.SetEligibility("learner-8", true);
Console.WriteLine(service.Enroll("learner-1").Message);
Console.WriteLine(service.Enroll("learner-2").Message);
Console.WriteLine(service.Enroll("learner-3").Message);
Console.WriteLine(service.Enroll("learner-4").Message);
Console.WriteLine(service.Enroll("learner-5").Message);
Console.WriteLine(service.Enroll("learner-6").Message);
Console.WriteLine(service.Enroll("learner-7").Message);
Console.WriteLine(service.Enroll("learner-8").Message);
Console.WriteLine(service.Drop("learner-1").Message);
var snapshot = service.GetSnapshot();
Console.WriteLine($"Enrolled ({snapshot.EnrolledLearnerIds.Count}/{snapshot.Capacity}): " +
                  string.Join(", ", snapshot.EnrolledLearnerIds));
Console.WriteLine("Waitlist: " + string.Join(", ", snapshot.WaitlistedLearnerIds));
