using System;

namespace TestSchool.ViewModels
{
    public class AssignedStudentData
    {
        public bool Assigned { get; set; }
        public bool? StudentAcnive { get; set; }
        public DateTime? StudentDate_of_birthe { get; set; }
        public string StudentFirstName { get; set; }
        public int StudentID { get; set; }
        public string StudentLastName { get; set; }
    }
}