using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RazorPageDemo2.Data;
using RazorPageDemo2.Models;

namespace RazorPageDemo2.Pages.Movies
{
    public class EditModel : PageModel
    {
        private readonly RazorPageDemo2.Data.RazorPageDemo2Context _context;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IConfiguration _config;

        public EditModel(RazorPageDemo2.Data.RazorPageDemo2Context context, IWebHostEnvironment environment, IConfiguration configuration)
        {
            hostingEnvironment = environment;
            _context = context;
            _config = configuration;
        }

        [BindProperty]
        public Movie Movie { get; set; }
        [BindProperty]
        public IFormFile Image { set; get; }
        public string name;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Movie = await _context.Movie.FirstOrDefaultAsync(m => m.ID == id);

            if (Movie == null)
            {
                return NotFound();
            }
            else { 
                name = Movie.ImageName;
            }
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Movie = await _context.Movie.FindAsync(id);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Image != null)
            {
                var fileName = GetUniqueName(this.Image.FileName);

                /* var uploads = Path.Combine(hostingEnvironment.WebRootPath, "uploads");
                 var filePath = Path.Combine(uploads, fileName);
                 this.Image.CopyTo(new FileStream(filePath, FileMode.Create));*/
                Stream imageStream = Image.OpenReadStream();
                await UploadFileToStorage(imageStream, fileName);

                if (Movie != null)
                {
                    if (Movie.ImageName != null)
                    {
                        /* var oldPath = Path.Combine(uploads, Movie.ImageName);
                         if (oldPath != null && System.IO.File.Exists(oldPath))
                         {
                             //删除文件
                             System.IO.File.Delete(oldPath);
                         }*/
                        await DeleteFileFromStorage(Movie.ImageName);
                    }
                }
                //await DeleteFileFromStorage("2_daa8.jpg");

                Movie.ImageName = fileName; // Set the file name
            }

            _context.Attach(Movie).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(Movie.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private async Task<bool> DeleteFileFromStorage(string fileName)
        {
            // Create a URI to the blob
            Uri blobUri = new Uri("https://" +
                                  _config.GetConnectionString("BolbStorageAccountName") +
                                  ".blob.core.windows.net/" +
                                  _config.GetConnectionString("BolbStorageAccountImageContainer") +
                                  "/" + fileName);

            // Create StorageSharedKeyCredentials object by reading
            // the values from the configuration (appsettings.json)
            StorageSharedKeyCredential storageCredentials =
                new StorageSharedKeyCredential(_config.GetConnectionString("BolbStorageAccountName"), _config.GetConnectionString("BolbStorageAccountKey"));

            // Create the blob client.
            BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

            // Upload the file
            await blobClient.DeleteIfExistsAsync();

            return await Task.FromResult(true);
        }

        private async Task<bool> UploadFileToStorage(Stream fileStream, string fileName)
        {
            // Create a URI to the blob
            Uri blobUri = new Uri("https://" +
                                  _config.GetConnectionString("BolbStorageAccountName") +
                                  ".blob.core.windows.net/" +
                                  _config.GetConnectionString("BolbStorageAccountImageContainer") +
                                  "/" + fileName);

            // Create StorageSharedKeyCredentials object by reading
            // the values from the configuration (appsettings.json)
            StorageSharedKeyCredential storageCredentials =
                new StorageSharedKeyCredential(_config.GetConnectionString("BolbStorageAccountName"), _config.GetConnectionString("BolbStorageAccountKey"));

            // Create the blob client.
            BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

            // Upload the file
            await blobClient.UploadAsync(fileStream);

            return await Task.FromResult(true);
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.ID == id);
        }

        private string GetUniqueName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                   + "_" + Guid.NewGuid().ToString().Substring(0, 4)
                   + Path.GetExtension(fileName);
        }
    }
}

