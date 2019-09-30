﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Set
{
    private const int MAX_GAMES_PER_SET = 7;
    private readonly TenisGame[] _games;
    private readonly int[] _results;
    private TenisGame _currentGame;
    private int _gameNumber;

    // 0 if no one has won the game yet, 1 if player 1 won and 2 if player 2 won.
    private int _winner;

    public Set()
    {
        _games = new TenisGame[MAX_GAMES_PER_SET + MAX_GAMES_PER_SET - 1];
        _results = new int[2];
        _winner = 0;
        _currentGame = new TenisGame();
        _gameNumber = 0;
        _games[_gameNumber] = _currentGame;

    }

    public int[] GetCurrentResult()
    {
        return _results;
    }

    public int GetWinner()
    {
        return _winner;
    }

    // returns true if game make set end and false if set continues
    private bool AddGame(int playerId)
    {
        if (_winner != 0)
        {
            Debug.Log("El set ya termino");//TODO make exception
   
        }
        else if (playerId != 1 && playerId != 2)
        {
            Debug.Log("No existe el id del jugador para agregar game");//TODO make exception
        }
        else
        {
            _results[playerId]++;
            if (HasWon(playerId))
            {
                _winner = playerId;
                return true;
            }
        }

        return false;
    }

    private bool HasWon(int playerId)
    {
        int otherPlayerId = (playerId % 2) + 1;
        int playerGames = _results[playerId];
        int otherPlayerGames = _results[otherPlayerId];

        if (playerGames == MAX_GAMES_PER_SET - 1 && (playerGames - otherPlayerGames >= 2) || playerGames == MAX_GAMES_PER_SET)
        {
            return true;
        }
        
        return false;
    }

    public bool AddPoint(int playerId)
    {
        if (_currentGame.AddPoint(playerId))
        {
            if (AddGame(playerId))
            {
                return true;
            }
            _currentGame = new TenisGame();
            _gameNumber++;
            _games[_gameNumber] = _currentGame;
        }

        return false;
    }

    public int[] GetCurrentGameResults()
    {
        return _currentGame.GetResults();
    }
}
