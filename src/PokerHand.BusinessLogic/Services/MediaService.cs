using System;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using System.Threading.Tasks;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Helpers.Media;

namespace PokerHand.BusinessLogic.Services
{
    public class MediaService : IMediaService
    {
        //TODO: Add resizing logics
        
        private readonly IFileSystem _fileSystem;

        public MediaService() : this (new FileSystem()) { }

        public MediaService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        
        public async Task<ResultModel<byte[]>> GetProfileImage(string playerIdJson)
        {
            var result = new ResultModel<byte[]>();
            
            var playerId = JsonSerializer.Deserialize<Guid>(playerIdJson);

            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages",
                $"{playerId.ToString()}.jpg");

            if (_fileSystem.File.Exists(path) is false)
            {
                result.IsSuccess = false;
                result.Message = $"File {path} doesn't exist";

                return result;
            }

            await using var fileStream = _fileSystem.File.OpenRead(path);
            var profileImage = new byte[fileStream.Length];
            await fileStream.ReadAsync(profileImage.AsMemory(0, profileImage.Length));

            result.IsSuccess = true;
            result.Value = profileImage;
            return result;
        }

        public async Task<ResultModel<byte[]>> UpdateProfileImage(string imageJson)
        {
            var result = new ResultModel<byte[]>();
            
            var image = JsonSerializer.Deserialize<Image>(imageJson);
            if (image?.BinaryImage is null || image.PlayerId == Guid.Empty)
            {
                result.IsSuccess = false;
                result.Message = "Image or Id is null";

                return result;
            }
                
            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages",
                $"{image.PlayerId.ToString()}.jpg");

            if (_fileSystem.File.Exists(path))
                RemoveProfileImage(image.PlayerId.ToString());

            await _fileSystem.File.WriteAllBytesAsync(path, image.BinaryImage);

            result.IsSuccess = true;
            result.Value = image.BinaryImage;
            return result;
        }

        public async Task<ResultModel<byte[]>> SetDefaultProfileImage(string playerIdJson)
        {
            var result = new ResultModel<byte[]>();
            
            var playerId = JsonSerializer.Deserialize<Guid>(playerIdJson);
            if (playerId == Guid.Empty)
            {
                result.IsSuccess = false;
                result.Message = "Player Id is empty";
                return result;
            }

            var sourceFileName = Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages", "default.jpg");
            if (_fileSystem.File.Exists(sourceFileName) is false)
            {
                result.IsSuccess = false;
                result.Message = "Source file not found";
                return result;
            }
            
            var destFileName = Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages", $"{playerId.ToString()}.jpg");
            if (_fileSystem.File.Exists(destFileName))
            {
                RemoveProfileImage(playerId.ToString());
            }
            
            _fileSystem.File.Copy(sourceFileName, destFileName);
            
            await using var fileStream = _fileSystem.File.OpenRead(destFileName);
            var profileImage = new byte[fileStream.Length];
            await fileStream.ReadAsync(profileImage.AsMemory(0, profileImage.Length));

            result.IsSuccess = true;
            result.Value = profileImage;
            return result;
        }

        #region Helpers

        private void RemoveProfileImage(string playerId)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages",
                $"{playerId}.jpg");

            if (_fileSystem.File.Exists(path) is false)
                return;
            
            _fileSystem.File.Delete(path);
        }

        #endregion
    }
}