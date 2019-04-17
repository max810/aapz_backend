using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BLL.Services;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Impl;

namespace BLL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {
        //private IConnectionManagerThreadSafe<string> _connManager;
        private HttpClient _httpClient = new HttpClient();
        private AAPZ_BackendContext _context = null;
        private IServiceProvider _provider;
        //private static bool QuartzLaunched = false;
        private StreamingLogic _streamingLogic;

        public StreamController(AAPZ_BackendContext context, 
            IServiceProvider provider,
            StreamingLogic streamingLogic)
        {
            _context = context;
            _provider = provider;
            _streamingLogic = streamingLogic;
        }

        // TODO - add RSA get pbk send pbk 200
        [HttpPost("start-stream")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<int>> StartStream(string driverIdentifier)
        {
            string hash = Misc.GetMD5HashB64(driverIdentifier);
            Driver driver = await _context.Drivers.FirstOrDefaultAsync(x => x.IdentifierHashB64 == hash);
            if(driver is null)
            {
                return Unauthorized();
            }
            if (_streamingLogic.StreamExists(driver))
            {
                return UnprocessableEntity("Stream already exists");
            }

            int listentingPort = _streamingLogic.StartNewListeningStream(driver, TimeSpan.FromSeconds(20));

            return Ok(listentingPort);
        }

        [HttpPost("stop-stream")]
        public async Task<IActionResult> StopStream(string driverIdentifier)
        {
            string hash = hash = Misc.GetMD5HashB64(driverIdentifier);
            Driver driver = await _context.Drivers.FirstOrDefaultAsync(x => x.IdentifierHashB64 == hash);

            _streamingLogic.StopStream(driver);
            throw new NotImplementedException();
            // TODO - implement get driver. _streaming.StopStream();
        }

        private async Task<string> MakeInferenceRequest(byte[] imgJpegEncoded)
        {
            //client.BaseAddress = new Uri("http://localost:8000");
            //var request = new HttpRequestMessage(HttpMethod.Post, "classifier/inference");
            //request.Content = new ByteArrayContent(imgJpegEncoded);
            //var r = await client.GetAsync("http://localhost:8000/classifier");
            var res = await _httpClient.PostAsync("http://localhost:8000/classifier/inference", new ByteArrayContent(imgJpegEncoded));

            return await res.Content.ReadAsStringAsync();
        }
    }
}