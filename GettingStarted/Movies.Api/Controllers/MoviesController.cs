﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Models;
using Movies.Application.Repository;
using Movies.Application.Service;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers
{

    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }


        [HttpPost(ApiEndpoints.Movies.Create)]
        public async Task<IActionResult> CreateMovie([FromBody] CreateMoviesRequest request)
        {
            var movie = request.MapToMovie();
            var rssult = await _movieService.CreateAsync(movie);

            return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie);
            //return Created($"/{ApiEndpoints.Movies.Create}/{movie.Id}", movie);
        }


        [HttpGet(ApiEndpoints.Movies.Get)]
        public async Task<IActionResult> Get([FromRoute] string idOrSlug)
        {

            var movie = Guid.TryParse(idOrSlug, out var id)
                ? await _movieService.GetByIdAsync(id)
                : await _movieService.GetBySlugAsync(idOrSlug);
            if (movie is null)
            {
                return NotFound();
            }
            var response = movie.MapToResponse();
            return Ok(response);
        }


        [HttpGet(ApiEndpoints.Movies.GetAll)]
        public async Task<IActionResult> Get()
        {

            var movie = await _movieService.GetAllAsync();

            var response = movie.MapToResponse();

            if (movie is null)
            {
                return NotFound();
            }
            return Ok(response);
        }


        [HttpPut(ApiEndpoints.Movies.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request)
        {
            var movie = request.MapToMovie(id);
            var updatedMovie = await _movieService.UpdateAsync(movie);
            if(updatedMovie is null)
                return NotFound();

            var response = movie.MapToResponse();
            return Ok(response);

        }

        [HttpDelete(ApiEndpoints.Movies.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var deleteMovie = await _movieService.DeleteByIdAsync(id);
            if (!deleteMovie)
                return NotFound();

            return Ok();

        }



    }
}
