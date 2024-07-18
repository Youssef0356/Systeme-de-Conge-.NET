using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NETCore.MailKit.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using testingdatabase.Models;

namespace testingdatabase.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeaveRequestController> _logger;

        public LeaveRequestController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LeaveRequestController> logger)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Home", "Utilisateur");
        }

        public async Task<IActionResult> LesDemandesDeConge()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (await _userManager.IsInRoleAsync(user, "admin"))
            {
                var requests = await _context.LeaveRequests.ToListAsync();
                return View("Requests", requests);
            }
            else
            {
                var employeeRequests = await _context.LeaveRequests
                    .Where(lr => lr.UserId == user.Id)
                    .ToListAsync();

                return View("Requests", employeeRequests);
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> ApproveAndReject(int id, string status)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(leaveRequest.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            try
            {
                switch (status)
                {
                    case "Accepted":
                        leaveRequest.Status = "Approved";
                        await UpdateSoldValue(user, leaveRequest);
                        break;
                    case "Refused":
                        leaveRequest.Status = "Refused";
                        break;
                    default:
                        leaveRequest.Status = "Pending";
                        break;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("LesDemandesDeConge");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling leave request status: {ex.Message}");
                return StatusCode(500); 
            }
        }

        private async Task UpdateSoldValue(ApplicationUser user, LeaveRequest leaveRequest)
        {
            var totalDays = (leaveRequest.EndDate - leaveRequest.StartDate).Days + 1;
            _logger.LogInformation($"Total days calculated: {totalDays}");

            if (totalDays > 22)
            {
                user.Sold = 0;
                leaveRequest.Sold = 0;
            }
            else
            {
                var newSold = user.Sold - totalDays;

                if (newSold < 0)
                {
                    newSold = 0;
                }

                user.Sold = newSold;
                leaveRequest.Sold = newSold;
            }

            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitLeaveRequest(string raison, DateTime startDate, DateTime endDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var overlappingRequests = await _context.LeaveRequests
                .Where(lr => lr.UserId == user.Id && (
                    (startDate >= lr.StartDate && startDate <= lr.EndDate) ||
                    (endDate >= lr.StartDate && endDate <= lr.EndDate) ||
                    (startDate <= lr.StartDate && endDate >= lr.EndDate)))
                .ToListAsync();

            if (overlappingRequests.Any())
            {
                ModelState.AddModelError("", "Vous avez déjà demandé un congé pour certaines ou toutes les dates sélectionnées.");
                return View("Home"); 
            }

            var leaveRequest = new LeaveRequest
            {
                UserId = user.Id,
                Description = raison,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Pending",
            };

            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            TempData["FormSubmitted"] = true;
            return RedirectToAction("LesDemandesDeConge");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null)
            {
                return View("Error");
            }

            _context.LeaveRequests.Remove(leaveRequest);
            await _context.SaveChangesAsync();
            return RedirectToAction("LesDemandesDeConge");
        }
    }
}
