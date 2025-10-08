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

            [Authorize]
            [HttpGet]
            public async Task<ActionResult<IEnumerable<TableDetailDTO>>> GetTables()
            {
                var tables = await _tableService.GetAllTablesAsync();
                return Ok(tables); // 200 
            }

            [Authorize]
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

            [Authorize]
            [HttpPost]
            public async Task<ActionResult<TableDetailDTO>> PostTable([FromBody] TableCreateUpdateDTO tableDTO)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);//400
                }

                try
                {
                    var newTable = await _tableService.CreateTableAsync(tableDTO);

                    // 201 response code ve URI döndürme işlemi
                    return CreatedAtAction(nameof(GetTable), new { id = newTable.Id }, newTable);
                }
                catch (DbUpdateException ex)
                {
                    return Conflict(new { message = "Veritabanında yeni oluşturulurken bir hata oluştu", ex.Message }); // 409 
                }
                catch (Exception)
                {
                    return StatusCode(500, "Masa oluşturulurken beklenmedik bir hata oluştu.");
                }
            }

            [Authorize]
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

            [Authorize]
            [HttpPatch("{id}/status")]
            public async Task<IActionResult> UpdateStatus(int id, [FromBody] Status newStatus)
            {
                try
                {
                    var success = await _tableService.UpdateTableStatusAsync(id, newStatus);

                    if (!success)
                    {
                        return NotFound(); // 404
                    }

                    return NoContent(); // 204
                }
                catch (ArgumentException ex)
                {
                    // Geçersiz durum değeri gönderilirse
                    return BadRequest(new { message = ex.Message }); // 400
                }
            }

            [Authorize]
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteTable(int id)
            {
                try
                {
                    var success = await _tableService.DeleteTableAsync(id);

                    if (!success)
                    {
                        return NotFound(new { message = $"ID'si {id} olan masa bulunamadı." });
                    }
                }
                catch (DbUpdateException ex)
                {
                    return StatusCode(500, new { message = $"Id değeri {id} olan kayıt siliniken bir hata oluştu.", error = ex.Message });
                }


                return NoContent(); // 204 No Content
            }

            [Authorize]
            [HttpPost("{id}/restore")]
            [ProducesResponseType(StatusCodes.Status204NoContent)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            public async Task<IActionResult> RestoreTable(int id)
            {
                try
                {
                    var success = await _tableService.RestoreTableAsync(id);
                    if (!success)
                    {
                        return NotFound(new { message = $"ID'si {id} olan masa bulunamadı." }); // 404
                    }
                }
                catch (InvalidOperationException ex)
                {
                    return StatusCode(500, new { message = $"Id değeri {id} olan kayıt geri getirilirken bir hata oluştu.", error = ex.Message });
                }

                return NoContent(); // 204 
            }
        }
    }
}
