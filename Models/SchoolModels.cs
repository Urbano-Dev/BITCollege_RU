using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;
using System.Web.Mvc;
using Utility;
using System.Data.Entity;
using BITCollege_RU.Data;

namespace BITCollege_RU.Models
{
    /// <summary>
    /// Registration model represents the Registration table.
    /// </summary>
    public class Registration
    {
        // Primary Key annotation.
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int RegistrationId { get; set; }

        [Required]
        // Foreign Key annotation reference Student model.
        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [Required]
        // Foreign Key annotation reference Course model.
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        [Required]
        [Display(Name ="Registration\nNumber")]
        public long RegistraionNumber { get; set; }

        [Required]
        [Display(Name ="Date")]
        // Display into a date format annotation.
        [DisplayFormat(DataFormatString ="{0:d}")]
        public DateTime RegistrationDate { get; set; }

        [DisplayFormat(NullDisplayText ="Ungraded")]
        [Range(0,1)]
        public double? Grade { get; set; }

        public string Notes { get; set; }

        //
        public void SetNextRegistrationNumber() { }

        // navigation properties - represents 1 cardinality.
        public virtual Student Student { get; set; }
        public virtual Course Course { get; set; }
    }

    /// <summary>
    /// AcademicProgram model represents the AcademicProgarm table.
    /// </summary>
    public class AcademicProgram
    {
        // Primary Key annotation.
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int AcademicProgramId { get; set; }

        [Required]
        [Display(Name ="Program")]
        public string ProgramAcronym { get; set; }

        [Required]
        [Display(Name = "Program\nName")]
        public string Description { get; set; }

        public virtual ICollection<Course> Course { get; set; }
        public virtual ICollection<Student> Students { get; set; }
    }

    /// <summary>
    /// Student model represents the Student table.
    /// </summary>
    public class Student
    {
        private BITCollege_RUContext dbContext = new BITCollege_RUContext();
        // Primary Key annotation
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int StudentId { get; set; }

        [Required]
        // Foreign Key annotation reference GradePointState Model.
        [ForeignKey("Grade")]
        public int GradePointStateId { get; set; }

        // Foreign Key annotation reference AcademicProgram Model.
        [ForeignKey("AcademicProgram")]
        public int? AcademicProgramId { get; set; }

        [Required]
        [Range(10000000, 99999999)] 
        [Display(Name = "Student\nNumber")]
        public long StudentNumber { get; set; }

        [Required]
        [Display(Name = "First\nName")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last\nName")]
        public string LastName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        // Regular Expression for Canadian Provinces with Error Message.
        [RegularExpression("^(N[BLSTU]|[AMN]B|[BQ]C|ON|PE|SK|YT)", ErrorMessage = "Please enter a valid Canadian Province Code.")]
        public string Province { get; set; }

        [Required]
        [Display(Name = "Date")]
        // Format date annotation.
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Grade\nPoint\nAverage")]
        // Format in 2 decimal place annotation.
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Range(0, 4.5)]
        public double? GradePointAverage { get; set; }

        [Required]
        [Display(Name = "Fees")]
        // Format in currency and 2 decimal place annotation.
        [DisplayFormat(DataFormatString ="{0:c2}")]
        public double OutstandingFees { get; set; }
        public string Notes { get; set; }
        [Display(Name = "Name")]
        public string FullName { get { return String.Format("{0} {1}", FirstName, LastName); } }
        [Display(Name = "Address")]
        public string FullAddress { get { return String.Format("{0} {1}, {2}", Address, City, Province); } }

        // Will get the students associated state.
        public void ChangeState() { 
            while (true) {
                // using the data context (dbContext) base on Students GradePointsStateId
                GradePointState currentState = dbContext.GradePointStates
                .SingleOrDefault(gradeState => gradeState.GradePointStateId == this.GradePointStateId);

                currentState.StateChangeCheck(this);

                // break if correct state acquired. 
                if (currentState.GradePointStateId == this.GradePointStateId) 
                    break;
            }
        }   

        //
        public void SetNextStudentNumber() { }

        // navigation properties - represents 1 or 0 - 1 cardinality.
        public virtual GradePointState Grade { get; set; }
        public virtual AcademicProgram AcademicProgram { get; set; }
        public virtual ICollection<Registration> Registration { get; set; }
    }

    /// <summary>
    /// GradePointState abstract model represents the GradePointState table.
    /// </summary>
    public abstract class GradePointState
    {
        protected static BITCollege_RUContext dbContext = new BITCollege_RUContext();
       
        // Primary Key annotation and Key annotation.
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int GradePointStateId { get; set; }

        [Required]
        [Display(Name ="Lower\nLimit")]
        // Format in 2 decimal place annotation.
        [DisplayFormat(DataFormatString ="{0:N2}")]
        public double LowerLimit { get; set; }

        [Required]
        [Display(Name = "Upper\nLimit")]
        // Format in 2 decimal place annotation.
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double UpperLimit { get; set; }

        [Required]
        [Display(Name = "Tuition\nRate\nFactor")]
        // Format in 2 decimal place annotation.
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double TuitionFactor { get; set; }

        [Display(Name ="State")]
        // Initialize Extract.State static class in utilities.
        public string Description { get { return Extract.State(GetType().Name); } }

        // Abstract state.
        public abstract double TuitionRateAdjustment(Student student);
        public abstract void StateChangeCheck(Student student);

        // navigation properties - represents 0 - many cardinality.
        public virtual ICollection<Student> Students { get; set; }
    }


    /// <summary>
    /// SuspendedState model a member of abstract GradePointState.
    /// </summary>
    public class SuspendedState : GradePointState
    {
        // Singleton instance of SuspendedState, ensures only one instance is created
        private static SuspendedState _suspendedState;

        // Private constructor for SuspendedState, sets default properties
        private SuspendedState() {
            LowerLimit = 0.00;
            UpperLimit = 1.00;
            TuitionFactor = 1.1;
        }

        // Singleton pattern implementation to get the instance of SuspendedState
        public static SuspendedState GetInstance {
            get {
                if (_suspendedState == null) {
                    _suspendedState = dbContext.SuspendedStates.SingleOrDefault();

                    if (_suspendedState == null) {
                        _suspendedState = new SuspendedState();
                        dbContext.GradePointStates.Add(_suspendedState);
                    }
                }
                return _suspendedState;
            }
        }

        public override double TuitionRateAdjustment(Student student) {
            return TuitionFactor;
        }

        // Overrides the StateChangeCheck method from the base class GradePointState
        // Checks if the student's GradePointAverage exceeds the UpperLimit. Then assign GetInstace GradePointStateId.
        // Persists the changes to the database
        public override void StateChangeCheck(Student student) {
            if (student.GradePointAverage > UpperLimit) {
                student.GradePointStateId = ProbationState.GetInstance.GradePointStateId;
            }

            dbContext.SaveChanges();
        }
    }

    /// <summary>
    /// ProbationdState model a member of abstract GradePointState.
    /// </summary>
    public class ProbationState : GradePointState
    {
        // Singleton instance of ProbationState, ensures only one instance is created.
        private static ProbationState _probationState;

        // Private constructor for ProbationState, sets default properties.
        private ProbationState() {
            LowerLimit = 1.00;
            UpperLimit = 2.00;
            TuitionFactor = 1.075;
        }

        // Singleton pattern implementation to get the instance of ProbationState.
        public static ProbationState GetInstance {
            get {
                if (_probationState == null) {
                    _probationState = dbContext.ProbationStates.SingleOrDefault();

                    if (_probationState == null) {
                        _probationState = new ProbationState();

                        dbContext.GradePointStates.Add(_probationState);
                    }
                }
                return _probationState;
            }
        }

        public override double TuitionRateAdjustment(Student student) { return TuitionFactor; }

        // Overrides the StateChangeCheck method from the base class GradePointState
        // Checks if the student's GradePointAverage exceeds the UpperLimit. Then assign GetInstace GradePointStateId.
        // Persists the changes to the database
        public override void StateChangeCheck(Student student) { 
            if (student.GradePointAverage > UpperLimit) {
                student.GradePointStateId = RegularState.GetInstance.GradePointStateId;
            }
            else if (student.GradePointAverage < LowerLimit) {
                student.GradePointStateId = SuspendedState.GetInstance.GradePointStateId;
            }
            dbContext.SaveChanges();
        }
    }

    /// <summary>
    /// RegularState model a member of abstract GradePointState.
    /// </summary>
    public class RegularState : GradePointState
    {
        // Singleton instance of RegularState, ensures only one instance is created
        private static RegularState _regularState;

        // Private constructor for RegularState, sets default properties
        private RegularState() {
            LowerLimit = 2.00;
            UpperLimit = 3.70;
            TuitionFactor = 1.0;
        }

        // Singleton pattern implementation to get the instance of RegularState
        public static RegularState GetInstance {
            get {
                if (_regularState == null) {
                    _regularState = dbContext.RegularStates.SingleOrDefault();

                    if (_regularState == null) {
                        _regularState = new RegularState();

                        dbContext.GradePointStates.Add(_regularState);
                    }
                }
                return _regularState;
            }
        }

        public override double TuitionRateAdjustment(Student student) { return TuitionFactor; }

        // Overrides the StateChangeCheck method from the base class GradePointState
        // Checks if the student's GradePointAverage exceeds the UpperLimit. Then assign GetInstace GradePointStateId.
        // Persists the changes to the database
        public override void StateChangeCheck(Student student) { 
            if (student.GradePointAverage > UpperLimit) {
                student.GradePointStateId = HonoursState.GetInstance.GradePointStateId;    
            } else if (student.GradePointAverage < LowerLimit) {
                student.GradePointStateId = ProbationState.GetInstance.GradePointStateId;
            }
            dbContext.SaveChanges();
        }

    }

    /// <summary>
    /// HonoursState model a member of abstract GradePointState.
    /// </summary>
    public class HonoursState : GradePointState
    {
        // Singleton instance of HonoursState, ensures only one instance is created.
        private static HonoursState _honoursState;

        // Private constructor for HonoursState, sets default properties
        private HonoursState() {
            LowerLimit = 3.70;
            UpperLimit = 4.50;
            TuitionFactor = 0.9;
        }

        // Singleton pattern implementation to get the instance of HonoursState
        public static HonoursState GetInstance {
            get {
                if (_honoursState == null) {
                    _honoursState = dbContext.HonoursStates.SingleOrDefault();

                    if (_honoursState == null) {
                        _honoursState = new HonoursState();

                        dbContext.GradePointStates.Add(_honoursState);
                    }
                }
                return _honoursState;
            }
        }
        
        public override double TuitionRateAdjustment(Student student) { return TuitionFactor; }

        // Overrides the StateChangeCheck method from the base class GradePointState
        // Checks if the student's GradePointAverage exceeds the UpperLimit. Then assign GetInstace GradePointStateId.
        // Persists the changes to the database
        public override void StateChangeCheck(Student student) { 
            if (student.GradePointAverage < LowerLimit) {
                student.GradePointStateId = RegularState.GetInstance.GradePointStateId;
            }

            dbContext.SaveChanges();
        }
    }

    /// <summary>
    /// Course abstract model represents the Course table.
    /// </summary>
    public abstract class Course
    {
        // Primary Key and Key annotation.
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CourseId { get; set; }

        // Foreign Key annotation reference AcademicProgram.
        [ForeignKey("AcademicProgram")]
        public int? AcademicProgramId { get; set; }

        [Required]
        [Display(Name ="Course\nNumber")]
        public string CourseNumber { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        // Format in 2 decimal place annotation.
        [DisplayFormat(DataFormatString ="{0:N2}")]
        [Display(Name ="Credit\nHours")]
        public double CreditHours { get; set; }

        [Required]
        // Format in 2 decimal place annotation.
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Display(Name = "Tuition")]
        public double TuitionAmount { get; set; }

        [Display(Name ="Course\nType")]
        public string CourseType { get { return Extract.State(GetType().Name); } }

        public string Notes { get; set; }

        public abstract void SetNextCourseNumber();

        // navigation property - represents 0 to many relationship.
        public ICollection<AcademicProgram> AcademicProgram { get; set; }
        public ICollection<Registration> Registration { get; set; }
    }

    /// <summary>
    /// GradedCourse model a member of abstract Course.
    /// </summary>
    public class GradedCourse : Course
    {
        [Required]
        // Format in 2 decimal place annotation.
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Display(Name = "Assignments")]
        public double AssignmentWeight { get; set; }

        [Required]
        // Format in 2 decimal place annotation.
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Display(Name = "Exams")]
        public double ExamWeight { get; set; }

        //
        public override void SetNextCourseNumber()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// AuditCourse model a member of abstract Course.
    /// </summary>
    public class AuditCourse : Course
    {
        //
        public override void SetNextCourseNumber()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// MasteryCourse model a member of abstract Course.
    /// </summary>
    public class MasteryCourse : Course
    {
        [Required]
        [Display(Name ="Maximum\nAttempts")]
        public int MaximumAttempts { get; set; }

        //
        public override void SetNextCourseNumber()
        {
            throw new NotImplementedException();
        }
    }
}

public abstract class NextUniqueNumber 
{
    [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
    public int NextUniqueNumberId { get; set; }

    public long NextAvailableNumber { get; set; }
}

public class NextStudent : NextUniqueNumber
{
    private static NextStudent _nextStudent;

    private NextStudent() { }

    public static NextStudent GetInstance()
    {
        return _nextStudent;
    }
}

public class NextRegistration : NextUniqueNumber
{
    private static NextRegistration _nextRegistration;

    private NextRegistration() { }

    public NextRegistration GetInstance()
    {
        return _nextRegistration;
    }
}

public class NextGradeCourse : NextUniqueNumber
{
    private static NextGradeCourse _nextGradeCourse;

    private NextGradeCourse() { }

    public NextGradeCourse GetInstance()
    {
        return _nextGradeCourse;
    }
}

public class NextAuditCourse : NextUniqueNumber
{
    private static NextAuditCourse _nextAuditCourse;

    private NextAuditCourse() { }

    public NextAuditCourse GetInstance()
    {
        return _nextAuditCourse;
    }
}

public class NextMasteryCourse : NextUniqueNumber
{
    private NextMasteryCourse _nextMasteryCourse;

    private NextMasteryCourse() { }

    public NextMasteryCourse GetInstance()
    {
        return _nextMasteryCourse;
    }
}

public class StoredProcedure
{
    public static long? NextUniqueNumber(string discriminator)
    {
        return 0;
    }
}