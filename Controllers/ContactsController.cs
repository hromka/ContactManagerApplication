using ContactManagerApplication.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ContactManagerApplication.Controllers
{
    public class ContactsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Contacts/Index
        public ActionResult Index()
        {
            var contacts = db.Contacts.ToList();
            return View(contacts);
        }

        // POST: /Contacts/Upload
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {

                    using (var reader = new StreamReader(file.InputStream))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                       
                        csv.Context.RegisterClassMap<ContactMap>();

                        var records = csv.GetRecords<Contact>().ToList();

                        foreach (var record in records)
                        {
                            
                            db.Contacts.Add(record);
                        }

                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error processing CSV: " + ex.Message;
                    return View("Index", db.Contacts.ToList());
                }
            }
            ViewBag.Error = "No file uploaded.";
            return View("Index", db.Contacts.ToList());
        }

        // POST: /Contacts/Edit
        [HttpPost]
        public ActionResult Edit(Contact contact)
        {
            if (ModelState.IsValid)
            {
                db.Entry(contact).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // POST: /Contacts/Delete
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var contact = db.Contacts.Find(id);
            if (contact != null)
            {
                db.Contacts.Remove(contact);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, error = "Contact not found" });
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