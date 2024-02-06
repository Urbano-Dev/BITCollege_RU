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
        [ForeignKey(nameof(Grade))]
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

        // 
        public void ChangeState() {
            GradePointState currentState = dbContext.GradePointStates
                .Where(gradeState => gradeState.GradePointStateId == this.GradePointStateId)
                .SingleOrDefault();

            currentState.StateChangedCheck(this);

            while (true) {
                GradePointState updateState = dbContext.GradePointStates
                    .Where(gradeState => gradeState.GradePointStateId == this.GradePointStateId)
                    .SingleOrDefault();

                if (currentState == updateState)
                    break;

                currentState = updateState;
                currentState.StateChangedCheck(this);
            }

            dbContext.SaveChanges();
        }   

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

        //
        public double TuitionRateAdjustment(Student student) { return 0; }

        //
        public void StateChangedCheck(Student student) { }

        // navigation properties - represents 0 - many cardinality.
        public virtual ICollection<Student> Students { get; set; }
    }


    /// <summary>
    /// SuspendedState model a member of abstract GradePointState.
    /// </summary>
    public class SuspendedState : GradePointState
    {
        private static SuspendedState suspendedState = new SuspendedState();

        private SuspendedState() {
            LowerLimit = 0.00;
            UpperLimit = 1.00;
            TuitionFactor = 1.1;
        }

        public static SuspendedState GetInstance() { 
            if (suspendedState == null) {
                suspendedState = dbContext.SuspendedStates.SingleOrDefault();

                if (suspendedState == null) {
                    suspendedState = new SuspendedState();

                    dbContext.GradePointStates.Add(suspendedState);
                    dbContext.SaveChanges();
                }
            }

            return suspendedState; 
        }

        public double TuitionRateAdjustment(Student student) { return TuitionFactor; }

        public void StateChangeCheck(Student student) { }
    }

    /// <summary>
    /// ProbationdState model a member of abstract GradePointState.
    /// </summary>
    public class ProbationState : GradePointState
    {
        private static ProbationState probationState = new ProbationState();

        private ProbationState() {
            LowerLimit = 1.00;
            UpperLimit = 2.00;
            TuitionFactor = 1.075;
        }

        public static ProbationState GetInstance() { 
            if (probationState == null) {
                probationState = dbContext.ProbationStates.SingleOrDefault();

                if (probationState == null) {
                    probationState = new ProbationState();

                    dbContext.GradePointStates.Add(probationState);
                    dbContext.SaveChanges();
                }
            }
            return probationState; 
        }

        public double TuitionRateAdjustment(Student student) { return TuitionFactor; }

        public void StateChangeCheck(Student student) { }
    }

    /// <summary>
    /// RegularState model a member of abstract GradePointState.
    /// </summary>
    public class RegularState : GradePointState
    {
        private static RegularState regularState = new RegularState();

        private RegularState() {
            LowerLimit = 2.00;
            UpperLimit = 3.70;
            TuitionFactor = 1.0;
        }

        public static RegularState GetInstance() { 
            if (regularState == null) {
                regularState = dbContext.RegularStates.SingleOrDefault();

                if (regularState == null) {
                    regularState = new RegularState();

                    dbContext.GradePointStates.Add(regularState);
                    dbContext.SaveChanges();
                }
            }
            return regularState; 
        }

        public double TuitionRateAdjustment(Student student) { return TuitionFactor; }

        public void StateChangeCheck(Student student) { }

    }

    /// <summary>
    /// HonoursState model a member of abstract GradePointState.
    /// </summary>
    public class HonoursState : GradePointState
    {
        private static HonoursState honoursState = new HonoursState();

        private HonoursState() {
            LowerLimit = 3.70;
            UpperLimit = 4.50;
            TuitionFactor = 0.9;
        }

        public static HonoursState GetInstance() { 
            if (honoursState == null) {
                honoursState = dbContext.HonoursStates.SingleOrDefault();

                if (honoursState == null) {
                    honoursState = new HonoursState();

                    dbContext.GradePointStates.Add(honoursState);
                    dbContext.SaveChanges();
                }
            }
            return honoursState; 
        }

        public double TuitionRateAdjustment(Student student) { return TuitionFactor; }

        public void StateChangeCheck(Student student) { }
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
    }

    /// <summary>
    /// AuditCourse model a member of abstract Course.
    /// </summary>
    public class AuditCourse : Course
    {
        
    }

    /// <summary>
    /// MasteryCourse model a member of abstract Course.
    /// </summary>
    public class MasteryCourse : Course
    {
        [Required]
        [Display(Name ="Maximum\nAttempts")]
        public int MaximumAttempts { get; set; }
    }
}