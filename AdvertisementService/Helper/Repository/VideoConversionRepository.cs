using AdvertisementService.Helper.Abstraction;
using FFMpegCore;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Helper.Repository
{
    public class VideoConversionRepository : IVideoConversionRepository
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VideoConversionRepository(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public string ConvertVideo(IFormFile file, bool mute)
        {
            string uniqueFileName = Path.GetFileNameWithoutExtension(file.FileName.Replace(" ", "_").Replace(".jpg", "").Replace(".png", "").Replace(".jpeg", "")) + "_" + Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string keycode = AdvertisementService.Models.Common.Random.RandomUnique(10);
            string uniqueFileNameForResizeImg = Path.GetFileNameWithoutExtension(file.FileName.Replace(" ", "_").Replace(".jpg", "").Replace(".png", "").Replace(".jpeg", "")) + "_" + Guid.NewGuid().ToString() + "_" + keycode + Path.GetExtension(file.FileName);
            var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "TempVideo");
            string filePath = Path.Combine(uploads, uniqueFileName);
            var path = _webHostEnvironment.WebRootPath + "\\TempVideo";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            // Set input file and output file
            var inputFile = new MediaFile { Filename = filePath };
            var outputFilePath = filePath.Split('.')[0] + "_" + Guid.NewGuid() + ".mp4";
            var outputFile = new MediaFile { Filename = outputFilePath };

            // Set Video conversion options
            var conversionOptions = new ConversionOptions();
            conversionOptions.VideoSize = VideoSize.Hd720;
            conversionOptions.VideoBitRate = 800;
            conversionOptions.AudioSampleRate = AudioSampleRate.Default;

            // Convert video using MediaToolKit
            using (var engine = new Engine())
            {
                engine.Convert(inputFile, outputFile, conversionOptions);
            }

            if (File.Exists(filePath))
                File.Delete(filePath);

            if (mute)
            {
                var muteFile = outputFile.Filename.Split('.')[0] + "_" + Guid.NewGuid() + ".mp4";
                // Mutes output video file from MediaToolKit conversion and produces another muted video file
                FFMpegOptions options = new FFMpegOptions { RootDirectory = _webHostEnvironment.WebRootPath + "\\FFMpeg" };
                FFMpegOptions.Configure(options);
                FFMpeg.Mute(outputFile.Filename, muteFile);

                // Delete first output file with audio
                if (System.IO.File.Exists(outputFile.Filename))
                {
                    System.IO.File.Delete(outputFile.Filename);
                }

                if (File.Exists(outputFilePath))
                    File.Delete(filePath);

                return muteFile;
            }

            return outputFile.Filename;
        }
    }
}
