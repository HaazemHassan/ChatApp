using System;
using Microsoft.AspNetCore.Http;

namespace ChatApi.Core.Abstracts.ServicesContracts;

public interface ITokenService
{
    public CookieOptions GetRefreshTokenCookieOptions();
}
