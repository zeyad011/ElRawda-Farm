using ElRawda.Core.Models;
using ElRawda.Core.Services;
using ElRawda.Hubs;
using ElRawda.Repository.Data;
using ElRawda.Shared.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ElRawda.Service
{
    public class CowServices : ICowServices
    {
        private readonly ELRawdaContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHubContext<CowHub> _hubContext;

        public CowServices(ELRawdaContext context, IHttpClientFactory httpClientFactory, IHubContext<CowHub> hubContext)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _hubContext = hubContext;
        }
        private string mapType(int cowType)
        {
            switch (cowType)
            {
                case 1: return "كتف يمين";
                case 2: return "كتف شمال ";
                case 3: return "فخده يمين";
                case 4: return "فخده شمال";
                default: throw new ArgumentException("Invalid cowType.");
            }
        }
        public async Task<ActionResult> KeyBad(double weight, int cowTybe, int machID)
        {
            var slaughteredCow = await _context.slaughteredCows
                           .OrderBy(sc => sc.DateOfSlaughter)
                           .FirstOrDefaultAsync(sc => !_context.cowsPieces
                               .Where(cp => cp.CowId == sc.Id)
                               .GroupBy(cp => cp.CowId)
                               .Any(g => g.Count() >= 4));

            if (slaughteredCow == null)
            {
                return new NotFoundObjectResult(new ApiResponse(404));
            }
            if(cowTybe > 4)
                return new BadRequestObjectResult(new ApiResponse(400));

            string pieceType = mapType(cowTybe);
            var existingPiece = await _context.cowsPieces
                           .FirstOrDefaultAsync(cp => cp.CowId == slaughteredCow.Id && cp.PieceTybe == pieceType);

            Random random = new Random();
            int randomSixDigits = random.Next(100, 999);
            var cowPiece = new CowsPieces
            {
                CowId = slaughteredCow.Id,
                MachId = machID,
                PieceWeight = weight,
                PieceTybe = mapType(cowTybe),
                dateOfSupply = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"),
                dateofExpiere = DateTime.Now.AddDays(8).ToString("MM/dd/yyyy HH:mm:ss"),
                PieceId = $"{slaughteredCow.CowsId}{randomSixDigits}",
            };
            _context.cowsPieces.Add(cowPiece);
            await _context.SaveChangesAsync();
            var cowPieces = await _context.cowsPieces
                     .Where(cp => cp.CowId == slaughteredCow.Id)
                     .ToListAsync();

            if (cowPieces.Count == 4 && cowPieces.Any(cp => cp.PieceTybe == null))
            {
                var existingTypes = cowPieces.Where(cp => cp.PieceTybe != null).Select(cp => cp.PieceTybe).ToHashSet();

                var allTypes = new HashSet<string> { "كتف يمين", "فخده شمال", "فخده يمين", "كتف شمال" };
                var missingType = allTypes.Except(existingTypes).FirstOrDefault();

                if (missingType != null)
                {
                    var pieceToFill = cowPieces.FirstOrDefault(cp => cp.PieceTybe == null);
                    pieceToFill.PieceTybe = missingType;
                    await _context.SaveChangesAsync();
                    // return new OkObjectResult($"Missing piece type '{missingType}' added to the cow.");
                }
            }
            if (cowPieces.Count == 4)
            {
                double? totalPieceWeight =  cowPieces.Sum(cp => cp.PieceWeight);
                slaughteredCow.Waste = slaughteredCow.WeightAtSlaughter - totalPieceWeight;
                await _context.SaveChangesAsync();
            }
            await _context.SaveChangesAsync();
            if (existingPiece != null)
            {
                return new BadRequestObjectResult(new ApiResponse(400,"Ok2"));//"This type of piece already exists for this cow."
            }
            var response = await GetHorizonDetails();
            await _hubContext.Clients.All.SendAsync("UpdateData", response);
            return new OkObjectResult("OK1");
        }

        public async Task<ActionResult> ScanCow(string CowsId, double Weight, int machID)
        {
            if (CowsId == null)
            {
                return new BadRequestObjectResult(new ApiResponse(400));
            }
            var cow = await _context.cows.FirstOrDefaultAsync(x => x.CowsId == CowsId);
            if (cow is not null)
            {
                return new BadRequestObjectResult(new ApiResponse(400));
            }
            var cows = new Cows
            {
                CowsId = CowsId,
                Weight = Weight,
                Date = DateTime.Now,
                MachId = machID
            };
            _context.cows.Add(cows);
            await _context.SaveChangesAsync();

            var response = await GetHorizonDetails();
            await _hubContext.Clients.All.SendAsync("UpdateData", response);

            return new OkObjectResult("OK");    
        }

        public async Task<ActionResult> ScanForSlaughteredCow(string CowsId, int machID)
        {
            if (CowsId == null)
            {
                return new BadRequestObjectResult(new ApiResponse(400));
            }
            var cow = await _context.cows.FirstOrDefaultAsync(x => x.CowsId == CowsId);
            if (cow == null)
            {
                return new NotFoundObjectResult(new ApiResponse(404));
            }

            var slaughteredCow = new SlaughteredCow
            {
                CowsId = CowsId,
                WeightAtSlaughter = cow.Weight,
                DateOfSlaughter = DateTime.Now,
                MachId = machID
            };

            _context.slaughteredCows.Add(slaughteredCow);


            _context.cows.Remove(cow);
            await _context.SaveChangesAsync();
            var response = await GetHorizonDetails();
            await _hubContext.Clients.All.SendAsync("UpdateData", response);
            return new OkObjectResult("OK");
        }
        public async Task<ActionResult> ModifyCowPieceType(int option)
        {
            var allCowPieces = await _context.cowsPieces.ToListAsync();
            var today = DateTime.Today;

            var todayCowPieces = allCowPieces
                .Where(cp => DateTime.Parse(cp.dateOfSupply).Date == today)
                .ToList();

            var duplicatePieces = todayCowPieces
                .GroupBy(cp => new { cp.CowId, cp.PieceTybe })
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderBy(cp => cp.Id))
                .ToList();

            if (duplicatePieces.Any())
            {
                var cowPieceToModify = option == 1 ? duplicatePieces[0] : duplicatePieces[1];
                cowPieceToModify.PieceTybe = null;
                await _context.SaveChangesAsync();
            }
            var cowGroups = todayCowPieces.GroupBy(cp => cp.CowId);
            foreach (var cowGroup in cowGroups)
            {
                if (cowGroup.Count() == 4)
                {
                    var existingTypes = cowGroup.Where(cp => cp.PieceTybe != null).Select(cp => cp.PieceTybe).ToHashSet();
                    var allTypes = new HashSet<string> { "كتف يمين", "فخده شمال", "فخده يمين", "كتف شمال" };

                    var missingType = allTypes.Except(existingTypes).FirstOrDefault();
                    if (missingType != null)
                    {
                        var pieceToFill = cowGroup.FirstOrDefault(cp => cp.PieceTybe == null);
                        if (pieceToFill != null)
                        {
                            pieceToFill.PieceTybe = missingType;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return new OkObjectResult("ok");
        }

        public async Task<ActionResult> GetHorizonDetails(int? year = null, DateTime? date = null)
        {
            DateTime targetDate = date ?? DateTime.Today;
            DateTime nextDay = targetDate.AddDays(1);
            int targetYear = year ?? DateTime.Now.Year;

            int cowRequest = 902; 
            int miscarriage = 350;



            int killedCowCount = await _context.slaughteredCows
                .CountAsync(sc => sc.DateOfSlaughter >= targetDate && sc.DateOfSlaughter < nextDay);


            int reminders = cowRequest - killedCowCount;

            double totalWeightOfKilledCows = await _context.slaughteredCows
                .Where(sc => sc.DateOfSlaughter >= targetDate && sc.DateOfSlaughter < nextDay)
                .SumAsync(sc => sc.WeightAtSlaughter);

            double totalWaste = await _context.slaughteredCows
                .Where(sc => sc.DateOfSlaughter >= targetDate && sc.DateOfSlaughter < nextDay)
                .SumAsync(sc => sc.Waste ?? 0);

            var monthlyData = _context.cowsPieces
                .AsEnumerable()
                .Select(cp => new
                {
                    cp.PieceWeight,
                    DateOfSupply = DateTime.TryParse(cp.dateOfSupply, out DateTime parsedDate) ? parsedDate : (DateTime?)null
                })
                .Where(cp => cp.DateOfSupply.HasValue && cp.DateOfSupply.Value.Year == targetYear)
                .GroupBy(cp => cp.DateOfSupply.Value.Month)
                .ToDictionary(
                    g => new DateTime(targetYear, g.Key, 1).ToString("MMM"),
                    g => g.Sum(cp => cp.PieceWeight ?? 0)
                );
            int standardRate = 60;
            DateTime fourHoursAgo = DateTime.Now.AddHours(-4);

            int recentSlaughteredCount = await _context.slaughteredCows
                .CountAsync(sc => sc.DateOfSlaughter >= fourHoursAgo);

            double performance = (double)recentSlaughteredCount / standardRate * 100;

            // Up-Time calculation
            var todayScans = await _context.slaughteredCows
                .Where(c => c.DateOfSlaughter >= targetDate && c.DateOfSlaughter < nextDay)
                .OrderBy(c => c.DateOfSlaughter)
                .ToListAsync();

            double upTimeMinutes = 0;
            for (int i = 1; i < todayScans.Count; i++)
            {
                upTimeMinutes += (todayScans[i].DateOfSlaughter - todayScans[i - 1].DateOfSlaughter).TotalMinutes;
            }

            double maxUpTimeMinutes = 8 * 60;
            double upTimePercentage = (upTimeMinutes / maxUpTimeMinutes) * 100;

            var response = new
            {
                CowRequest = cowRequest,
                KilledCow = killedCowCount,
                Reminders = reminders,
                WeightOfKilledCows = totalWeightOfKilledCows,
                TotalWaste = totalWaste,
                Miscarriage = miscarriage,
                Performance = performance,
                UpTime = upTimePercentage,
                Graph = monthlyData
            };
            await _hubContext.Clients.All.SendAsync("CowScanned", response);
            return new OkObjectResult(response);
        }

        public async Task<ActionResult> GetPieceTypeCounts(DateTime? date = null)
        {
            DateTime targetDate = date ?? DateTime.Now;

            
            var pieceTypeCounts = _context.cowsPieces
                .AsEnumerable()
                .Where(cp =>
                {
                    if (DateTime.TryParse(cp.dateOfSupply, out DateTime parsedDate))
                    {
                        return parsedDate.Date == targetDate.Date; 
                    }
                    return false; 
                })
                .GroupBy(cp => cp.PieceTybe)
                .Select(g => new
                {
                    PieceType = g.Key,
                    Count = g.Count()
                })
                .ToList();

            return new OkObjectResult(pieceTypeCounts);

        }

        public async Task<ActionResult> GetToken()
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://login.microsoftonline.com/be88f713-a964-488f-89ef-00a04bc0f789/oauth2/v2.0/token");

            request.Headers.Add("Host", "login.microsoftonline.com");
            request.Headers.Add("Cookie", "fpc=AvyEP5Jed7RKmdCSrL3MDMGixMtQAQAAAJTIx94OAAAA; stsservicecookie=estsfd; x-ms-gateway-slice=estsfd");

            var formContent = new List<KeyValuePair<string, string>>
    {
        new("client_id", "af9c6191-37aa-4bb4-a623-5e7f2c364c17"),
        new("client_secret", "GTq8Q~zIvI3XbcewOdV-4OEAgZYCFQiC4EOwYdbK"),
        new("grant_type", "client_credentials"),
        new("scope", "https://shatat-group.operations.dynamics.com/.default")
    };

            request.Content = new FormUrlEncodedContent(formContent);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseBody);


                if (jsonDoc.RootElement.TryGetProperty("access_token", out var accessToken))
                {
                    return new OkObjectResult(new { access_token = accessToken.GetString() });
                }

                return new BadRequestObjectResult(new ApiResponse(400, "access_token not found in response"));
            }
            else
            {
                return new ObjectResult(new { error = response.ReasonPhrase })
                {
                    StatusCode = (int)response.StatusCode
                };
            }
        }

        public async Task<ActionResult> GetSerialTrans(string inventSerialId)
        {
            var tokenResponse = await GetToken();
            if (tokenResponse is BadRequestObjectResult || tokenResponse is StatusCodeResult)
            {
                return tokenResponse; 
            }

            var tokenData = (OkObjectResult)tokenResponse;
            var accessToken = ((dynamic)tokenData.Value).access_token;

            if (accessToken == null)
            {
                return new BadRequestObjectResult(new ApiResponse(400, "Access token not found in response"));
            }

            
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://shatat-group.operations.dynamics.com/Data/Sha_SerialTrans?$filter=InventSerialId eq '{inventSerialId}'");

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("Cookie", "OpenIdConnect.nonce=..."); 

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<dynamic>(jsonString, options);

                return new OkObjectResult(data);
            }
            else
            {
                return new ObjectResult(new { error = response.ReasonPhrase })
                {
                    StatusCode = (int)response.StatusCode
                };
            }
        }
    }
}
