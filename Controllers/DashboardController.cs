using Book_Managment_SYS.Data;
using Book_Managment_SYS.Models;
using Book_Managment_SYS.Models.DTO_s;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using X.PagedList.Extensions;
using X.PagedList;
using X.PagedList.Mvc.Core;
using ClosedXML.Excel;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Book_Managment_SYS.Controllers
{

    public class DashboardController : Controller
    {
        private readonly Data_DbContext _context;
        private readonly IWebHostEnvironment _environment;
        public DashboardController(Data_DbContext dbcontext,IWebHostEnvironment env)
        {
            this._context = dbcontext;
            this._environment = env;
        }

        [HttpGet]public IActionResult Index()
        {
           
            ViewBag.c_count = _context.Categories.Count();
            ViewBag.c_book = _context.Books.Count();
            return View();
        }


        //for category
        [HttpGet]public async Task<IActionResult> CategoryIndex(string searchquery,int? page) {
            int pagesize = 5;
            int pagenum = (page ?? 1);

            var data = _context.Categories.AsQueryable();
            if (!string.IsNullOrEmpty(searchquery)) {
                data = data.Where(d=>d.category_name.Contains(searchquery)); 
            }
            var list =  data.OrderBy(d => d.Id).ToPagedList(pagenum, pagesize);

            if (!list.Any()) {
                ViewBag.NotFound="Category Data is Not Found";
            }

            return View(list);

        }
        [HttpGet] public async Task<IActionResult> category_toExcel() {

            var data = await _context.Categories.ToListAsync();
            using (var excel = new XLWorkbook()) {
                var worksheet = excel.Worksheets.Add("Category");
                var currentrow = 1;

                worksheet.Cell(currentrow, 1).Value = "CategoryID";
                worksheet.Cell(currentrow, 2).Value = "CategoryName";
                worksheet.Cell(currentrow, 3).Value = "CategoryImage";
              
                foreach (var d in data) {
                    currentrow++;
                    worksheet.Cell(currentrow, 1).Value = d.Id ;
                    worksheet.Cell(currentrow, 2).Value = d.category_name;
                    worksheet.Cell(currentrow, 3).Value = d.category_image;
                }
                using (var stream = new MemoryStream()) {
                    excel.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,"application/vnd.openxmlformats-officedocument.spredsheet","Category.xlsx");
                }
            }
        
        }
        [HttpGet]public IActionResult CategoryCreate() {
            return View();
        }
        [HttpPost]public async Task<IActionResult> CategoryCreateAction(categoryDTO cdto) {
            if (cdto.category_image == null)
            {
                ModelState.AddModelError("category_image", "Category Image is Required");
            }
            if (!ModelState.IsValid)
            {
                return View(cdto);
            }
            string date = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string filename = Path.GetExtension(cdto.category_image.FileName);
            filename = $"{date}{filename}";
            string fpath = $"/Gallery/Dashboard/Category/{filename}";
            string fullpath = Path.Combine(_environment.WebRootPath, "Gallery/Dashboard/Category", filename);
            using (var stream = System.IO.File.Create(fullpath))
            {
                await cdto.category_image.CopyToAsync(stream);

            }

            var data = new Category()
            {
                category_name = cdto.category_name,
                category_image = fpath
            };
            await _context.Categories.AddAsync(data);
            await _context.SaveChangesAsync();
            return RedirectToAction("CategoryIndex");
        }

        [HttpGet]public async Task<IActionResult> CategoryEdit(int id) {
            var sort = await _context.Categories.FindAsync(id);

            if (sort != null) {
                var data = new categoryDTO()
                {
                category_name = sort.category_name
                };
                ViewBag.id = sort.Id;
                ViewBag.img = sort.category_image;
            return View(data);
            }
            return RedirectToAction("CategoryIndex");
        }
        [HttpPost]
        public async Task<IActionResult> CategoryEditAction(int id, categoryDTO cdto)
        {
            

            var sort = await _context.Categories.FindAsync(id);

            if (sort == null)
            {
                return NotFound();
            }
            
            if (cdto.category_image != null)
            { 
               
                string date = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                string filename = Path.GetExtension(cdto.category_image.FileName);
                filename = $"{date}{filename}";
                string fpath = $"/Gallery/Dashboard/Category/{filename}";
                string fullpath = Path.Combine(_environment.WebRootPath, "Gallery/Dashboard/Category", filename);
                using (var stream = System.IO.File.Create(fullpath))
                {
                    await cdto.category_image.CopyToAsync(stream);

                }
                string oldImagePath = Path.Combine(_environment.WebRootPath, cdto.category_oldImg.TrimStart('/'));
                System.IO.File.Delete(oldImagePath);


                sort.category_name = cdto.category_name;
                sort.category_image = fpath;
                _context.SaveChanges();
               
                


            }
            else {
                
                    sort.category_name = cdto.category_name;
                    sort.category_image = cdto.category_oldImg;
                
                 _context.SaveChanges();
            
            }

                return RedirectToAction("CategoryIndex");

        }

            [HttpGet]public async Task<IActionResult> CategoryDelete(int id) {
                var sort = await _context.Categories.FindAsync(id);
                if (sort!=null) {
                string oldImagePath = Path.Combine(_environment.WebRootPath, sort.category_image.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath)) { 
                System.IO.File.Delete(oldImagePath);
                _context.Categories.Remove(sort);
                 await _context.SaveChangesAsync();
                }
                }
                return RedirectToAction("CategoryIndex");

            }

        [HttpGet]public IActionResult BookCreate() {
            ViewBag.data = new SelectList(_context.Categories, "Id", "category_name");
            return View();
        }

        //[HttpPost]public async Task<IActionResult> BookCreateAction(bookDTO bdto)
        //{
        //    //if (!ModelState.IsValid) {

        //    //    return View("BookCreate", bdto);
        //    //}
        //    if (bdto.book_img == null) {
        //        ModelState.AddModelError("book_img","Image is Not Found Try Again");
        //    }
        //    string date = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        //    string filename =  Path.GetExtension(bdto.book_img.FileName);
        //    filename = $"{date}{filename}";
        //    string fpath = $"/Gallery/Dashboard/Book/{filename}";
        //    string fullpath = Path.Combine(_environment.WebRootPath, "Gallery/Dashboard/Book", filename);
        //    using (var stream = System.IO.File.Create(fullpath))
        //    {
        //        await bdto.book_img.CopyToAsync(stream);

        //    }

        //   var data = new Book() { 
        //   book_title = bdto.book_title,
        //   book_author = bdto.book_author,
        //   book_pages = bdto.book_pages,
        //   book_price = bdto.book_price,
        //   book_desc  = bdto.book_desc,
        //   book_img = fpath,
        //       //Category = bdto.category
        //       book_category = bdto.category_id // 👈 Correct mapping

        //   };
        //    await _context.Books.AddAsync(data);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction("BookIndex");



        //}

        [HttpPost]
        public async Task<IActionResult> BookCreateAction(bookDTO model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = "";

               
                if (model.book_img != null)
                {
                 
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

                    string extension = Path.GetExtension(model.book_img.FileName);
                    string datetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    uniqueFileName = "book_" + datetime + extension;

                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                   
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.book_img.CopyToAsync(fileStream);
                    }
                }

               
                var newBook = new Book
                {
                    book_title = model.book_title,
                    book_author = model.book_author,
                    book_pages = model.book_pages,
                    book_price = model.book_price,
                    book_desc = model.book_desc,
                    book_img = uniqueFileName, 
                    book_category = model.book_category
                };

                _context.Books.Add(newBook);
                await _context.SaveChangesAsync();

                return RedirectToAction("BookList"); 
            }

            return View(model);
        }


        [HttpGet]public IActionResult BookIndex(string searchquery,int? page) {
            int pagesize = 5;
            int pagenum = (page ?? 1);
            var data = _context.Books.Include(d => d.Category).AsQueryable();
            if (!string.IsNullOrEmpty(searchquery)) {
            
             data = _context.Books.Include(d=> d.Category).Where(d=>d.Category.category_name.Contains(searchquery)||d.book_title.Contains(searchquery));
            }
            var list = data.OrderBy(d => d.book_id).ToPagedList(pagenum, pagesize);
            if (!list.Any()) {
                ViewBag.notfound = "Book Data is Not Found";
            }
            return View(data);


          


        }
        [HttpGet]public IActionResult BookEdit(int id) {

            var sort = _context.Books.Find(id);
            if (sort == null)
            {
                return NotFound();
            }
            var bdto = new bookDTO() { 
            book_title =  sort.book_title,
            book_author = sort.book_author,
            oldimage = sort.book_img,
            book_price = sort.book_price,
            book_pages =sort.book_pages,
            book_category = sort.book_category
            };
            ViewBag.data = new SelectList(_context.Categories, "Id", "category_name",bdto.book_category);
            ViewBag.id = sort.book_id;
            return View(bdto);
            
        }
        [HttpPost]public async Task<IActionResult> BookEditAction(int id,bookDTO bdto) {

            var sort = _context.Books.Find(id);
            if (sort == null) {
                return NotFound();

            }
            if (bdto.book_img != null)
            {

                string date = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                string filename = Path.GetExtension(bdto.book_img.FileName);
                filename = $"{date}{filename}";
                string fpath = $"/Gallery/Dashboard/Book/{filename}";
                string fullpath = Path.Combine(_environment.WebRootPath, "Gallery/Dashboard/Book", filename);
                using (var stream = System.IO.File.Create(fullpath))
                {
                    await bdto.book_img.CopyToAsync(stream);

                }
                //string oldImagePath = Path.Combine(_environment.WebRootPath, bdto.oldimage);
                //System.IO.File.Delete(oldImagePath);

                string oldImagePath = Path.Combine(_environment.WebRootPath, bdto.oldimage.TrimStart('/'));
                System.IO.File.Delete(oldImagePath);

                sort.book_title = bdto.book_title;
                sort.book_img = fpath;
                sort.book_author = bdto.book_author;
                sort.book_price = bdto.book_price;
                sort.book_category = bdto.book_category;
                sort.book_pages = bdto.book_pages;
                sort.book_desc = bdto.book_desc;
                _context.SaveChanges();
            }
            else {
                sort.book_title = bdto.book_title;
                sort.book_img = bdto.oldimage;
                sort.book_author = bdto.book_author;
                sort.book_price = bdto.book_price;
                sort.book_category = bdto.book_category;
                sort.book_pages = bdto.book_pages;
                sort.book_desc = bdto.book_desc;

                await _context.SaveChangesAsync();

            }

            return RedirectToAction("BookIndex");
        
        }

        [HttpGet]
        public async Task<IActionResult> book_toExcel()
        {

            var data = _context.Books.Include(d => d.Category);
            using (var excel = new XLWorkbook())
            {
                var worksheet = excel.Worksheets.Add("Book");
                var currentrow = 1;

                worksheet.Cell(currentrow, 1).Value = "Book ID";
                worksheet.Cell(currentrow, 2).Value = "Book Category";
                worksheet.Cell(currentrow, 3).Value = "Book Title";
                worksheet.Cell(currentrow, 4).Value = "Book Author";
                worksheet.Cell(currentrow, 5).Value = "Book Pages";
                worksheet.Cell(currentrow, 6).Value = "Book Price";
                worksheet.Cell(currentrow, 7).Value = "Book Image";
                worksheet.Cell(currentrow, 8).Value = "Book Description";

                foreach (var d in data)
                {
                    currentrow++;
                    worksheet.Cell(currentrow, 1).Value = d.book_id;
                    worksheet.Cell(currentrow, 2).Value = d.Category.category_name;
                    worksheet.Cell(currentrow, 3).Value = d.book_title;
                    worksheet.Cell(currentrow, 4).Value = d.book_author;
                    worksheet.Cell(currentrow, 5).Value = d.book_pages;
                    worksheet.Cell(currentrow, 6).Value = d.book_price;
                    worksheet.Cell(currentrow, 7).Value = d.book_img;
                    worksheet.Cell(currentrow, 8).Value = d.book_desc;
                }
                using (var stream = new MemoryStream())
                {
                    excel.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spredsheet", "Book.xlsx");
                }
            }

        }

        //[HttpPost] public async Task<IActionResult> BookDelete(int id)
        //{
        //    string name = "a";
        //    return View(await name);

        //}
    }
}
