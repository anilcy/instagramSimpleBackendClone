using instagramClone.Business.Interfaces;
using instagramClone.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace instagramClone.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : BaseController
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // Yeni yorum ekleme: POST: api/comments
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentDto dto)
        {
            var commentDto = await _commentService.AddCommentAsync(dto, CurrentUserId);
            return Ok(commentDto);
        }

        // Belirli bir postun yorumlarını getir: GET: api/comments/{postId}
        [HttpGet("{postId}")]
        public async Task<IActionResult> GetComments(int postId)
        {
            var comments = await _commentService.GetCommentsByPostIdAsync(postId);
            return Ok(comments);
        }
    }
}