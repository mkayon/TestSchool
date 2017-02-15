using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity.Infrastructure;
using TestSchool.Models;
using TestSchool.ViewModels;

namespace TestSchool.Controllers
{
    public class ClassesController : Controller
    {
        private testbaseEntities db = new testbaseEntities();

        // GET: Classes
        public async Task<ActionResult> Index()
        {
            return View(await db.Class.ToListAsync());
        }

        // GET: Classes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Class @class = await db.Class.FindAsync(id);
            PopulateAssignedStudentData(@class);
            if (@class == null)
            {
                return HttpNotFound();
            }
            return View(@class);
        }

        // GET: Classes/Create
        public ActionResult Create()
        {
            var _class = new Class();
            _class.Student = new List<Student>();
            PopulateAssignedStudentData(_class);
            return View(_class);
        }

        private void PopulateAssignedStudentData(Class _class)
        {
            var allStudent = db.Student;
            var classeStudent = new HashSet<int>(_class.Student.Select(c => c.Student_Id));
            var viewModel = new List<AssignedStudentData>();
            foreach (var student in allStudent)
            {
                viewModel.Add(new AssignedStudentData
                {
                    StudentID = student.Student_Id,
                    StudentFirstName = student.First_Name,
                    StudentLastName = student.Last_Name,
                    StudentDate_of_birthe = student.Date_of_birth,
                    StudentAcnive=student.Active,
                     Assigned = classeStudent.Contains(student.Student_Id)
                });
            }
            ViewBag.students = viewModel;
        }

        // POST: Classes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Classes_Id,Title,Description,Active,Start_Date,End_Date,Create_Date,Update_Date")] Class @class, string[] selectedStudents)
        {
            if (selectedStudents != null)
            {
                @class.Student = new List<Student>();
                foreach (var student in selectedStudents)
                {
                    var studentToAdd = db.Student.Find(int.Parse(student));
                    @class.Student.Add(studentToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                db.Class.Add(@class);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            PopulateAssignedStudentData(@class);
            return View(@class);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create1([Bind(Include = "Classes_Id,Title,Description,Active,Start_Date,End_Date,Create_Date,Update_Date")] Class @class)
        {
            if (ModelState.IsValid)
            {
                db.Class.Add(@class);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(@class);
        }

        // GET: Classes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           
           Class @class =  db.Class
                .Where(i => i.Classes_Id == id)
                .Single();
            PopulateAssignedStudentData(@class);
            if (@class == null)
            {
                return HttpNotFound();
            }
            return View(@class);
        }



        // POST: Classes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit1([Bind(Include = "Classes_Id,Title,Description,Active,Start_Date,End_Date,Create_Date,Update_Date")] Class @class)
        {
            if (ModelState.IsValid)
            {
                db.Entry(@class).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(@class);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, string[] Selectedstudents)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var classToUpdate = db.Class
             .Where(i => i.Classes_Id == id)
             .Single();

            if (TryUpdateModel(classToUpdate, "",
                   new string[] { "First_Name", "Last_Name", "Date_of_birth", "Phone_Number", "Picture", "Start_Date", "End_Date", "Create_Date", "Update_Date" }))
            {
                try
                {

                    UpdateClass_students(Selectedstudents, classToUpdate);

                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            //    PopulateAssignedCourseData(teacherToUpdate);
            return View(classToUpdate);
        }

        private void UpdateClass_students(string[] selectedstudents, Class classToUpdate)
        {

            if (selectedstudents == null)
            {
                classToUpdate.Student = new List<Student>();
                return;
            }

            var selectedStudentsHS = new HashSet<string>(selectedstudents);
            var teacherClasses = new HashSet<int>
                (classToUpdate.Student.Select(c => c.Student_Id));
            foreach (var Student in db.Student)
            {
                if (selectedStudentsHS.Contains(Student.Student_Id.ToString()))
                {
                    if (!teacherClasses.Contains(Student.Student_Id))
                    {
                        classToUpdate.Student.Add(Student);
                    }
                }
                else
                {
                    if (teacherClasses.Contains(Student.Student_Id))
                    {
                        classToUpdate.Student.Remove(Student);
                    }
                }
            }
        }

        // GET: Classes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Class @class = await db.Class.FindAsync(id);
            if (@class == null)
            {
                return HttpNotFound();
            }
            return View(@class);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Class @class = await db.Class.FindAsync(id);
            db.Class.Remove(@class);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
