namespace MyOrderProjectAPI.Controller
{
    using global::MyOrderProjectAPI.DTOs;
    using global::MyOrderProjectAPI.Models;
    using global::MyOrderProjectAPI.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    namespace MyOrderProjectAPI.Controllers
    {
        [Authorize]
        [Route("api/[controller]")]
        [ApiController]
        public class TablesController : ControllerBase
        {
            private readonly ITableService _tableService;

            //Dependency Injection
            public TablesController(ITableService tableService)
            {
                _tableService = tableService;
            }


            [HttpGet]
            public async Task<ActionResult<IEnumerable<TableDetailDTO>>> GetTables()
            {
                var tables = await _tableService.GetAllTablesAsync();
                return Ok(tables); // 200 
            }


            [HttpGet("{id}")]
            public async Task<ActionResult<TableDetailDTO>> GetTable(int id)
            {
                var table = await _tableService.GetTableByIdAsync(id);

                if (table == null)
                {
                    return NotFound(); // 404 
                }

                return Ok(table); // 200 
            }


            [HttpPost]
            public async Task<ActionResult<TableDetailDTO>> PostTable([FromBody] TableCreateUpdateDTO tableDTO)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);//400
                }

                // DÜZELTME: Try-catch bloğu kaldırıldı. 
                // DbUpdateException ve genel Exception hataları Global Handler'a fırlatılacaktır.
                var newTable = await _tableService.CreateTableAsync(tableDTO);

                // 201 response code ve URI döndürme işlemi
                return CreatedAtAction(nameof(GetTable), new { id = newTable.Id }, newTable);
            }


            [HttpPut("{id}")]
            public async Task<IActionResult> PutTable(int id, [FromBody] TableCreateUpdateDTO tableDTO)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _tableService.UpdateTableAsync(id, tableDTO);

                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }


            [HttpPatch("{id}/status")]
            public async Task<IActionResult> UpdateStatus(int id, [FromBody] Status newStatus)
            {
                // DÜZELTME: Try-catch bloğu kaldırıldı.
                // ArgumentException Global Handler'a fırlatılacaktır.
                var success = await _tableService.UpdateTableStatusAsync(id, newStatus);

                if (!success)
                {
                    return NotFound(); // 404
                }

                return NoContent(); // 204
            }


            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteTable(int id)
            {
                // DÜZELTME: Try-catch bloğu kaldırıldı.
                // DbUpdateException ve genel hatalar Global Handler'a fırlatılacaktır.
                var success = await _tableService.DeleteTableAsync(id);

                if (!success)
                {
                    return NotFound(new { message = $"ID'si {id} olan masa bulunamadı." });
                }

                return NoContent(); // 204 No Content
            }


            [HttpPost("{id}/restore")]
            [ProducesResponseType(StatusCodes.Status204NoContent)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            public async Task<IActionResult> RestoreTable(int id)
            {
                // DÜZELTME: Try-catch bloğu kaldırıldı.
                // InvalidOperationException ve genel hatalar Global Handler'a fırlatılacaktır.
                var success = await _tableService.RestoreTableAsync(id);

                if (!success)
                {
                    return NotFound(new { message = $"ID'si {id} olan masa bulunamadı." }); // 404
                }

                return NoContent(); // 204 
            }
        }
    }
}