using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaPlatform.Entities.Dtos.CommentDtos;

namespace SocialMediaPlatform.API.Controllers
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
        
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CommentCreateDto dto)
        {
            var commentDto = await _commentService.AddCommentAsync(dto, CurrentUserId);
            return Ok(commentDto);
        }
        
        [AllowAnonymous]
        [HttpGet("{postId}")]
        public async Task<IActionResult> GetComments(Guid postId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var comments = await _commentService.GetCommentsByPostIdAsync(postId, page, pageSize);
            return Ok(comments);
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
         await _commentService.DeleteCommentAsync(commentId, CurrentUserId);
         return NoContent();
        }
    }
}