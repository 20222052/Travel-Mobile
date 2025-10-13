
using project.Models;
using project.Models.Request;
using project.Models.Response;

using Microsoft.EntityFrameworkCore;
using project.Services;


namespace project.OtpConfig
{
    public class OtpService : IOtpService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public OtpService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<OtpResponse> GenerateOtpAsync(OtpRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return new OtpResponse { Success = false, Message = "Email không được để trống" };

            var otp = new Random().Next(100000, 999999).ToString();
            var expireAt = DateTime.UtcNow.AddMinutes(5);

            var otpEntity = new OtpCode
            {
                Email = request.Email,
                Code = otp,
                ExpireAt = expireAt,
                Used = false
            };

            _context.OtpCodes.Add(otpEntity);
            await _context.SaveChangesAsync();

            var html = $@"
                <h3>Mã OTP xác thực</h3>
                <p>Mã OTP của bạn là: <b>{otp}</b></p>
                <p>Hết hạn sau 5 phút.</p>
            ";
            await _emailService.SendEmailAsync(request.Email, "Xác thực OTP", html);

            return new OtpResponse { Success = true, Message = "Đã gửi mã OTP qua email." };
        }

        public async Task<OtpResponse> VerifyOtpAsync(VerifyOtpRequest request)
        {
            var otp = await _context.OtpCodes
                .Where(x => x.Email == request.Email && x.Code == request.Code && !x.Used)
                .FirstOrDefaultAsync();

            if (otp == null)
                return new OtpResponse { Success = false, Message = "Mã OTP không hợp lệ!" };

            if (otp.ExpireAt < DateTime.UtcNow)
                return new OtpResponse { Success = false, Message = "Mã OTP đã hết hạn!" };

            otp.Used = true;
            await _context.SaveChangesAsync();

            return new OtpResponse { Success = true, Message = "Xác thực OTP thành công!" };
        }
    }
}
