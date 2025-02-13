using Microsoft.AspNetCore.Mvc;
using System.Reflection.PortableExecutable;

namespace ElRawda.Core.Services
{
    public interface ICowServices
    {
        public Task<ActionResult> ScanCow(string CowsId, double Weight, int machID);
        public Task<ActionResult> ScanForSlaughteredCow(string CowsId, int machID);
        public Task<ActionResult> KeyBad(double weight,int cowTybe,int machID);
        public Task<ActionResult> ModifyCowPieceType(int option);
        public Task<ActionResult> GetHorizonDetails(int? year, DateTime? date);
        public Task<ActionResult> GetPieceTypeCounts(DateTime? date);
        public Task<ActionResult> GetToken();
        public Task<ActionResult> GetSerialTrans(string inventSerialId);
    }
}
