using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using PokerHand.BusinessLogic.Interfaces;
using PokerHand.Common;
using PokerHand.Common.Helpers.Bot;
using PokerHand.Common.Helpers.Media;
using PokerHand.DataAccess.Interfaces;
using Serilog;

namespace PokerHand.BusinessLogic.Services
{
    public class MediaService : IMediaService
    {
        private readonly IFileSystem _fileSystem;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITablesOnline _allTables;

        private const string DefaultImage = "/9j/4AAQSkZJRgABAQAAAQABAAD//gAfQ29tcHJlc3NlZCBieSBqcGVnLXJlY29tcHJlc3P/2wCEAAQEBAQEBAQEBAQGBgUGBggHBwcHCAwJCQkJCQwTDA4MDA4MExEUEA8QFBEeFxUVFx4iHRsdIiolJSo0MjRERFwBBAQEBAQEBAQEBAYGBQYGCAcHBwcIDAkJCQkJDBMMDgwMDgwTERQQDxAUER4XFRUXHiIdGx0iKiUlKjQyNEREXP/CABEIAXIBcgMBIgACEQEDEQH/xAAdAAEAAQUBAQEAAAAAAAAAAAAAAwECBAgJBwYF/9oACAEBAAAAAN/gAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABD8d+BL9L9YAAAAAAPAdWfHceqn6Xt23PqoAAAAAfPc8/HppLqqWR27sbA/ZAAAAAMLlz8PkS3XC2yOGDYroHeAAAADnpr9lTX1qKW2xwY2zu+YAAAAeMc1svIkrUFKWxY8HW36IAAAAc1vHsqW8AUsgxN2NtgAAAD8LkZlZElwFBbDi+tdMQAAADX/AJ35k99QAtixsvr8AAAAaj6U5U94AKRY2F2UuAAAAab6c5U14AKRY+J2SuAAAAaoaPZU94AKRY/6vXUAAAA8U5sZc99QAtixvV+moAAABjchYp5KgBbDjbXbvgAAADR3VfJkqAFkWL0a90AAAAHOjwvIlqAFsOLuVuKAAAAfj8gMvIlqAFsON9d1hAAAAPn+QuZky3UqKBZDi/ZdYAAAAByp+EyZrwUVUjgxNtd4QAAAB4Tzfyp77gC2LG/b6ufsgAAABpjp3ky3gKWQWdPvUQAAAAfJ8k8qeSoFsWN6104AAAAAcy/H8mW8FLMfG3z2lAAAAAea8tr55KhbFjfU9ZpQAAAADVvQ7JlvFLIJel/rYAAAAAc3/EJ5BSKDdLb8AAAAAHO/X/IvqUix9w9zwAAAAAczfIZ5BSKDYzoMAAAAAEPILDmvCOH6XrVUAAAAA/K5+eCTSXBbHB7Rvj92AAAADzXV3WPCmkvqFLI4o/dNqPergAAAfG61a2eeyySX3XAUtjsji/Z2L2X9gAAAx9YtXPKr5L777q1AKUttssjj+q2d2z+jAANXtHvxJZL7rq1qAApbbbZZDLvxtAABrrznyJr761rUABQLaWxQX9cvpwApyh+CyJbq1AAAClLIcbc7dEAPOOUuVPfUAAAAtjx/QurAAai6OZMt4AAAApHBjdi/1gBzk19yJLgBQAACyHF6de0gDkj8ZPfUAAUAAsig312xAIeMFsl1aAABQALbbNvt6wB49LKuluuvvvrJW+++5LKkkqmvuuS330lrcAAAAAAAAP/EABQBAQAAAAAAAAAAAAAAAAAAAAD/2gAIAQIQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA/8QAFAEBAAAAAAAAAAAAAAAAAAAAAP/aAAgBAxAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/xAAoEAACAAQFBQEBAAMAAAAAAAABAgAGBxEDBQgSMAQTF0BQIBYQGHD/2gAIAQEAAQIA/wCodXmHXVffUDg17y2pmDjfGd541KzPqD6vqxh9vtnDyvOZW1K08rj8ObpuqbWpcMJs2bNmwoUQyXqllaqnwJlmOotQkRVC7bWtbaVZGQrS7UPlGae9qEqgiKoW1v8ANoIKlWR001VM92v0/oqKo4TBDBxh40iTT7lfZwQKBA4zDjEGkyaPbnbP9yBYHIYaHFA889vVBmmGEhYHIYaHjIsw9vV31KQsDlMNDxiR0T+1q/GHCwOUw0PDLgYXtauegwykDlMNDxKfQ+3qTyXDKQIHIYaHigmU+3nGT5nliQsDkMNDxpLyX3NUFP0hYHIYaMSNM8ve5X+o6QIHIYaMSNLlR/bm/MkZIEDkMNDxTPNPbnvLMOEKkchhy5pflfuVWk9GUg8RJLlzpSkz3NQVLlKFSOIli7SlKkqS17uo+j6MpB4DBLnA6eklJveqPLiFWUg/sli7UFlz4FeZGRlIP6JJdnbStJ/wKw026jpkIIP5JYu0hSZlmXfB1DUWQqR+TBhjl2WUZpT8PUbkykQPwYaHjSN0PxNT+KsLA/BhoeNIXV/Er/mKwv5MNDxpZzX4ec5tnWarA/JhoYSLMfTdT8GaJwrBXRQP2YYMtMK5SRVn3prrPOWqzNc3VQBA/RgghlUybqLlHU30HXexNdVJq1cTTVcIECgADhIIKlSmRzJKmqyVNQuFi+lm2czhqsmytuwIFChQAOSxBUqUKSzPUm6tZRqDz1V1KTFNIQIFCgAW57WttKlGToutoJXfl1KVMRVUKABa3p2tYgqy9F1tPZy4yarTggUAW9kwQwcaRZs46zzGgUAD3DDQ4o9MXHq9z5AsD2b3vcwYeC0q51xaoc5SFIN73ve973ve973ve973ve973uSSxeNN+ccVTM2Vg27fu3bt27du3bt27du3bt27du3bt27dv3795ZmY6Qs04cz6rExAQ4xO53O53O5v7m/f3O53O5v39zudzf3N+/udzudzudzuby0aQOv4cw6A6eDp4/168AGgfgPwP4K8F+DxRLwr4XNHFo74f8SLSY0q8WCmPjfxv48Sn5kT+GElCTf5AyuJXEtfz5yQZLgdF9T/xABOEAABAwICBAYLDAgFBQAAAAABAgMEBQYAEQcSMUAhMFBRYdITFSIyQVZxcoGUoiMkM0JSc5KTssLD0xQWIESRlbHRNENigrMQRlVjcP/aAAgBAQADPwD/AOoU+nNF+ozo8RkbVyHUtJ/iojGjGnFSZF8UkkbQw+H/APi1saH2u4N4IOXNClq/o1jRE8dVu8mU+fFkt/bbGNHVSIRBvWjLWdiFzG21n0LIOGZDaXmHUONqGaVoUFJPkI5HQ2hbjiwlCQVKUo5AAbScWXbK3oNBSqvVBGafe69SIhXS91MaTbkU4hmrikRVZ5M01PYj9ac14nVF9UmoTH5T6trj7inFnylRJ/ZrdCeEiiVebAdzz14r62T7BGNIlBU21WHWK5DG1EpIbfy6HW/vA4szSCtuAw+qnVhWyBMICnPmV7HORKDZNFfrlwzRHit9y2gcLrzngbbT8ZRxdGkZ96IlxdOoAV7nT2V9+Od9XxzxTjDjbzK1IcbUFoWgkKSpJzBBGwjFFj0il0+8qbUlT2WENPzmA26h1SeDXUklJBONH94Ftmg3LEXJXsivEsP+ht3VKuQaVaVDqFwVqQGYUNorWfCo7EoQPCpR4AMVrSRX3atUlluI2VIgwgfc4zPXPxlcaUkKBIIOYIxcNovx6VdTz1WoJIRrrOvLjDnQs8K0j5JxTqzTIdXpUxuTBlNh1l5s5pUk7+u9LiNu0mRnQaQ8pAKDwSZQ4Fu9ITsRuD1ArybHqsgmlVVz3nrngYmnYB0O78bGsl6LTn9SsVkrhxSD3baMvdXR5oO4vRX2ZUdxTbzLiXG1pOSkrQcwR0g4ZvGzqBczRTrzYiFPJGxD6O4dT6Fg76bu0jVVDLutApBNNjc2bJ91V6V7kXoNyWc+5mYy0VGKD8hzJt3fBalnXJcRyC4MB51oHwu6uTY9KiMLdWpxxZWtaipSlHMknhJJ3JVB0q20rXyZnrcp7o5xITkgfTA3ztbozEFCsl1OqRox8xGb/wDVsbmqk1+h1VJyMKoRpIPMWnAvfC3TrFg7EuyZ731SGx9/dC7AhuL2rYbUfSnez2XR8vwatU/A3MrUEJGZJyAwGGGWR/loSj6Iy3sGgWbUQOBioSWPr2wr8PczVbttimJTmZdWhMZfOPJTvhrGiuqvto13KZKjTh5Eq7Ev+CV7maxpYtVBTm3Fcemr6Aw0VJ9rfI1dotWok1PvefEeiuc+q8gpJxLolVqNHntlEqDJdjPJ5ltKKTuQduC6rjcR3MSC1Cb8+SvXP8A1vq6ZXI9+U9j3lU9RidlsblIGSVHocSNy7TaNI1QcRk9V5j8w+YCGUfYz3168LtkUKA+TQ6K8thpIPcvSUdy48fsp3J9b0nR1VX9ZsNrlUtS9qdXhdY++N8XQbRuistHJyBSpkpHntNKWnBUoqUSSTmSeEk7k7R9ItlVBlRBRWIravm3lhpY9KVHfF1SxrwpbQJelUWc02B8tTKgnc3azpHsqA0CdasRXV/NsLDq/ZTvrli39XqH2IoiF8yoJ8Cor51kZeb3u5LlVmrX3La97QG1QoZPxn3h7oR5iN9VfVst1ejMa9eo6FuMoSM1SY54Vs9J8KMFJKVAgg5EHcKve9w0+26KyVyZS8isg6jLQ791fMlIxTLLtyl2zR05RoTOprnvnFnhW4r/UtRzO/U+msTdJNAKIyC8320h5ZILj6wgPt8xKiNcce/NlRoUZGu++6hltI+MtZ1QPSTilaMaMUlSJVblpSZ8z8JvmbTv67osG66E0jWflU53sCed9r3Rv20jBByPHKuXSlbiC3rR6c4am/wBAi8KPb1eQV2Pf9RLDOpS6spdQhEDuR2Q5uNDzFccaTbVTvGW1lIrDnYIufgisddfILOkez36a0EJq8TOTTnT4HgOFsn5DmJMCVIhTWFsyY7qmnmnBqrQtByUkg7CDxlQv+66XbVPCgH3NeS8BmGIyOFxw4h0WmQKTTmQzDhx248dobEttp1RnyEu4G3r6tWJrVZhvOoxGxwy2kbHUc7qOLqNaqESlUmG7KnSnA0yw0nNa1HEXRjQVGUW3rgnhK576eEIA2MNn5KeRKbRdKE9FMhtxm5cOPLcQ0Mkl5zPXXxVOdjXhUFRGTUGX4rSJBQC6hpxKiUA+AEjkUOaUnQD3lKhp+0eKym31Az79mA8B5hdSeRU1LS3dakHNEdcaKPKywhKva4oQdJT9PUeCpUmQyPPaKXv6IPIkGgUmoVeoOhqHCjuSHl8yG06xy6cP16tVetyvh6hMfluDPPJTyysj0Z8Uq0Lyty5BnqQZra3tXaWVdw6B5UE4jzI7EuI8h6O82lxpxB1krQsZhQI2gjkK2rMp6qnctXYhMAHUDhzccI8DaBmpZ6Bio6RdahUdlyBbaHAotq+GlqQcwp3LYkeBHGXNo4LdNdHbSga3DBeXkpnPaWF/FxY+kBlHaOroRP1c1QJWTUpB6EHv/Kjf9HFmJdbqdwsyZqP3OB75e1uY6nAg+eRi5KmHYlm0pqksHMCVJykSuojFWr852p1upSZ0xzvnpDinF+TM7BzDjltLQ60tSFpIUlSTkQRsIIxpFtMMxpk1Nbp6Mh2GoZqdCf8AQ8O7+lnjR7cPYmKyt+gzVZAiWNePn0PI++BiBUorU2mzWJUVwZoeYcS62odCkkg7zYFlhxFeuWI3JR+6sns8n6tvMjHwjFlWx0CXVFfgtdfGkK8w43XLmlKir2xGD2CPlzFDeQV/u3S4rXlfplvVqbT3s8yYzymwrzwOBQ6Di8aX2Ji6qZFrLAyBeb96yfYBQcaM7p7Gyus9qZiv8iqJDA9DuZbw0+2h5h1DjS0hSFoUFJUD4QRudFtyC5Uq7VI0GIja9JcS2jPmGe0nFs0suRbNpTtYfGYEp/OPF9AI1140lXiXG59wuxIa/wB0p+cZrLmOodZY84nGZJO3ebxsx0O2zcMyCM8yyheswrzml5oOJjHYYl80BDyNhm03uF+VTKzkcWffMf8ASLYrkeYQkFxjPUkNee0vJQ3Cm2u9Kt6xm2alVmyW35y+GIwvmTl8KsYuO756qnctYkz5JzyLy80oB8CEDJKB0J36dSpjFQpkx6LLYVrtPsLLbiFc6VJyIxJvB9Fn3i+326DZMKZkEfpgQMyhY2B0cc7ZttsWvRZJarFaQsLWg5LYhbFrHS5sHIMylT4dTp76mJcV5D7DqOBSHGzrJUMRr6s2iXOxqpclMZPtjY3JbOo4jyBQ4wAEk5AYXfV/XBXg5rxC+Y0LmEVg6jeXnd9yEUv3LZMhzuFpFUiDpGTT3GG0tGd1VNtzVlOxf0KMRtDss9izHSkHW5DNr6TLRqZcKGVzkxH/AJqXmyonoGtnxhapVo2y2v8AxMp+e8OhhIbR9s8hqbWlxCilSSFAjgIIwLhta3K6lQPbCmxpSjzKdbCiOLNS0pyIOvmilU6JE6ApYL5/5ORO2eiSgoWvNcB6VCV/sdK0+yscWa3pDvOp62sh2sSw2f8A1NuFCPZHIeeNe2bwpBX/AIWpMyvWWtT8Lik06l1GouEakWK9IV5GkFWFvOOPOqKlrUVKJ2kqOZOMv2en/r07v0/sal0XdTM/h6W1I+oe1PxeKiVWnzqXPaLkSZHdjPoClIKmnUlChrJIIzB2jGhpvZZo9fm/nY0NoyJs1Hr0z83Gh3xLa9cl/m40PIGX6ls+tSvzcaH0bLLY9Zk/mY0Q+JUb6+R18aIgOGyYf1r/AF8aI0An9SIX03j9/GiXxIg/xd62NE/iPT/b62NFA2WNTfoqxopR3X6jUz6Bxoq8RaV9VjRWgcNi0f1cY0XD/sOjerJONF3iFRfVEY0YeIVD9SbxoyTssGg+oNY0ZoGYsGgE9NPZ6uNGniBb38tY6uNGyQANH9t+mlRj9zGjhvPVsC2weilRepjR34hW5/K43UxYHiNb/wDLI3UxYiBkLKoHopsfqYsUHMWZQh0inR+piyfE+ify9jq4s0DIWlRQOiAx1cWgk5ptSjg9EFjq4tPxXpHqTPVxayeAW3S/U2uri2k97btMHkiNdXFuDhFApvqrXVxQP/B0/wBWb/tiiIGqmjwR5I7f9sUZJzTSYQPQwj+2IUZRcjxGWlEZFTbaUnLmzHKv/8QAFBEBAAAAAAAAAAAAAAAAAAAAkP/aAAgBAgEBPwAQP//EABQRAQAAAAAAAAAAAAAAAAAAAJD/2gAIAQMBAT8AED//2Q==";

        public MediaService(IUnitOfWork unitOfWork, ITablesOnline allTables) : this (new FileSystem(), unitOfWork, allTables) { }

        public MediaService(IFileSystem fileSystem, IUnitOfWork unitOfWork, ITablesOnline allTables)
        {
            _fileSystem = fileSystem;
            _unitOfWork = unitOfWork;
            _allTables = allTables;
        }
        
        public async Task<ResultModel<string>> GetProfileImage(Guid playerId)
        {
            Log.Information("GetProfileImage. Start");
            var result = new ResultModel<string>();

            if (await _unitOfWork.Players.PlayerExistsAsync(playerId) is false)
                await GetBotImage(playerId, result);
            else
                await GetPlayerImage(playerId, result);
            
            Log.Information($"GetProfileImage. Result: {JsonSerializer.Serialize(result)}");

            return result;
        }

        public async Task<ResultModel<string>> UpdateProfileImage(Guid playerId, string newProfileImage)
        {
            Log.Information("UpdateProfileImage. Start");
            Log.Information($"UpdateProfileImage. playerId: {playerId}, image: {newProfileImage}");
            
            var result = new ResultModel<string>();
            
            if (newProfileImage is null || playerId == Guid.Empty)
            {
                result.IsSuccess = false;
                result.Message = "Image or Id is null";

                return result;
            }

            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages", $"{playerId.ToString()}");

            if (_fileSystem.File.Exists(path))
                RemoveProfileImage(playerId.ToString());

            await _fileSystem.File.WriteAllTextAsync(path, newProfileImage);
            
            Log.Information($"UpdateProfileImage. Image saved");

            result.IsSuccess = true;
            result.Value = newProfileImage;
            return result;
        }

        public async Task<ResultModel<string>> SetDefaultProfileImage(Guid playerId)
        {
            var result = new ResultModel<string>();
            
            if (playerId == Guid.Empty)
            {
                result.IsSuccess = false;
                result.Message = "Player Id is empty";
                return result;
            }

            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages",
                $"{playerId.ToString()}");
            
            if (_fileSystem.File.Exists(path))
                RemoveProfileImage(playerId.ToString());

            await _fileSystem.File.WriteAllTextAsync(path, DefaultImage);

            result.IsSuccess = true;
            result.Value = DefaultImage;
            return result;
        }

        public async Task<ResultModel<bool>> HasCustomProfileImage(Guid playerId)
        {
            var result = new ResultModel<bool>();
            
            var getImageResult = await GetPlayerImage(playerId);

            if (getImageResult.IsSuccess is false)
            {
                result.IsSuccess = false;
                result.Message = getImageResult.Message;
            }

            if (getImageResult.Value is DefaultImage)
            {
                result.IsSuccess = true;
                result.Value = false;
                return result;
            }

            result.IsSuccess = true;
            result.Value = true;
            return result;
        }

        #region Helpers

        private void RemoveProfileImage(string playerId)
        {
            _fileSystem.File.Delete(Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages", $"{playerId}"));
        }
        
        private async Task GetBotImage(Guid botId, ResultModel<string> result)
        {
            var imageUri = ((Bot)_allTables.GetByPlayerId(botId).Players.First(b => b.Id == botId)).ImageUri;
            using var webClient = new WebClient();
            var image = await webClient.DownloadDataTaskAsync(imageUri);
            
            var newResult = Convert.ToBase64String(image);

            result.IsSuccess = true;
            result.Value = newResult;
        }

        private async Task GetPlayerImage(Guid playerId, ResultModel<string> result)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages", $"{playerId.ToString()}");
            Log.Information($"GetPlayerImage. Path: {path}");

            if (_fileSystem.File.Exists(path) is false)
            {
                result.IsSuccess = false;
                result.Message = $"File {path} doesn't exist";
                return;
            }

            var profileImage = await _fileSystem.File.ReadAllTextAsync(path);

            result.IsSuccess = true;
            result.Value = profileImage;
        }
        
        private async Task<ResultModel<string>> GetPlayerImage(Guid playerId)
        {
            var result = new ResultModel<string>();
            
            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages", $"{playerId.ToString()}");
            Log.Information($"GetPlayerImage. Path: {path}");

            if (_fileSystem.File.Exists(path) is false)
            {
                result.IsSuccess = false;
                result.Message = $"File {path} doesn't exist";
                return result;
            }

            var profileImage = await _fileSystem.File.ReadAllTextAsync(path);

            result.IsSuccess = true;
            result.Value = profileImage;
            return result;
        }
        
        #endregion
    }
}