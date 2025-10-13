using System.Threading.Tasks;
using project.Models.Request;
using project.Models.Response;

namespace project.OtpConfig
{
    public interface IOtpService
    {
        Task<OtpResponse> GenerateOtpAsync(OtpRequest request);
        Task<OtpResponse> VerifyOtpAsync(VerifyOtpRequest request);
    }
}
