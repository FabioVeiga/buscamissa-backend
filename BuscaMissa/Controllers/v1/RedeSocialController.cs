using Asp.Versioning;
using BuscaMissa.DTOs.v1.RedeSocialDto;
using BuscaMissa.Enums;
using BuscaMissa.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RedeSocialController : ControllerBase
    {
        private static readonly Dictionary<TipoRedeSocialEnum, string> _icones = new()
        {
            { TipoRedeSocialEnum.Facebook,  "pi pi-facebook"  },
            { TipoRedeSocialEnum.Instagram, "pi pi-instagram" },
            { TipoRedeSocialEnum.YouTube,   "pi pi-youtube"   },
            { TipoRedeSocialEnum.TikTok,    "pi pi-tiktok"    },
            { TipoRedeSocialEnum.Twitter,   "pi pi-twitter"   },
        };
        

        [HttpGet("tipos")]
        public IActionResult ObterTipos()
        {
            var tipos = Enum.GetValues<TipoRedeSocialEnum>()
                .Select(t => new RedeSocialTipoResponse(
                    id: (int)t,
                    nome: t.ToString(),
                    urlBase: RedeSocialHelper.ObterUrlBase(t),
                    icone: _icones.TryGetValue(t, out var icone) ? icone : ""
                ))
                .ToList();

            return Ok(tipos);
        }
    }
}
