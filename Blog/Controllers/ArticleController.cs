using Blog.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System;
using System.Collections.Generic;

namespace Blog.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult List()
        {
            var databese = new BlogDbContext();

            var articles = databese.Articles
                .Include(a => a.Author )
                .Include(a => a.Tags)
                .ToList();

            return View(articles);
        }

        public ActionResult Details(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var databese = new BlogDbContext();

            var article = databese.Articles.Where(a => a.Id == id).First();

            return View(article);
        }

        //GET: Article/Create
        [HttpGet]
        [Authorize]
        public ActionResult Create()
        {
            using (var db = new BlogDbContext())
            {
               
                var model = new ArticleViewModel();
                model.Categories = db.Categories.ToList();
                return View(model);
            }

            
        }

        //POST:Article/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create (ArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {

                    var authotId = database.Users.Where(u => u.UserName  == this.User.Identity.Name).First().Id;

                    var article = new Article(authotId, model.Title, model.Content, model.CategoryId);
                    this.SetArticleTags(article, model, database);
                    
                    database.Articles.Add(article);
                    database.SaveChanges();

                    return RedirectToAction("List");

                }
            }
            return View(model);
        }

        private void SetArticleTags(Article article, ArticleViewModel model, BlogDbContext database)
        {
            var tagStrings = model.Tags
                .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct();

            article.Tags.Clear();

            foreach (var tagString in tagStrings)
            {
                Tag tag = database.Tags.FirstOrDefault(t => t.Name.Equals(tagString));
                if (tag==null)
                {
                    tag = new Tag() { Name = tagString };
                    database.Tags.Add(tag);
                }
                article.Tags.Add(tag);
            }
        }


        //GET:Edit
        [HttpGet]
        public ActionResult Edit(int? id)
        {

            using (var db = new BlogDbContext())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var article = db.Articles.FirstOrDefault(a => a.Id == id);

                if (article == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var model = new ArticleViewModel();
                model.AuthorId = article.AuthorId;
                model.Title = article.Title;
                model.Content = article.Content;
                model.CategoryId = article.CategoryId;
                model.Categories = db.Categories.OrderBy(c => c.Name).ToList();
                model.Tags = string.Join(",", article.Tags.Select(t => t.Name));

                return View(model);
            }
        }


        //POST:Edit
        [HttpPost]
        public ActionResult Edit(int? id, ArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new BlogDbContext())
                {
                    var article = db.Articles.FirstOrDefault(a => a.Id == id);
                    article.Title = model.Title;
                    article.Content = model.Content;
                    article.CategoryId = model.CategoryId;
                    this.SetArticleTags(article, model, db);

                    db.Entry(article).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("List");
                }
            }

            return View(model);
        }

        //Get:Delete
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var article = db.Articles.Include(a => a.Category).Include(a => a.Tags).FirstOrDefault(a => a.Id == id);

                ViewBag.Tags = string.Join(",", article.Tags.Select(t => t.Name));

                if (article == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                return View(article);
            }
           
        }

        //POS:Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed (int? id)
        {
            using (var db = new BlogDbContext())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var article = db.Articles.FirstOrDefault(a => a.Id == id);

                if (article == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                db.Articles.Remove(article);
                db.SaveChanges();

                return RedirectToAction("List");
            }

        }

    }

    }
