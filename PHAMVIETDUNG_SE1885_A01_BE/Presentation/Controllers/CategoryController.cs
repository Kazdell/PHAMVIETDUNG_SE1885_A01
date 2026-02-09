using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A01_BE.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [EnableQuery(MaxTop = 100, PageSize = 20)]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_service.GetCategoriesWithCounts());
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var category = _service.GetCategoryById(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Category category)
        {
            try
            {
                _service.CreateCategory(category);
                return CreatedAtAction(nameof(Get), new { id = category.CategoryId }, category);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Category category)
        {
            if (id != category.CategoryId) return BadRequest("ID mismatch");
            try
            {
                _service.UpdateCategory(category);
                return Ok(category);
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
                _service.DeleteCategory(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
