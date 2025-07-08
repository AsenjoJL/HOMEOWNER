using HOMEOWNER.Data;
using HOMEOWNER.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ForumController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public ForumController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
    {
        _context = context;
        _hostingEnvironment = hostingEnvironment;
    }

    // Original Index action (for standalone forum page)
    public async Task<IActionResult> Index()
    {
        var posts = await GetForumPosts();
        return View(posts);
    }

    // New action for embedded forum content
    public async Task<IActionResult> Embedded()
    {
        var posts = await GetForumPosts();
        return PartialView("_ForumPartial", posts);
    }

    private async Task<List<ForumPost>> GetForumPosts()
    {
        var posts = await _context.ForumPosts
            .Include(p => p.Homeowner)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Homeowner)
            .Include(p => p.Reactions)
                .ThenInclude(r => r.Homeowner)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var settings = await _context.CommunitySettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new CommunitySettings();
            _context.CommunitySettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        ViewBag.CommunitySettings = settings;
        return posts;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromForm] string title, [FromForm] string content,
    IFormFile mediaFile, IFormFile musicFile)
    {
        // 1. Validate required fields
        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest("Post content is required.");
        }

        var homeownerId = HttpContext.Session.GetInt32("HomeownerID");
        if (homeownerId == null) return Unauthorized();

        // 2. Create post with validated content
        var post = new ForumPost
        {
            Title = title ?? "", // Handle null title (column is NOT NULL in DB)
            Content = content,   // Guaranteed not null due to validation
            HomeownerID = homeownerId.Value,
            CreatedAt = DateTime.Now
        };

        // 3. Handle media file (optional)
        if (mediaFile != null && mediaFile.Length > 0)
        {
            const int maxMediaSize = 20 * 1024 * 1024; // 20MB
            if (mediaFile.Length > maxMediaSize)
            {
                return BadRequest("Media file exceeds 20MB limit.");
            }

            post.MediaUrl = await UploadFile(mediaFile, "media");
            post.MediaType = mediaFile.ContentType.StartsWith("image") ? "image" :
                             mediaFile.ContentType.StartsWith("video") ? "video" : null;
        }

        // 4. Handle music file (optional)
        if (musicFile != null && musicFile.Length > 0)
        {
            const int maxMusicSize = 10 * 1024 * 1024; // 10MB
            if (musicFile.Length > maxMusicSize)
            {
                return BadRequest("Music file exceeds 10MB limit.");
            }

            post.MusicUrl = await UploadFile(musicFile, "music");
            post.MusicTitle = Path.GetFileNameWithoutExtension(musicFile.FileName);
        }

        // 5. Save to database
        try
        {
            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();
            return Ok(new { PostId = post.ForumPostID });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error creating post: " + ex.Message);
        }
    }


    [HttpPost]
    public async Task<IActionResult> AddComment(int postId, string commentText, IFormFile mediaFile)
    {
        var homeownerId = HttpContext.Session.GetInt32("HomeownerID");
        if (homeownerId == null) return Unauthorized();

        var comment = new ForumComment
        {
            ForumPostID = postId,
            HomeownerID = homeownerId.Value,
            CommentText = commentText,
            CreatedAt = DateTime.Now
        };

        if (mediaFile != null && mediaFile.Length > 0)
        {
            var mediaPath = await UploadFile(mediaFile, "comments");
            comment.MediaUrl = mediaPath;
        }

        _context.ForumComments.Add(comment);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> AddReaction(int postId, string reactionType)
    {
        var homeownerId = HttpContext.Session.GetInt32("HomeownerID");
        if (homeownerId == null) return Unauthorized();

        // Check if the user already reacted to this post
        var existingReaction = await _context.Reactions
            .FirstOrDefaultAsync(r => r.ForumPostID == postId && r.HomeownerID == homeownerId.Value);

        if (existingReaction != null)
        {
            // Update existing reaction
            existingReaction.ReactionType = reactionType;
            existingReaction.CreatedAt = DateTime.Now;
        }
        else
        {
            // Create new reaction
            var reaction = new Reaction
            {
                ForumPostID = postId,
                HomeownerID = homeownerId.Value,
                ReactionType = reactionType,
                CreatedAt = DateTime.Now
            };
            _context.Reactions.Add(reaction);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    // ... (keep all other existing methods unchanged)

    private async Task<string> UploadFile(IFormFile file, string subfolder)
    {
        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", subfolder);
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/uploads/{subfolder}/{uniqueFileName}".Replace("\\", "/");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateBackground(IFormFile backgroundImage, string customCSS)
    {
        var settings = await _context.CommunitySettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new CommunitySettings();
            _context.CommunitySettings.Add(settings);
        }

        if (backgroundImage != null && backgroundImage.Length > 0)
        {
            var bgPath = await UploadFile(backgroundImage, "backgrounds");
            settings.BackgroundImageUrl = bgPath;
        }

        if (!string.IsNullOrEmpty(customCSS))
        {
            settings.CustomCSS = customCSS;
        }

        settings.LastUpdated = DateTime.Now;
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> SetFeaturedMusic(IFormFile musicFile)
    {
        var settings = await _context.CommunitySettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new CommunitySettings();
            _context.CommunitySettings.Add(settings);
        }

        if (musicFile != null && musicFile.Length > 0)
        {
            var musicPath = await UploadFile(musicFile, "featured-music");
            settings.FeaturedMusicUrl = musicPath;
            settings.LastUpdated = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        return Json(new { success = true, musicUrl = settings.FeaturedMusicUrl });
    }

}