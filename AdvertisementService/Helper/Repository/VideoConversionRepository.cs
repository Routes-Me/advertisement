using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Models;
using FFMpegCore;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Helper.Repository
{
    public class VideoConversionRepository : IVideoConversionRepository
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<VideoConversionRepository> _logger;
        public VideoConversionRepository(IWebHostEnvironment webHostEnvironment, ILogger<VideoConversionRepository> logger)
        {
            _env = webHostEnvironment;
            _logger = logger;
        }
        public async Task<VideoMetadata> ConvertVideoAsync(string file)
        {
            try
            {
                VideoMetadata videoMetadata = new VideoMetadata();
                CloudBlockBlob blockBlob = new CloudBlockBlob(new Uri(file));
               // var uploadPath = Path.Combine(_env.ContentRootPath, "CompressedFiles");
                var fileName = blockBlob.Name.Split('/');
                var updatedFileName = fileName.Last().Split('.');
                string inputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, updatedFileName.First() + "_input" + ".mp4");
                string outputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName.Last());

                //if (!Directory.Exists(uploadPath))
                //    Directory.CreateDirectory(uploadPath);

                using (var fs = new FileStream(inputFilePath, FileMode.Create))
                {
                    await blockBlob.DownloadToStreamAsync(fs);
                }

                // Set input file and output file
                var inputFile = new MediaFile(inputFilePath);
                var outputFile = new MediaFile(outputFilePath);

                var videoSize = Convert.ToDecimal(Convert.ToDecimal((new FileInfo(inputFilePath).Length / 1024)) / 1024).ToString("0.##");   //display size in mb

                // Set Video conversion options
                var conversionOptions = new ConversionOptions();
                conversionOptions.VideoSize = VideoSize.Hd720;
                conversionOptions.VideoBitRate = 800;
                conversionOptions.AudioSampleRate = AudioSampleRate.Default;

                _logger.LogInformation("inputFile -" + inputFile.Filename);
                _logger.LogInformation("outputFile -" + outputFile.Filename);
                // Convert video using MediaToolKit
                try
                {
                    using (var engine = new Engine())
                    {
                        if (Convert.ToDecimal(videoSize) >= 3)
                            engine.Convert(inputFile, outputFile, conversionOptions);
                        else
                            engine.Convert(inputFile, outputFile, null);
                        engine.GetMetadata(outputFile);
                        engine.Dispose();
                    }
                }
                catch (Exception ex) { _logger.LogError(ex.Message); }

                if (outputFile != null)
                {
                    if (outputFile.Metadata != null)
                    {
                        var duration = outputFile.Metadata.Duration;
                        videoMetadata.Duration = (float)duration.TotalSeconds;
                    }
                }

                videoMetadata.CompressedFile = outputFilePath;
                videoMetadata.VideoSize = (float)Convert.ToDecimal(videoSize);
                File.Delete(inputFilePath);
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
                //var uploadPath = Path.Combine(_env.ContentRootPath, "CompressedFiles");
                var fileName = blockBlob.Name.Split('/');
                string originalFilePath = Path.Combine(_env.ContentRootPath, fileName.Last());

                //if (!Directory.Exists(uploadPath))
                //    Directory.CreateDirectory(uploadPath);

                using (var fs = new FileStream(originalFilePath, FileMode.Create))
                {
                    await blockBlob.DownloadToStreamAsync(fs);
                }
                FileInfo fInfo = new FileInfo(originalFilePath);
                // Length/1024 = kb
                // kb/1024 = mb
                var size = Convert.ToDecimal(Convert.ToDecimal((fInfo.Length / 1024)) / 1024).ToString("0.####");   //display size in mb
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

