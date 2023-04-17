using JWT_Practice.DaraBase;
using JWT_Practice.DTO;
using JWT_Practice.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JWT_Practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        //public static Login log = new Login();
        private readonly IConfiguration _configuration;

        private readonly JwtDbContext _db;

        public LoginController(IConfiguration configuration, JwtDbContext db)
        {
            _configuration = configuration;
            _db = db;
        }

        [HttpGet("All")]
        public IActionResult Get()
        {
            Log.Information("z");
            try
            {
                var db = _db.Logins.FirstOrDefault();
                Log.Debug("Debug Getting All Details ");
                throw new AccessViolationException("Error geting the status");
                return Ok(_db.Logins);
            }
            catch (Exception)
            {
                throw;
            }

            //return BadRequest(exe.Message);
        }

        [HttpPost("register")]
        public ActionResult AddUser(LoginDTO login)
        {
            try
            {
                //throw new DivideByZeroException("divide by zero");
                CreatePasswordHash(login.password, out byte[] PasswordHash, out byte[] PasswordSalt);

                Login logins = new Login()
                {
                    UserName = login.UserName,
                    PasswordHash = PasswordHash,
                    PasswordSalt = PasswordSalt,
                    Role = login.Role,
                };
                _db.Add(logins);
                _db.SaveChanges();

                return Ok("Registered Sucessfully " + login.UserName);
            }
            catch (Exception exe)
            {
                //throw;
                return BadRequest(exe.Message);
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult> Login(LoginResponseDTO login)
        {
            try
            {
                //var a = 0;
                //var ans = a / a;
                //?--------------------------------user match -----------------------------------
                if (!_db.Logins.Any(x => x.UserName == login.UserName))
                //if (log.UserName != login.UserName)
                {
                    return BadRequest("User Not Found");
                }
                //?---------------------password match ---------------------------

                //var Passhash = _db.Logins.Where(x=>x.UserName == login.UserName).Select(x=>x.PasswordHash).ToString;
                var Passhash = await _db.Logins.FirstOrDefaultAsync(x => x.UserName == login.UserName);
                if (!VerifyPass(login.password, Passhash?.PasswordHash, Passhash?.PasswordSalt))
                //; sending saved password to method verify password
                {
                    return BadRequest("Wrong Password");
                }
                string token = CreateToken(login);

                return Ok(token);
            }
            catch (Exception)
            {
                throw;
                //return BadRequest(exe.Message);
            }
        }

        //?-----------------------------Create Token ---------------------------
        private string CreateToken(LoginResponseDTO login)
        {
            var role = _db.Logins.FirstOrDefault(_db => _db.UserName == login.UserName);
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name , login.UserName),
                new Claim(ClaimTypes.Role , role.Role),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddSeconds(60),
                    signingCredentials: cred
                    );

            var JwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return JwtToken;
        }

        private void CreatePasswordHash(String Password, out byte[] PasswordHash, out byte[] PasswordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                PasswordSalt = hmac.Key;
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Password));
            }
        }

        private bool VerifyPass(string Password, byte[] PasswordHash, byte[] PasswordSalt)
        {
            using (var hmac = new HMACSHA512(PasswordSalt))
            //;using key to access password
            {
                //; converting again user input to hascode and matching byte by byte with saved password comming from parameter
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Password));
                return computedHash.SequenceEqual(PasswordHash);
            }
        }
    }
}