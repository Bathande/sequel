﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using MyTimesheet.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTimesheet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimesheetController : ControllerBase
    {
        
        private readonly TimesheetContext _db;
        readonly IConfiguration _config;

        public IConfiguration Config => _config;

        public TimesheetController(TimesheetContext context,IConfiguration _config)
        {
            _db = context;
            this._config = _config;

        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimesheetEntry>>> Get()
        {
            return await _db.Entries.ToListAsync();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TimesheetEntry>> Get(int id)
        {
            return await _db.Entries.FindAsync(id);
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] TimesheetEntry value)
        {
            await _db.Entries.AddAsync(value);
            await _db.SaveChangesAsync();
            var cacheConnection = Config.GetValue<String>("CacheConnection").ToString();
               
            var lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {

                return ConnectionMultiplexer.Connect(cacheConnection);
            });
            IDatabase cache = lazyConnection.Value.GetDatabase();
            await cache.SetAddAsync($"{value.Name}--{value.Surname}",value.ToString());
            //var cacheItem=await cache.StringGetAsync($"{value.Name}--{value.Surname}");
            
            var cacheitem=cache.Execute("KEYS","LIST").ToString();
           // lazyConnection.Value.Dispose();
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] TimesheetEntry value)
        {
            var entry = await _db.Entries.FindAsync(id);
            entry = value;
            await _db.SaveChangesAsync();

             ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
             IDatabase data = redis.GetDatabase();
                

        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var entry = await _db.Entries.FindAsync(id);
            _db.Entries.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}
