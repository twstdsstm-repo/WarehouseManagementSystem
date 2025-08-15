using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;

namespace Warehouse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptResourcesController : ControllerBase
    {
        private readonly IReceiptResourceService _service;

        public ReceiptResourcesController(IReceiptResourceService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ReceiptResource>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ReceiptResource), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpGet("by-document/{documentId:int}")]
        [ProducesResponseType(typeof(IEnumerable<ReceiptResource>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByDocument(int documentId)
        {
            var items = await _service.GetByDocumentIdAsync(documentId);
            return Ok(items);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReceiptResource), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ReceiptResource item)
        {
            var id = await _service.CreateAsync(item);
            return CreatedAtAction(nameof(Get), new { id }, item);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ReceiptResource item)
        {
            if (item == null) return BadRequest("Пустое тело запроса.");
            if (item.Id != 0 && item.Id != id)
                return BadRequest("Идентификатор в пути и теле запроса не совпадают.");

            var result = await _service.UpdateAsync(id, item);
            return result ? NoContent() : NotFound();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}
