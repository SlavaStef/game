// using System;
// using System.IO;
// using System.IO.Abstractions.TestingHelpers;
// using System.Text.Json;
// using FluentAssertions;
// using PokerHand.BusinessLogic.Services;
// using PokerHand.Common.Helpers.Media;
// using Xunit;
//
// namespace PokerHand.BusinessLogic.Tests.Services
// {
//     public class MediaServiceTests
//     {
//         [Fact]
//         public async void GetProfileImage_ReturnsImage_IfItExists()
//         {
//             // Arrange
//             var mockFileSystem = new MockFileSystem();
//
//             var playerId = Guid.NewGuid();
//             
//             var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages",
//                 $"{playerId}.jpg");
//             var file = new byte[] {1, 2, 3, 4, 5};
//             var mockFile = new MockFileData(file);
//             mockFileSystem.AddFile(path, mockFile);
//             
//             var sut = new MediaService(mockFileSystem);
//
//             // Act
//             var result = await sut.GetProfileImage(playerId);
//             
//             // Assert
//             result.IsSuccess.Should().BeTrue();
//             result.Value.Should().BeEquivalentTo(file);
//         }
//         
//         [Fact]
//         public async void GetProfileImage_ReturnsError_Message_IfItDoesNotExist()
//         {
//             // Arrange
//             var mockFileSystem = new MockFileSystem();
//
//             var playerId = Guid.NewGuid();
//
//             var sut = new MediaService(mockFileSystem);
//
//             // Act
//             var result = await sut.GetProfileImage(playerId);
//             
//             // Assert
//             result.IsSuccess.Should().BeFalse();
//             result.Message.Should().NotBeNull();
//         }
//         
//         [Fact]
//         public async void UpdateProfileImage_ReturnsTrue_IfImageIsOk()
//         {
//             // Arrange
//             var mockFileSystem = new MockFileSystem();
//
//             var playerIdGuid = Guid.NewGuid();
//             
//             var file = new Image
//             {
//                 PlayerId = playerIdGuid,
//                 BinaryImage = new byte[] {1, 2, 3, 4, 5}
//             
//             };
//             
//             var mockFile = new MockFileData(new byte[] {1, 1, 1});
//             
//             var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages",
//                 $"{playerIdGuid.ToString()}.jpg");
//             
//             mockFileSystem.AddFile(path, mockFile);
//             
//             var sut = new MediaService(mockFileSystem);
//
//             // Act
//             var result = await sut.UpdateProfileImage(JsonSerializer.Serialize(file));
//             
//             // Assert
//             result.IsSuccess.Should().BeTrue();
//         }
//         
//         [Fact]
//         public async void UpdateProfileImage_ReturnsFalse_IfImageIsNull()
//         {
//             // Arrange
//             var sut = new MediaService();
//
//             // Act
//             var result = await sut.UpdateProfileImage(JsonSerializer.Serialize(new Image()));
//             
//             // Assert
//             result.IsSuccess.Should().BeFalse();
//         }
//         
//         [Fact]
//         public async void UpdateProfileImage_ReturnsFalse_IfPlayerIdIsEmpty()
//         {
//             // Arrange
//             var sut = new MediaService();
//
//             // Act
//             var result =
//                 await sut.UpdateProfileImage(JsonSerializer.Serialize(new Image {BinaryImage = new byte[] {1, 2, 3}}));
//             
//             // Assert
//             result.IsSuccess.Should().BeFalse();
//         }
//
//         [Fact]
//         public async void SetDefaultProfileImage_CopiesFile_IfPlayerIdIsOk_And_SourceFileExists_And_DestFileExists()
//         {
//             // Arrange
//             var mockFileSystem = new MockFileSystem();
//             
//             var sourceFile = new MockFileData(new byte[] {1, 2, 3});
//             var sourceFilePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages", "default.jpg");
//             mockFileSystem.AddFile(sourceFilePath, sourceFile);
//
//             var destFile = new MockFileData(new byte[] {3, 2, 1});
//             var playerId = Guid.NewGuid();
//             var destFilePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "profileImages",
//                 $"{playerId.ToString()}.jpg");
//             mockFileSystem.AddFile(destFilePath, destFile);
//
//             var sut = new MediaService(mockFileSystem);
//
//             // Act
//             var result = await sut.SetDefaultProfileImage(playerId);
//             
//             // Assert
//             result.IsSuccess.Should().BeTrue();
//             mockFileSystem.GetFile(destFilePath).Should().BeEquivalentTo(sourceFile);
//         }
//         
//         [Fact]
//         public async void SetDefaultProfileImage_ReturnsFalse_IfSourceFileDoesntExist()
//         {
//             // Arrange
//             var mockFileSystem = new MockFileSystem();
//             
//             var sut = new MediaService(mockFileSystem);
//
//             // Act
//             var result = await sut.SetDefaultProfileImage(Guid.NewGuid());
//             
//             // Assert
//             result.IsSuccess.Should().BeFalse();
//             result.Message.Should().Be("Source file not found");
//         }
//         
//         [Fact]
//         public async void SetDefaultProfileImage_ReturnsFalse_IfPlayerIdIsEmpty()
//         {
//             // Arrange
//             var sut = new MediaService();
//
//             // Act
//             var result = await sut.SetDefaultProfileImage(Guid.Empty);
//             
//             // Assert
//             result.IsSuccess.Should().BeFalse();
//         }
//     }
// }