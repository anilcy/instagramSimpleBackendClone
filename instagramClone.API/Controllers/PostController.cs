using instagramClone.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using instagramClone.Business.Interfaces;

namespace instagramClone.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        // POST: api/posts
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] PostCreateDto dto)
        {
            // Claim'leri filtreleme
            var nameIdentifierClaims = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).ToList();

            if (nameIdentifierClaims.Count == 0)
            {
                Console.WriteLine("User ID claim not found.");
                return BadRequest("Invalid user ID.");
            }

            var userIdString = nameIdentifierClaims.First().Value;
            Console.WriteLine($"userIdString değeri: {userIdString}"); // Debug satırı

            // Guid.TryParse
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                Console.WriteLine($"Guid.TryParse failed for: {userIdString}");
                return BadRequest("Invalid user ID format.");
            }
            Console.WriteLine($"Guid.TryParse user id: {userId}");
            // Tüm claim'leri incele (Debug için)
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }

            var result = await _postService.CreatePostAsync(dto, userId);
            return Ok(result);
        }

        // GET: api/posts?page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("Invalid user ID.");
            }
            var posts = await _postService.GetPostsAsync(userId, page, pageSize);
            return Ok(posts);
        }
        // To get the details of a post
        // GET: api/posts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("Invalid user ID.");
            }
            var post = await _postService.GetPostByIdAsync(id, userId);
            return Ok(post);
        }

        // PUT: api/posts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromForm] PostUpdateDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("Invalid user ID.");
            }
            var updatedPost = await _postService.UpdatePostAsync(id, dto, userId);
            return Ok(updatedPost);
        }

        // DELETE: api/posts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("Invalid user ID.");
            }
            var success = await _postService.DeletePostAsync(id, userId);
            if (!success)
                return NotFound("Post not found or already deleted.");
            return Ok("Post deleted successfully.");
        }
    }
}
