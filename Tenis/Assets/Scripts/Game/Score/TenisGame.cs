﻿using System;
using UnityEngine;

public class TenisGame  {
    public static readonly string[] PointStrings = {"0", "15", "30", "40", "Ad"};
    public const int AdvantageIndex = 4;

    private int[] _points;

    // 0 if no one has won the game yet, 1 if player 1 won and 2 if player 2 won.
    private int _winner;

    public TenisGame()
    {
        _points = new int[2];
        _winner = 0;
    }

    public int[] GetResults()
    {
        return _points;
    }
    
    public int GetWinner()
    {
        return _winner;
    }

    // returns true if point make game end and false if game continues
    public bool AddPoint(int playerId)
    {
        if (_winner != 0)
        {
            throw new Exception("El juego ya terminó");
   
        }
        else if (playerId != 1 && playerId != 2)
        {
            throw new Exception("No existe el ID del jugador para agregar punto");
        }
        else
        {
            _points[playerId - 1]++;
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
        int playerPoints = _points[playerId - 1];
        int otherPlayerPoints = _points[otherPlayerId - 1];

        if (playerPoints == otherPlayerPoints && playerPoints >= 4)
        {
            _points[playerId - 1] = 3;
            _points[otherPlayerId - 1] = 3;
            return false;
        }
        
        if (playerPoints >= 4 && (playerPoints - otherPlayerPoints) >= 2)
        {
            return true;
        }

        return false;

    }

    public string GetTeam1Points()
    {
        return PointStrings[_points[0]];
    }
    public string GetTeam2Points()
    {
        return PointStrings[_points[1]];
    }
    
}
