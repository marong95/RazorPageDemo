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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RazorPageDemo2.Data;
using RazorPageDemo2.Models;

namespace RazorPageDemo2.Pages.Movies
{
    public class CreateModel : PageModel
    {
        private readonly RazorPageDemo2.Data.RazorPageDemo2Context _context;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IConfiguration _config;


        public CreateModel(RazorPageDemo2.Data.RazorPageDemo2Context context, IWebHostEnvironment environment, IConfiguration configuration)
        {
            hostingEnvironment = environment;
            _context = context;
            _config = configuration;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Movie Movie { get; set; }
        [BindProperty]
        public IFormFile Image { set; get; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
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

                this.Movie.ImageName = fileName; // Set the file name
            }

            _context.Movie.Add(Movie);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
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

        private string GetUniqueName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                   + "_" + Guid.NewGuid().ToString().Substring(0, 4)
                   + Path.GetExtension(fileName);
        }
    }
}
