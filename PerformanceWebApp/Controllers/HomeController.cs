using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Metrics;

namespace PerformanceWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMetrics _metrics;

        public HomeController(IMetrics metrics)
        {
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }

        public IActionResult Index()
        {
            var metricsProcessor = new MetricsProcessor(_metrics);
            metricsProcessor.ProcessRequest(4, 2);
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
