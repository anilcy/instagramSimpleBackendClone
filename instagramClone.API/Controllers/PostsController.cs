using instagramClone.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using instagramClone.Business.Interfaces;

namespace instagramClone.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : BaseController
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
            var result = await _postService.CreatePostAsync(dto, CurrentUserId);
            return Ok(result);
        }
 
        // GET: api/posts?userId={userId}&page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] Guid? userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var currentUserId = CurrentUserId;
            var targetUserId = userId ?? currentUserId;
            var posts = await _postService.GetPostsAsync(targetUserId, currentUserId, page, pageSize);
            return Ok(posts);
        }
        // To get the details of a post
        // GET: api/posts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            var post = await _postService.GetPostByIdAsync(id, CurrentUserId);
            return Ok(post);
        }

        // PUT: api/posts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromForm] PostUpdateDto dto)
        {
            var updatedPost = await _postService.UpdatePostAsync(id, dto, CurrentUserId);
            return Ok(updatedPost);
        }

        // DELETE: api/posts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var success = await _postService.DeletePostAsync(id, CurrentUserId);
            if (!success)
                return NotFound("Post not found or already deleted.");
            return Ok("Post deleted successfully.");
        }
    }
}
