using Microsoft.AspNetCore.Mvc;
using CommBank.Services;
using CommBank.Models;

namespace CommBank.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoalController : ControllerBase
{
    private readonly IGoalsService _goalsService;
    private readonly IUsersService _usersService;

    public GoalController(IGoalsService goalsService, IUsersService usersService)
    {
        _goalsService = goalsService;
        _usersService = usersService;
    }

    /// <summary>
    /// Gets all goals
    /// </summary>
    /// <returns>A list of all goals including their icon properties</returns>
    [HttpGet]
    public async Task<List<Goal>> Get() =>
        await _goalsService.GetAsync();

    /// <summary>
    /// Gets a specific goal by ID
    /// </summary>
    /// <param name="id">The goal ID</param>
    /// <returns>The goal with all properties including the icon, or NotFound if not found</returns>
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Goal>> Get(string id)
    {
        var goal = await _goalsService.GetAsync(id);

        if (goal is null)
        {
            return NotFound();
        }

        return goal;
    }

    /// <summary>
    /// Gets all goals for a specific user
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>A list of goals for the user including their icon properties</returns>
    [HttpGet("User/{id:length(24)}")]
    public async Task<List<Goal>?> GetForUser(string id) =>
        await _goalsService.GetForUserAsync(id);

    [HttpPost]
    public async Task<IActionResult> Post(Goal newGoal)
    {
        await _goalsService.CreateAsync(newGoal);

        if (newGoal.Id is not null && newGoal.UserId is not null)
        {
            var user = await _usersService.GetAsync(newGoal.UserId);

            if (user is not null && user.Id is not null)
            {
                if (user.GoalIds is not null)
                {
                    user.GoalIds.Add(newGoal.Id);
                }
                else
                {
                    user.GoalIds = new()
                    {
                        newGoal.Id
                    };
                }

                await _usersService.UpdateAsync(user.Id, user);
            }
        }

        return CreatedAtAction(nameof(Get), new { id = newGoal.Id }, newGoal);
    }

    /// <summary>
    /// Updates an existing goal
    /// </summary>
    /// <param name="id">The ID of the goal to update</param>
    /// <param name="updatedGoal">The updated goal data including the icon property</param>
    /// <returns>NoContent if successful, NotFound if goal doesn't exist</returns>
    /// <remarks>
    /// This endpoint supports updating all goal properties including the 'icon' field.
    /// When sending a PUT request, include the 'icon' property in the request body to update the goal's icon.
    /// The icon value will be saved and returned in subsequent GET responses.
    /// 
    /// Example request body:
    /// {
    ///   "name": "House Down Payment",
    ///   "targetAmount": 100000,
    ///   "targetDate": "2025-12-31T00:00:00Z",
    ///   "balance": 25000.00,
    ///   "icon": "🏠",
    ///   "userId": "60d5ecb54e8b9a2d4c8f1234"
    /// }
    /// </remarks>
    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Goal updatedGoal)
    {
        var goal = await _goalsService.GetAsync(id);

        if (goal is null)
        {
            return NotFound();
        }

        updatedGoal.Id = goal.Id;

        await _goalsService.UpdateAsync(id, updatedGoal);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var goal = await _goalsService.GetAsync(id);

        if (goal is null)
        {
            return NotFound();
        }

        await _goalsService.RemoveAsync(id);

        return NoContent();
    }
}