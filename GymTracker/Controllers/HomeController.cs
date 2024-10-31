using Microsoft.AspNetCore.Mvc;
using GymTracker.Models;

namespace GymTracker.Controllers;

public class HomeController(IStoreRepository repo) : Controller
{
    public IStoreRepository Repository { get; set; } = repo;

    public ViewResult Index() => View(Repository.WorkoutPlans);

}
