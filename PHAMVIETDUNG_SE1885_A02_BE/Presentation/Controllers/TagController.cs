using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.Services;
using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A02_BE.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService _service;

        public TagController(ITagService service)
        {
            _service = service;
        }

        [EnableQuery(MaxTop = 100, PageSize = 20)]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_service.GetAllTags());
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var tag = _service.GetTagById(id);
            if (tag == null) return NotFound();
            return Ok(tag);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Tag tag)
        {
            try
            {
                _service.CreateTag(tag);
                return CreatedAtAction(nameof(Get), new { id = tag.TagId }, tag);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Tag tag)
        {
            if (id != tag.TagId) return BadRequest("ID mismatch");
            try
            {
                _service.UpdateTag(tag);
                return Ok(tag);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _service.DeleteTag(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
