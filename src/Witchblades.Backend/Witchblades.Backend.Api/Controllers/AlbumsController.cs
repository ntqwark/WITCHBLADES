﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Witchblades.Backend.Api.DataContracts.ViewModels;
using Witchblades.Backend.Data;
using AutoMapper;

namespace Witchblades.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlbumsController : ControllerBase
    {
        #region Private fields
        private readonly WitchbladesContext _context;
        private readonly IMapper _mapper;
        #endregion

        #region Constructors
        public AlbumsController(
            WitchbladesContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        #endregion


        #region GET: api/Albums
        /// <summary>
        /// Returns all albums
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Album>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> Get()
        {
            var albums = await _context.Albums
                .Include(t => t.Artist)
                .OrderByDescending(t => t.ReleaseDate)
                .Select(t => _mapper.Map<Album>(t))
                .ToListAsync();

            if (albums is null || albums.Count == 0)
            {
                return NoContent();
            }

            return Ok(albums);
        }
        #endregion

        #region GET: api/Albums/{id}
        /// <summary>
        /// Returns the album with specified id
        /// </summary>
        /// <param name="id">Album GUID</param>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Album))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Get(Guid id)
        {
            var album = await _context.Albums
                .Include(t => t.Tracks)
                .Include("Tracks.TrackArtists")
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (album == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<Album>(album));
        }
        #endregion

        #region PUT: api/Albums/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Album album)
        {
            _context.Entry(album).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {

            }

            return NoContent();
        }
        #endregion

        #region POST: api/Albums
        /// <summary>
        /// Creates an album
        /// </summary>
        /// <response code="424">Failed Dependency Error</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Album))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ProblemDetails))]
        public async Task<ActionResult> Post(AlbumCreate album)
        {
            var newAlbum = new Models.Album();

            // Creating a new album

            // Check artist
            {
                var artist = await _context.Artists.FirstOrDefaultAsync(t => t.Id == album.Artist);

                if (artist is null)
                {
                    return Problem($"Artist with id '{album.Artist}' not found",
                        "Artist", 424, "Failed dependency error", "Artist");
                }
                else
                {
                    newAlbum.Artist = artist;
                }
            }

            // Check tracks
            {
                if (album.Tracks != null && album.Tracks.Any())
                {
                    var tracks = new List<Models.Track>(album.Tracks.Count());

                    foreach (var trackId in album.Tracks)
                    {
                        var found = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == trackId);

                        if (found is null)
                        {
                            return Problem($"Track with id '{trackId}' not found",
                                "Artist", 424, "Failed dependency error", "Artist");
                        }
                        else
                        {
                            tracks.Add(found);
                        }
                    }

                    newAlbum.Tracks = tracks;
                }
            }

            {
                newAlbum.ReleaseDate = album.ReleaseDate;
                newAlbum.AlbumName = album.AlbumName;
                newAlbum.AlbumImage = album.AlbumImage;
            }
            
            _context.Albums.Add(newAlbum);

            return CreatedAtAction("GetAlbum", _mapper.Map<Album>(newAlbum));
        }
        #endregion

        #region DELETE: api/Albums/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlbum(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }

            _context.Albums.Remove(album);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}
