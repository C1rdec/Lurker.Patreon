﻿namespace Lurker.Patreon;

using System.Threading.Tasks;
using Lurker.Patreon.Models;

public class PatreonService
{
    #region Fields

    private PatreonFile _patreonFile;
    private bool _isPledged;
    private string _patreonId;
    private PatreonApiCredential _credential;

    #endregion

    #region Constructors

    public PatreonService(PatreonApiCredential credential)
    {
        _credential = credential;
        _patreonFile = new PatreonFile();
        _patreonFile.Initialize();
    }

    #endregion

    #region Properties

    public string PatreonId => _patreonId;

    public bool IsPledged => _isPledged;

    #endregion

    #region Methods

    public void LogOut()
    {
        _patreonId = string.Empty;
        _isPledged = false;
        _patreonFile.Delete();
    }

    public async Task LoginAsync()
    {
        using var service = CreateService();
        var accessToken = _patreonFile.Entity?.AccessToken;
        if (DateTime.Compare(_patreonFile.Entity.ExpiredDate, DateTime.UtcNow) < 0)
        {
            var tokenResult = await service.GetAccessTokenAsync(_patreonFile.Entity?.RefreshToken);
            _patreonFile.Save(PatreonToken.FromTokenResult(tokenResult));
            accessToken = tokenResult.AccessToken;
        }

        _patreonId = await service.GetPatronId(accessToken);
    }

    public async Task<bool> CheckPledgeStatusAsync(string campaignId)
    {
        if (string.IsNullOrEmpty(_patreonFile.Entity.AccessToken))
        {
            return false;
        }

        using var service = CreateService();
        _isPledged = await service.IsPledging(campaignId, _patreonFile.Entity.AccessToken);
        _patreonId = await service.GetPatronId(_patreonFile.Entity.AccessToken);

        return _isPledged;
    }

    private PatreonApiHandler CreateService()
        => new(_credential.Ports, _credential.ClientId, _credential.ClientSecret, _credential.WhiteListUrl);

    #endregion
}
