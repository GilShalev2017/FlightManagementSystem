using FlightManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FlightManagementSystem.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightPriceController : ControllerBase
    {
        /*
        The FlightPriceController can serve multiple purposes:

        Expose Flight Price Data to Clients:

        Provide APIs for consumers to query the latest flight prices.
        Useful for exposing the cached or latest data stored by your background service.
        Dynamic Endpoint Management:

        Allow admin users to add, update, or remove API endpoints during runtime without restarting the service.
        Example: Add an endpoint dynamically via an API call.*/

    }
}
