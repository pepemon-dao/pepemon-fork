using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Numerics;
using Nethereum.ABI;
using Nethereum.RLP;

[System.Serializable]
public class Player
{
    [BoxGroup("Player Loadout")] public Pepemon PlayerPepemon;
    [BoxGroup("Player Loadout")] public Deck PlayerDeck;

    [BoxGroup("Runtime")] public int CurrentHP;
    [BoxGroup("Runtime")] public Deck CurrentDeck;
    [BoxGroup("Runtime")] public Hand CurrentHand;
    [BoxGroup("Runtime")] public int StartingIndex;


    public void Initialise(int index)
    {
        CurrentHP = PlayerPepemon.HealthPoints;
        StartingIndex = index;
    }

    public void SetPlayerDeck(Pepemon pepemon, IEnumerable<Card> supportCards)
    {
        PlayerPepemon = pepemon;
        PlayerDeck.ClearDeck();
        PlayerDeck.GetDeck().AddRange(supportCards);
    }

    public void GetAndShuffelDeck(BigInteger seed, BigInteger currentTurn, BigInteger battleRng)
    {
        // Get local copy of deck and shuffle
        CurrentDeck.ClearDeck();
        CurrentDeck.GetDeck().AddRange(PlayerDeck.GetDeck());

        // calculate random seed like in solidity
        var abiEncode = new ABIEncode();
        CurrentDeck.ShuffelDeck(
            abiEncode.GetSha3ABIEncodedPacked(
                new ABIValue("uint256", seed),
                new ABIValue("uint256", currentTurn),
                new ABIValue("uint256", battleRng)
             ).ToBigIntegerFromRLPDecoded());
    }

    public void DrawNewHand()
    {
        // Clear previous hand
        CurrentHand.ClearHand();

        // Draw cards to hand
        List<Card> cacheList = new List<Card>();
        for (int i = 0; i < PlayerPepemon.Intelligence; i++)
        {
            if (i >= 0 && i < CurrentDeck.GetDeck().Count)
            {
                CurrentHand.AddCardToHand(CurrentDeck.GetDeck()[i]);
                cacheList.Add(CurrentDeck.GetDeck()[i]);
            }
        }

        // Cleanup working decks
        foreach (var item in cacheList) CurrentDeck.RemoveCard(item);
    }


    // editor only func
    public void Reset()
    {
        CurrentHand.ClearHand();
        CurrentDeck.ClearDeck();
        CurrentHP = 0;
    }


#if UNITY_EDITOR
    [Button("Add All Cards")]
    public void AddAllCardsToDeck()
    {
        PlayerDeck.ClearDeck();
        // All all cards to deck 4x
        for (int i = 0; i < 5; i++)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(Card).Name);  //FindAssets uses tags check documentation for more info
            Card[] a = new Card[guids.Length];
            for (int j = 0; j < guids.Length; j++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[j]);
                a[j] = UnityEditor.AssetDatabase.LoadAssetAtPath<Card>(path);
                PlayerDeck.GetDeck().Add(a[j]);
            }
        }
    }
#endif
}
