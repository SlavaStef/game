// using System.Threading.Tasks;
// using Google.Apis.Auth.AspNetCore3;
// using Google.Apis.Auth.OAuth2;
// using Google.Apis.CloudIdentity;
// using Google.Apis.CloudIdentity.v1;
// using Google.Apis.PeopleService.v1;
// using Google.Apis.Plus.v1;
// using Google.Apis.Services;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authentication.Cookies;
// using Microsoft.AspNetCore.Mvc;
//
// namespace PokerHand.Server.Controllers
// {
//     [ApiController]
//     public class AuthorizationController : ControllerBase
//     {
//         [HttpGet]
//         [GoogleScopedAuthorize()]
//         public async Task<IActionResult> GetUserInfo([FromServices] IGoogleAuthProvider auth)
//         {
//             GoogleCredential credentials = await auth.GetCredentialAsync();
//             
//             var service = new PeopleServiceService(new BaseClientService.Initializer
//             {
//                 HttpClientInitializer = credentials
//             });
//             
//             var info = service.
//         }
//
//         public async Task<IActionResult> Logout()
//         {
//             if (User.Identity.IsAuthenticated)
//                 await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
//
//             return Ok();
//         }
//     }
// }