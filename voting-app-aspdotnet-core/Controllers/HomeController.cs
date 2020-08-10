using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using VotingApp.Models;

namespace VotingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly CacheManager _cache;

        public HomeController(IDistributedCache cache)
        {
            _cache = new CacheManager(cache);
        }

        public IActionResult Index()
        {
            return View(new VoteViewModel(_cache.Vote1, _cache.Vote2));
        }

        
        [HttpPost]
        public IActionResult Vote1()
        {
            _cache.IncrementVote1();
            return RedirectToAction("Index", new VoteViewModel(_cache.Vote1, _cache.Vote2));
        }

        [HttpPost]
        public IActionResult Vote2()
        {
            _cache.IncrementVote2();
            return RedirectToAction("Index", new VoteViewModel(_cache.Vote1, _cache.Vote2));
        }


        [HttpPost]
        public IActionResult Reset()
        {   
            _cache.Reset();
            return RedirectToAction("Index", new VoteViewModel());
        }
    }
}