using Microsoft.AspNetCore.Mvc;
using StajP.DTOs;
using StajP.Interfaces;
using System.Collections.Generic;

namespace StajP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObjectController : ControllerBase
    {
        private readonly IObjectService _objectService;

        public ObjectController(IObjectService objectService)
        {
            _objectService = objectService;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_objectService.GetAll());

        [HttpGet("{id}")]
        public IActionResult GetById(int id) => Ok(_objectService.GetById(id));

        [HttpPost]
        public IActionResult Add([FromBody] ObjectDto dto) => Ok(_objectService.Add(dto));

        [HttpPost("addrange")]
        public IActionResult AddRange([FromBody] List<ObjectDto> dtos) => Ok(_objectService.AddRange(dtos));

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] ObjectDto dto) => Ok(_objectService.Update(id, dto));

        [HttpDelete("{id}")]
        public IActionResult Delete(int id) => Ok(_objectService.Delete(id));
    }
}