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
        public async Task<ActionResult<JobSetsResponse>> GetAll([FromQuery] JobSetsQuery jobSetsQuery)
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
            return new JobSetsResponse
            {
                Data = data,
                NextPageToken = nextPageToken
            };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetJobSetResponse>> Get(int id)
        {
            var result = await JobShopCollectionDbContext.JobSet
                .ProjectTo<JobSetDto>(Mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(j => j.Id == id);

            return result is null
                ? new GetJobSetResponse
                {
                    Status = GetJobSetResponseStatus.NotFound
                }
                : new GetJobSetResponse
                {
                    Status = GetJobSetResponseStatus.Ok,
                    Data = result
                };
        }

        [HttpPost]
        public async Task<ActionResult<JobSetDto>> Post([FromBody] NewJobSetRequest newJobSetRequest)
        {
            var jobSet = Mapper.Map<JobSet>(newJobSetRequest);
            JobShopCollectionDbContext.JobSet.Add(jobSet);
            await JobShopCollectionDbContext.SaveChangesAsync();

            var result = Mapper.Map<JobSetDto>(jobSet);
            return CreatedAtAction("Get", new { id = jobSet.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UpdateJobSetResponse>> Put(int id, [FromBody] UpdateJobSetRequest updateJobSetRequest)
        {
            if (!id.Equals(updateJobSetRequest.Id))
                return BadRequest(new { Message = "The Id in route does not equal to the Id in Body." });

            var newJobSet = Mapper.Map<JobSet>(updateJobSetRequest);

            var savedJobSet = await JobShopCollectionDbContext.JobSet
                .FirstOrDefaultAsync(j => j.Id == id);

            if (savedJobSet is null)
            {
                return new UpdateJobSetResponse
                {
                    Status = UpdateJobSetResponseStatus.NotFound
                };
            }
            if (savedJobSet.IsLocked)
            {
                return new UpdateJobSetResponse
                {
                    Status = UpdateJobSetResponseStatus.ForbiddenBeacuseLocked
                };
            }

            JobSetDto savedJobSetDto = Mapper.Map<JobSetDto>(savedJobSet);

            string? savedVersionToken = VersionTokenHelper.GetVersionToken(savedJobSet); // copied
            if (savedVersionToken != null && updateJobSetRequest.VersionToken != savedVersionToken)
            {
                return new UpdateJobSetResponse
                {
                    Status = UpdateJobSetResponseStatus.VersionConditionFailed,
                    SavedJobSet = savedJobSetDto
                };
            }
            try
            {
                JobShopCollectionDbContext.Entry(savedJobSet).CurrentValues.SetValues(newJobSet);
                savedJobSet.RowVersion = VersionTokenHelper.ConvertToRowVersion(updateJobSetRequest.VersionToken!);
                await JobShopCollectionDbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (ex.Entries.Count == 1 && ex.Entries.Single().Entity is JobSet)
                {
                    var databaseValues = ex.Entries.Single().GetDatabaseValues();
                    JobShopCollectionDbContext.Entry(savedJobSet).CurrentValues.SetValues(databaseValues);
                    JobSetDto databaseJobSetDto = Mapper.Map<JobSetDto>(savedJobSet);
                    return new UpdateJobSetResponse
                    {
                        Status = UpdateJobSetResponseStatus.VersionConditionFailed,
                        SavedJobSet = databaseJobSetDto
                    };
                }
                else
                {
                    return new UpdateJobSetResponse
                    {
                        Status = UpdateJobSetResponseStatus.VersionConditionFailed,
                        SavedJobSet = savedJobSetDto
                    };
                }
            }

            JobSetDto updatedJobSetDto = Mapper.Map<JobSetDto>(savedJobSet);
            return new UpdateJobSetResponse
            {
                Status = UpdateJobSetResponseStatus.Done,
                UpdatedJobSet = updatedJobSetDto
            };
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
            JobShopCollectionDbContext.Entry(original).State = EntityState.Deleted;
            await JobShopCollectionDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/lock")]
        public async Task<IActionResult> Lock(int id)
        {
            var jobSet = await JobShopCollectionDbContext.JobSet
                .FirstOrDefaultAsync(j => j.Id == id);

            if (jobSet is null)
                return NotFound();
            jobSet.IsLocked = true;
            await JobShopCollectionDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/unlock")]
        public async Task<IActionResult> Unlock(int id)
        {
            var jobSet = await JobShopCollectionDbContext.JobSet
                .FirstOrDefaultAsync(j => j.Id == id);

            if (jobSet is null)
                return NotFound();
            jobSet.IsLocked = false;
            await JobShopCollectionDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
