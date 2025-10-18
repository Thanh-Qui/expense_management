using expenseManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace expenseManagement.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Blog
        public async Task<IActionResult> Index()
        {
            var blogs = await _context.Blogs
        .Include(b => b.Images) // bao gồm hình ảnh
        .ToListAsync();

            return View(blogs);
        }

        // GET: Blog/AddOrEdit
        public IActionResult AddOrEdit(int id=0)
        {
            if (id == 0)
            {
                return View(new Blog());
            }else
            {
                return View(_context.Blogs.Find(id));
            }
            
        }

        // POST: Blog/AddOrEdit
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("Id,Title,Slug,Content,IsPublished,PublishedAt,CreatedAt,UpdatedAt")] Blog blog, List<IFormFile>? images)
        {
            if (ModelState.IsValid)
            {
               
                // tọa slug từ title
                blog.Slug = GenerateSlug(blog.Title);

                if (blog.Id == 0)
                {
                    // thêm mới blog
                    blog.CreatedAt = DateTime.Now;
                    blog.UpdatedAt = DateTime.Now;
                    blog.PublishedAt = blog.IsPublished ? DateTime.Now : null;
                    _context.Blogs.Add(blog);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var existingBlog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == blog.Id);
                    if (existingBlog == null) return NotFound();

                    existingBlog.Title = blog.Title;
                    existingBlog.Content = blog.Content;
                    existingBlog.Slug = blog.Slug;
                    existingBlog.IsPublished = blog.IsPublished;
                    existingBlog.UpdatedAt = DateTime.Now;

                    if (blog.IsPublished)
                        existingBlog.PublishedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                }


                // nếu có hình ảnh thì thực hiện lưu hình ảnh
                if (images != null && images.Count > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/blogs");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    foreach (var image in images)
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var blogImage = new BlogImage
                        {
                            BlogId = blog.Id,
                            ImagePath = "/uploads/blogs/" + uniqueFileName
                        };

                        _context.BlogImages.Add(blogImage);
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return Guid.NewGuid().ToString();
            return title
                .ToLower()
                .Replace(" ", "-")
                .Replace("?", "")
                .Replace("!", "")
                .Replace(",", "")
                .Replace(".", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("/", "")
                .Replace("\\", "");
        }
    }
}
