using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using TestSchool.Models;
using TestSchool.ViewModels;

namespace TestSchool.Controllers
{
    public class TeachersController : Controller
    {
        private testbaseEntities db = new testbaseEntities();

        // GET: Teachers
        public async Task<ActionResult> Index()
        {
            return View(await db.Teacher.ToListAsync());
        }

        // GET: Teachers/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teacher.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        // GET: Teachers/Create
        public ActionResult Create()
        {
            var teacher = new Teacher();
            teacher.Class = new List<Class>();
            PopulateAssignedClassData(teacher);
            return View(teacher);
        }

        // POST: Teachers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create1([Bind(Include = "Teacher_Id,First_Name,Last_Name,Date_of_birth,Phone_Number,Picture,Start_Date,End_Date,Create_Date,Update_Date")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                db.Teacher.Add(teacher);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(teacher);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Teacher_Id,First_Name,Last_Name,Date_of_birth,Phone_Number,Picture,Start_Date,End_Date,Create_Date,Update_Date")]Teacher teacher, string[] selectedClasses)
        {
            if (selectedClasses != null)
            {
                teacher.Class = new List<Class>();
                foreach (var course in selectedClasses)
                {
                    var classToAdd = db.Class.Find(int.Parse(course));
                    teacher.Class.Add(classToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                db.Teacher.Add(teacher);
                db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            PopulateAssignedClassData(teacher);
            return View(teacher);
        }
        // GET: Teachers/Edit/5
        public ActionResult Edit1(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teacher.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teacher
                .Where(i => i.Teacher_Id == id)
                .Single();
            PopulateAssignedClassData(teacher);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        private void PopulateAssignedClassData(Teacher teacher)
        {
            var allClasses = db.Class;
            var teacherClasses = new HashSet<int>(teacher.Class.Select(c => c.Classes_Id));
            var viewModel = new List<AssignedClassData>();
            foreach (var Class in allClasses)
            {
                viewModel.Add(new AssignedClassData
                {
                    ClassID = Class.Classes_Id,
                    Title = Class.Title,
                    Assigned = teacherClasses.Contains(Class.Classes_Id)
                });
            }
            ViewBag.Courses = viewModel;
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit1([Bind(Include = "Teacher_Id,First_Name,Last_Name,Date_of_birth,Phone_Number,Picture,Start_Date,End_Date,Create_Date,Update_Date")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                db.Entry(teacher).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, string[] selectedClasses)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
              var teacherToUpdate = db.Teacher
               .Where(i => i.Teacher_Id == id)
               .Single();

            if (TryUpdateModel(teacherToUpdate, "",
                   new string[] { "First_Name","Last_Name","Date_of_birth","Phone_Number", "Picture", "Start_Date", "End_Date", "Create_Date", "Update_Date" }))
            {
                try
                {

                    teacherClasses(selectedClasses, teacherToUpdate);

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
            return View(teacherToUpdate);
        }

        private void teacherClasses(string[] selectedClasses, Teacher teacherToUpdate)
        {

            if (selectedClasses == null)
            {
                teacherToUpdate.Class = new List<Class>();
                return;
            }

            var selectedClassesHS = new HashSet<string>(selectedClasses);
            var teacherClasses = new HashSet<int>
                (teacherToUpdate.Class.Select(c => c.Classes_Id));
            foreach (var _Class in db.Class)
            {
                if (selectedClassesHS.Contains(_Class.Classes_Id.ToString()))
                {
                    if (!teacherClasses.Contains(_Class.Classes_Id))
                    {
                        teacherToUpdate.Class.Add(_Class);
                    }
                }
                else
                {
                    if (teacherClasses.Contains(_Class.Classes_Id))
                    {
                        teacherToUpdate.Class.Remove(_Class);
                    }
                }
            }
        }

        // GET: Teachers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teacher.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Teacher teacher = db.Teacher.Find(id);
            db.Teacher.Remove(teacher);
            db.SaveChanges();
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
