using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using jwtCoreDemo.Entities;
using jwtCoreDemo.Helpers;

namespace WebApi.Services
{
    public interface IUserService
    {
        User Authenticate(string kullaniciAdi, string sifre);
        IEnumerable<User> GetAll();
        IEnumerable<User> Insert(User user);
        bool IsUserExist(User user);
    }

    public class UserService : IUserService
    {
        // Kullanıcılar veritabanı yerine manuel olarak listede tutulamaktadır. Önerilen tabiki veritabanında hash lenmiş olarak tutmaktır. Burada Kullanıcı Adını da Şifreyi de MD5 ile şifrelemek doğru olandır
        
        private List<User> _users = new List<User>
        {
            new User { Id = 1, Ad = "Enes", Soyad = "Cemcir", KullaniciAdi = "enescemcir", Sifre = "1234" },
            new User { Id = 2, Ad = "Ali", Soyad = "Güç", KullaniciAdi = "aliguc", Sifre = "4321" }
        };

        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public User Authenticate(string kullaniciAdi, string sifre)
        {
            var user = _users.SingleOrDefault(x => x.KullaniciAdi == kullaniciAdi && x.Sifre == sifre);

            // Kullanici bulunamadıysa null döner.
            if (user == null)
                return null;

            // Authentication(Yetkilendirme) başarılı ise JWT token üretilir.
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.KullaniciAdi.ToString())
                }),
                Expires = DateTime.Now.AddMinutes(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            // Sifre null olarak gonderilir.
            user.Sifre = null;

            return user;
        }

        public IEnumerable<User> GetAll()
        {
            // Kullanicilar sifre olmadan dondurulur.
            return _users.Select(x =>
            {
                x.Sifre = null;
                return x;
            });
        }

        public bool IsUserExist(User user)
        {
            bool isExist;

            var userName = user.KullaniciAdi.ToLower();
            isExist = _users.Any(n => n.KullaniciAdi == user.KullaniciAdi.ToLower());

            return isExist;
        }

        public IEnumerable<User> Insert(User user)
        {
            _users.Add(user);
            return _users;
        }
       
    }
}