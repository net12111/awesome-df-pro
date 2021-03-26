﻿// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Api
{
    public class ToDo
    {
        static int _nextId = 1;
        public static int NewId()
        {
            return _nextId++;
        }
        
        public int Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Name { get; set; }
        public string User { get; set; }
    }

    [Authorize("RequireInteractiveUser")]
    public class ToDoController : ControllerBase
    {
        private static readonly List<ToDo> __data = new List<ToDo>()
        {
            new ToDo { Id = ToDo.NewId(), Date = DateTimeOffset.UtcNow, Name = "Demo ToDo API", User = "bob" },
            new ToDo { Id = ToDo.NewId(), Date = DateTimeOffset.UtcNow.AddHours(1), Name = "Stop Demo", User = "bob" },
            new ToDo { Id = ToDo.NewId(), Date = DateTimeOffset.UtcNow.AddHours(4), Name = "Have Dinner", User = "alice" },
        };

        [HttpGet("todos")]
        public IActionResult GetAll()
        {
            return Ok(__data.AsEnumerable());
        }

        [HttpGet("todos/{id}")]
        public IActionResult Get(int id)
        {
            var item = __data.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();
            
            return Ok(item);
        }

        [HttpPost("todos")]
        public IActionResult Post([FromBody] ToDo model)
        {
            model.Id = ToDo.NewId();
            model.User = $"{User.FindFirst("sub").Value} ({User.FindFirst("name").Value})";
            
            __data.Add(model);

            return Created(Url.Action(nameof(Get), new { id = model.Id }), model);
        }

        [HttpPut("todos/{id}")]
        public IActionResult Put(int id, [FromBody] ToDo model)
        {
            var item = __data.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();

            item.Date = model.Date;
            item.Name = model.Name;

            return NoContent();
        }
        
        [HttpDelete("todos/{id}")]
        public IActionResult Delete(int id)
        {
            var item = __data.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();

            __data.Remove(item);

            return NoContent();
        }
    }
}
