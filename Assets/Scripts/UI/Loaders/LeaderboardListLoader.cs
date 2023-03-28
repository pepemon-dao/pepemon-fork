using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the loading of player rankings; Creates instances of _playerRankingPrefab controlled by PlayerRankingController
/// </summary>
public class LeaderboardListLoader : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] PlayerRankingController _playerRankingPrefab;
    [TitleGroup("Component References"), SerializeField] GameObject _rankingList;
    [TitleGroup("Component References"), SerializeField] Text _loadingMessage;

    private bool loadingInProgress = false;

    /// <summary>
    /// Removes all elements in _playerList and loads all entries using _playerRankingPrefab.
    /// </summary>
    public async void ReloadLeaderboard(PepemonMatchmaker.PepemonLeagues league)
    {
        // prevent re-reloading things over and over again with old async calls if
        // the user decides to go back and forth very quickly between screens
        if (loadingInProgress)
            return;

        loadingInProgress = true;

        _loadingMessage.gameObject.SetActive(true);
        _loadingMessage.text = "Loading leaderboard...";

        // destroy before re-creating
        foreach (var playerRanking in _rankingList.GetComponentsInChildren<PlayerRankingController>())
        {
            Destroy(playerRanking.gameObject);
        }

        // should not happen, but if it happens then it won't crash the game
        var account = Web3Controller.instance.SelectedAccountAddress;
        if (string.IsNullOrEmpty(account))
        {
            _loadingMessage.text = "Error: No account selected";
            return;
        }

        // load all rankings
        List<(string Address, ulong Ranking)> rankings;
        try
        {
            // TODO: use dynamic count and offset
            rankings = (await PepemonMatchmaker.GetPlayersRankings(league, count: 10, offset: 0))
                .OrderByDescending((i) => i.Ranking).ToList();
        }
        catch(System.Exception)
        {
            // Might always happen when there are no players in the leaderboard, eg.: new deployment of the Matchmaker contract.
            // Also when there are network issues
            _loadingMessage.text = "Unable to load leaderboard";
            return;
        }


        List<PlayerRankingController> refs = new List<PlayerRankingController>();
        foreach (var playerRanking in rankings)
        {
            var playerRankingInstance = Instantiate(_playerRankingPrefab);
            playerRankingInstance.transform.SetParent(_rankingList.transform, false);
            playerRankingInstance.SetInfo(playerRanking.Address, playerRanking.Ranking.ToString());
            refs.Add(playerRankingInstance);
        }

        _loadingMessage.gameObject.SetActive(false);
        loadingInProgress = false;
    }
}
