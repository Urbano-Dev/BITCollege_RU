﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BITCollege_RU.Data;
using BITCollege_RU.Models;

namespace BITCollege_RU.Controllers
{
    public class GradedCoursesController : Controller
    {
        private BITCollege_RUContext db = new BITCollege_RUContext();

        // GET: GradedCourses
        public ActionResult Index()
        {
            return View(db.GradedCourses.ToList());
        }

        // GET: GradedCourses/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GradedCourse gradedCourse = db.GradedCourses.Find(id);
            if (gradedCourse == null)
            {
                return HttpNotFound();
            }
            return View(gradedCourse);
        }

        // GET: GradedCourses/Create
        public ActionResult Create()
        {
            ViewBag.AcademicProgramId = new SelectList(db.AcademicPrograms, "AcademicProgramId", "ProgramAcronym");
            return View();
        }

        // POST: GradedCourses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CourseId,AcademicProgramId,CourseNumber,Title,CreditHours,TuitionAmount,Notes,AssignmentWeight,ExamWeight")] GradedCourse gradedCourse)
        {
            if (ModelState.IsValid)
            {
                gradedCourse.SetNextCourseNumber();
                db.Courses.Add(gradedCourse);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(gradedCourse);
        }

        // GET: GradedCourses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GradedCourse gradedCourse = db.GradedCourses.Find(id);
            if (gradedCourse == null)
            {
                return HttpNotFound();
            }
            return View(gradedCourse);
        }

        // POST: GradedCourses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CourseId,AcademicProgramId,CourseNumber,Title,CreditHours,TuitionAmount,Notes,AssignmentWeight,ExamWeight")] GradedCourse gradedCourse)
        {
            if (ModelState.IsValid)
            {
                db.Entry(gradedCourse).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(gradedCourse);
        }

        // GET: GradedCourses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GradedCourse gradedCourse = db.GradedCourses.Find(id);
            if (gradedCourse == null)
            {
                return HttpNotFound();
            }
            return View(gradedCourse);
        }

        // POST: GradedCourses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            GradedCourse gradedCourse = db.GradedCourses.Find(id);
            db.Courses.Remove(gradedCourse);
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
