using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RazorPageDemo2.Data;
using RazorPageDemo2.Models;

namespace RazorPageDemo2.Pages.Movies
{
    public class DeleteModel : PageModel
    {
        private readonly RazorPageDemo2.Data.RazorPageDemo2Context _context;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IConfiguration _config;

        public DeleteModel(RazorPageDemo2.Data.RazorPageDemo2Context context, IWebHostEnvironment environment, IConfiguration configuration)
        {
            hostingEnvironment = environment;
            _context = context;
            _config = configuration;
        }

        [BindProperty]
        public Movie Movie { get; set; }

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
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Movie = await _context.Movie.FindAsync(id);

            if (Movie != null)
            {
                if(Movie.ImageName != null)
                {
                    /*var uploads = Path.Combine(hostingEnvironment.WebRootPath, "uploads");
                    var oldPath = Path.Combine(uploads, Movie.ImageName);
                    if (oldPath != null && System.IO.File.Exists(oldPath))
                    {
                        //删除文件
                        System.IO.File.Delete(oldPath);
                    }*/
                    await DeleteFileFromStorage(Movie.ImageName);
                }
               

                _context.Movie.Remove(Movie);
                await _context.SaveChangesAsync();
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
    }
}
