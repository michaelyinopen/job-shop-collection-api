using AutoMapper;
using AutoMapper.QueryableExtensions;
using job_shop_collection_api.Data;
using job_shop_collection_api.Data.Models;
using job_shop_collection_api.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace job_shop_collection_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobSetsController : ControllerBase
    {
        private JobShopCollectionDbContext JobShopCollectionDbContext { get; }
        private IMapper Mapper { get; }

        public JobSetsController(
            JobShopCollectionDbContext jobShopCollectionDbContext,
            IMapper mapper)
        {
            JobShopCollectionDbContext = jobShopCollectionDbContext;
            Mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<JobSetHeadersDto>> GetAll([FromQuery] JobSetsQuery jobSetsQuery)
        {
            IQueryable<JobSet> dataQuery = JobShopCollectionDbContext.JobSet;

            if (jobSetsQuery.PageToken != null)
            {
                dataQuery = dataQuery.Where(j => j.Id < jobSetsQuery.PageToken);
            }

            var data = await dataQuery
                .OrderByDescending(j => j.Id)
                .Take(jobSetsQuery.Limit)
                .ProjectTo<JobSetHeaderDto>(Mapper.ConfigurationProvider)
                .ToListAsync();

            int? nextPageToken = data.Count == jobSetsQuery.Limit ? data[^1].Id : default(int?);
            return new JobSetHeadersDto
            {
                Data = data,
                NextPageToken = nextPageToken
            };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JobSetDto>> Get(int id)
        {
            var result = await JobShopCollectionDbContext.JobSet
                .ProjectTo<JobSetDto>(Mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (result is null)
                return NotFound();

            string? eTag = result.GetETag();
            if (eTag != null)
            {
                if (HttpContext.Request.Headers.Keys.Contains("If-None-Match") && HttpContext.Request.Headers["If-None-Match"].ToString() == eTag)
                {
                    return new StatusCodeResult(304);//Not Modified
                }
                HttpContext.Response.Headers.Add("ETag", new[] { eTag });
            }

            return result;
        }

        [HttpPost]
        public async Task<ActionResult<JobSetDto>> Post([FromBody] NewJobSetDto newJobSetDto)
        {
            var jobSet = Mapper.Map<JobSet>(newJobSetDto);
            JobShopCollectionDbContext.JobSet.Add(jobSet);
            await JobShopCollectionDbContext.SaveChangesAsync();

            var result = Mapper.Map<JobSetDto>(jobSet);
            string? eTag = jobSet.GetETag();
            if (eTag != null)
            {
                HttpContext.Response.Headers.Add("ETag", new[] { eTag });
            }
            return CreatedAtAction("Get", new { id = jobSet.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<JobSetDto>> Put(int id, [FromBody] UpdateJobSetDto updateJobSetDto)
        {
            if (!id.Equals(updateJobSetDto.Id))
                return BadRequest(new { Message = "The Id in route does not equal to the Id in Body." });

            var newJobSet = Mapper.Map<JobSet>(updateJobSetDto);

            var current = await JobShopCollectionDbContext.JobSet
                .FirstOrDefaultAsync(j => j.Id == id);

            if (current is null)
                return NotFound();
            if (current.IsLocked)
            {
                return new StatusCodeResult((int)StatusCodes.Status403Forbidden);
            }

            // the "If-Match" Header is mandatory
            if (!HttpContext.Request.Headers.Keys.Contains("If-Match"))
            {
                return new StatusCodeResult(StatusCodes.Status412PreconditionFailed);
            }

            string? currentETag = current.GetETag();
            if (currentETag != null && HttpContext.Request.Headers["If-Match"].ToString() != currentETag)
            {
                return new StatusCodeResult(StatusCodes.Status412PreconditionFailed);
            }

            JobShopCollectionDbContext.Entry(current).CurrentValues.SetValues(newJobSet);
            await JobShopCollectionDbContext.SaveChangesAsync();

            JobSetDto jobSetDto = Mapper.Map<JobSetDto>(current);
            string? eTag = jobSetDto.GetETag();
            if (eTag != null)
            {
                HttpContext.Response.Headers.Add("ETag", new[] { eTag });
            }
            return Ok(jobSetDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var original = await JobShopCollectionDbContext.JobSet
                .FirstOrDefaultAsync(j => j.Id == id);

            if (original is null)
                return NotFound();
            if (original.IsLocked)
            {
                return new StatusCodeResult((int)StatusCodes.Status403Forbidden);
            }

            // the "If-Match" Header is mandatory
            if (!HttpContext.Request.Headers.Keys.Contains("If-Match"))
            {
                return new StatusCodeResult(StatusCodes.Status412PreconditionFailed);
            }

            string? originalETag = original.GetETag();
            if (originalETag != null && HttpContext.Request.Headers["If-Match"].ToString() != originalETag)
            {
                return new StatusCodeResult(StatusCodes.Status412PreconditionFailed);
            }

            JobShopCollectionDbContext.Entry(original).State = EntityState.Deleted;
            await JobShopCollectionDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
