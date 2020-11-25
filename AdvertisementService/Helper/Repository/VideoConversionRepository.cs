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
        private readonly IWebHostEnvironment _env;
        public VideoConversionRepository(IWebHostEnvironment webHostEnvironment)
        {
            _env = webHostEnvironment;
        }
        public async Task<VideoMetadata> ConvertVideoAsync(string file)
        {
            try
            {
                VideoMetadata videoMetadata = new VideoMetadata();
                CloudBlockBlob blockBlob = new CloudBlockBlob(new Uri(file));
                var uploadPath = Path.Combine(_env.WebRootPath, "TempVideo");
                var fileName = blockBlob.Name.Split('/');
                var updatedFileName = fileName.Last().Split('.');
                string inputFilePath = Path.Combine(uploadPath, updatedFileName.First() + "_input" + ".mp4");
                string outputFilePath = Path.Combine(uploadPath, updatedFileName.First() + "_output" + ".mp4");
                string originalFilePath = Path.Combine(uploadPath, fileName.Last());

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

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
                FFMpegOptions options = new FFMpegOptions { RootDirectory = _env.WebRootPath + "\\FFMpeg" };
                FFMpegOptions.Configure(options);
                FFMpeg.Mute(outputFile.Filename, originalFilePath);
                var duration = outputFile.Metadata.Duration;
                FileInfo fInfo = new FileInfo(originalFilePath);
                // Length/1024 = kb
                // kb/1024 = mb
                var videoSize = Convert.ToDecimal(Convert.ToDecimal((fInfo.Length / 1024)) / 1024).ToString("0.##");   //display size in mb
                videoMetadata.CompressedFile = originalFilePath;
                videoMetadata.Duration = (float)duration.TotalSeconds;
                videoMetadata.VideoSize = (float)Convert.ToDecimal(videoSize);
                FileInfo inputFileInfo = new FileInfo(inputFilePath);
                FileInfo outputFileInfo = new FileInfo(outputFilePath);
                inputFileInfo.Delete();
                outputFileInfo.Delete();
                return videoMetadata;
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public async Task<float> ConvertImageAsync(string file)
        {
            try
            {
                CloudBlockBlob blockBlob = new CloudBlockBlob(new Uri(file));
                var uploadPath = Path.Combine(_env.WebRootPath, "TempImage");
                var fileName = blockBlob.Name.Split('/');
                string originalFilePath = Path.Combine(uploadPath, fileName.Last());

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                using (var fs = new FileStream(originalFilePath, FileMode.Create))
                {
                    await blockBlob.DownloadToStreamAsync(fs);
                }
                FileInfo fInfo = new FileInfo(originalFilePath);
                // Length/1024 = kb
                // kb/1024 = mb
                var size = Convert.ToDecimal(Convert.ToDecimal((fInfo.Length / 1024)) / 1024).ToString("0.##");   //display size in mb
                fInfo.Delete();
                return (float)Convert.ToDecimal(size);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
