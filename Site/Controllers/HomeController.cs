﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Site.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Site.Controllers
{
    public class HomeController : Controller
    {
        private readonly SiteDBContext _context;

        public HomeController(SiteDBContext context)
        {
            this._context = context;
        }

        public async Task<IActionResult> Start()
        {
            AllFilesViewModel model = new AllFilesViewModel
            {
                Images = await _context.Images.Where(p => p.IsPublished == 1).ToListAsync<Images>()
            };
            var SortedImages = model.Images.OrderByDescending(x => x.PublishedDate).ToList();

            model.Images = SortedImages;

            List<int> distinctCategoryIds = new List<int>();

            foreach (Images img in model.Images)
            {
                if (!distinctCategoryIds.Contains(img.CategoryId))
                {
                    distinctCategoryIds.Add(img.CategoryId);
                }
            }

            List<FileCategories> categories = new List<FileCategories>();

            foreach (int id in distinctCategoryIds)
            {
                categories.Add(await _context.FileCategories.FirstAsync<FileCategories>(i => i.CategoryId == id));
            }

            model.Categories = categories;

            return View(model);
        }

        public async Task<IActionResult> Index()
        {

            AllFilesViewModel model = new AllFilesViewModel
            {
                Images = await _context.Images.Where(s => s.StartImage == 1).ToListAsync<Images>()
            };


            List<int> distinctCategoryIds = new List<int>();

            foreach (Images img in model.Images)
            {
                if (!distinctCategoryIds.Contains(img.CategoryId))
                {
                    distinctCategoryIds.Add(img.CategoryId);
                }
            }

            List<FileCategories> categories = new List<FileCategories>();

            foreach (int id in distinctCategoryIds)
            {
                categories.Add(await _context.FileCategories.FirstAsync<FileCategories>(i => i.CategoryId == id));
            }

            model.Categories = categories;

            return View(model);

            /*
            List<Images> model = await _context.Images.Where(s => s.StartImage == 1).ToListAsync();

            return View(model);
            */
        }

        [Route("sitemap.xml")]
        public IActionResult Sitemap()
        {
            SitemapNode nodeClass = new SitemapNode();
            string nodeMap = nodeClass.GetSitemapDocument(nodeClass.GetSitemapNodes());
            return Content(nodeMap);
        }

        [HttpGet]
        [ActionName("IndexCategory")]
        public async Task<IActionResult> Index (int? categoryId)
        {
            AllFilesViewModel model = new AllFilesViewModel
            {
                Images = await _context.Images.Where<Images>(i => i.CategoryId == categoryId && i.IsPublished == 1).ToListAsync<Images>()
            };

            var SortedImages = model.Images.OrderByDescending(x => x.PublishedDate).ToList();

            model.Images = SortedImages;

            List<int> distinctCategoryIds = new List<int>();

            foreach (Images img in await _context.Images.Where<Images>(p => p.IsPublished == 1).ToListAsync<Images>())
            {
                if (!distinctCategoryIds.Contains(img.CategoryId))
                {
                    distinctCategoryIds.Add(img.CategoryId);
                }
            }

            List<FileCategories> categories = new List<FileCategories>();

            foreach (int id in distinctCategoryIds)
            {
                categories.Add(await _context.FileCategories.FirstAsync<FileCategories>(i => i.CategoryId == id));
            }

            var SortedCategories = categories.OrderByDescending(i => i.CategoryLabel).ToList();

            model.Categories = SortedCategories;

            return View("Start", model);
        }

        public IActionResult Download(string fileName)
        {
            int index = fileName.IndexOf('.') + 1;
            string mimeType = fileName.Substring(index).ToLower();

            string file = fileName.Substring(fileName.IndexOf('/') + 1);

            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
            {
                FileName = file,
                Inline = false
            };

            Response.Headers.Add("Content-Disposition", cd.ToString());
            Response.Headers.Add("X-Content-Type-Options", "nosniff");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", fileName);
            return File(System.IO.File.ReadAllBytes(path), "application/"+mimeType, true);
        }

        public IActionResult Login()
        {
            return View();
        }

        public async Task<IActionResult> Contact()
        {
            ContactText model = await _context.ContactText.FirstAsync(i => i.ID == 1);

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
