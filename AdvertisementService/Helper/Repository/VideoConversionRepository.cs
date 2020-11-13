using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Models;
using FFMpegCore;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage.Blob;
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
        public async Task<VideoMetadata> ConvertVideoAsync(string file)
        {
            VideoMetadata videoMetadata = new VideoMetadata();
            CloudBlockBlob blockBlob = new CloudBlockBlob(new Uri(file));
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "TempVideo");
            var fileName = blockBlob.Name.Split('/');
            var updatedFileName = fileName.Last().Split('.');
            string inputFilePath = Path.Combine(uploadPath, updatedFileName.First() + "_input" + ".mp4");
            string outputFilePath = Path.Combine(uploadPath, updatedFileName.First() + "_output" + ".mp4");
            string originalFilePath = Path.Combine(uploadPath, fileName.Last());

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            DirectoryInfo di = new DirectoryInfo(uploadPath);
            foreach (FileInfo files in di.GetFiles())
            {
                files.Delete();
            }

            using (var fs = new FileStream(inputFilePath, FileMode.Create))
            {
                await blockBlob.DownloadToStreamAsync(fs);
            }

            // Set input file and output file
            var inputFile = new MediaFile { Filename = inputFilePath };
            var outputFile = new MediaFile { Filename = outputFilePath };

            // Set Video conversion options
            var conversionOptions = new ConversionOptions();
            conversionOptions.VideoSize = VideoSize.Hd720;
            conversionOptions.VideoBitRate = 800;
            conversionOptions.AudioSampleRate = AudioSampleRate.Default;

            // Convert video using MediaToolKit
            try
            {
                using (var engine = new Engine())
                {
                    engine.Convert(inputFile, outputFile, conversionOptions);
                    engine.GetMetadata(outputFile);

                }
            }
            catch (Exception) { return videoMetadata; }

            // Mutes output video file from MediaToolKit conversion and produces another muted video file
            FFMpegOptions options = new FFMpegOptions { RootDirectory = _webHostEnvironment.WebRootPath + "\\FFMpeg" };
            FFMpegOptions.Configure(options);
            FFMpeg.Mute(outputFile.Filename, originalFilePath);
            var duration = outputFile.Metadata.Duration;
            FileInfo fInfo = new FileInfo(originalFilePath);
            // Length/1024 = kb
            // kb/1024 = mb
            var videoSize = (fInfo.Length / 1024) / 1024;   //display size in mb

            videoMetadata.CompressedFile = originalFilePath;
            videoMetadata.Duration = (float)duration.TotalSeconds;
            videoMetadata.VideoSize = videoSize;
            return videoMetadata;
        }
    }
}
