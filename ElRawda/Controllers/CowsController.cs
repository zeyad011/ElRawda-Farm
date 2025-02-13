namespace ElRawda.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CowsController : ControllerBase
    {
        private readonly ICowServices _cowServices;
        private readonly IHttpClientFactory _httpClientFactory;

        public CowsController(ICowServices cowServices, IHttpClientFactory httpClientFactory)
        {
            _cowServices = cowServices;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("Scan")]
        public async Task<ActionResult> ScanCows(string CowsId, double Weight, int machID)
        {
            return await _cowServices.ScanCow(CowsId, Weight, machID);
        }

        [HttpPost("ScanForSlaughteredCow")]
        public async Task<ActionResult> ScanForSlaughteredCow(string CowsId, int machID)
        {
            return await _cowServices.ScanForSlaughteredCow(CowsId, machID);
        }
        [HttpPost("KeyBad")]
        public async Task<ActionResult> KeyBad(double weight, int cowTybe, int machID)
        {
            return await _cowServices.KeyBad(weight, cowTybe, machID);
        }

        [HttpPost("ModifyCowPieceType")]
        public async Task<ActionResult> ModifyCowPieceType(int option)
        {
            return await _cowServices.ModifyCowPieceType(option);
        }
        [HttpGet("GetCowStatistics")]
        public async Task<ActionResult> GetCowStatistics(int? year = null,DateTime? date = null)
        {
            return await _cowServices.GetHorizonDetails(year, date);
        }
        [HttpGet("GetToken")]
        public async Task<IActionResult> GetToken()
        {
           return await _cowServices.GetToken();
        }


        [HttpGet("GetPieceTypeCounts")]
        public async Task<ActionResult> GetPieceTypeCounts(DateTime dateTime)
        {
            return await _cowServices.GetPieceTypeCounts(dateTime);
        }
        [HttpGet("GetSerialTrans")]
        public async Task<IActionResult> GetSerialTrans(string inventSerialId)
        {
           return await _cowServices.GetSerialTrans(inventSerialId);
        }


    }
}
