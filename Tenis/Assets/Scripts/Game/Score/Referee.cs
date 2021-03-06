﻿using System;
using System.Collections;
using System.Collections.Generic;
using Game.GameManager;
using Game.Score;
using UnityEngine;

public class Referee
{
    // court delimiters
    private Vector3 _eastCourtSide;
    private Vector3 _westCourtSide;
    private Vector3 _southCourtSide;
    private Vector3 _northCourtSide;
    
    // service walls
    private GameObject _southServiceWall;
    private GameObject _southEastServiceWall;
    private GameObject _southWestServiceWall;
    private GameObject _southMiddleServiceWall;
    private GameObject _northServiceWall;
    private GameObject _northEastServiceWall;
    private GameObject _northWestServiceWall;
    private GameObject _northMiddleServiceWall;

    //service delimiters
    private Vector3 _southServiceDelimiter;
    private Vector3 _eastServiceDelimiter;
    private Vector3 _westServiceDelimiter;
    private Vector3 _northServiceDelimiter;

    // 0 undefined, -1 south, 1 north
    private int _lastBoucedSide;
    private int _lastHitter;
    private int _previousToLastHitter;

    // players
    private PlayerLogic _player1;
    private AIPlayer _aiPlayer;
    private Player2Logic _player2;
    
    // service related
    private bool _isServing;
    private int _serviceTimes;
    
    // game mode
    private bool _twoPlayers;
    private int _difficulty;

    


    public Referee(Vector3 eastCourtSide, Vector3 westCourtSide,
                    Vector3 southCourtSide, Vector3 northCourtSide,
                    GameObject southServiceWall, GameObject southEastServiceWall,
                    GameObject southWestServiceWall, GameObject southMiddleServiceWall,
                    GameObject northServiceWall, GameObject northEastServiceWall,
                    GameObject northWestServiceWall,GameObject northMiddleServiceWall,
                    Vector3 southServiceDelimiter, Vector3 eastServiceDelimiter,
                    Vector3 westServiceDelimiter, Vector3 northServiceDelimiter,
                    PlayerLogic player1, AIPlayer aiPlayer, Player2Logic player2, 
                    bool twoPlayers, int difficulty)
    {
        _eastCourtSide = eastCourtSide;
        _westCourtSide = westCourtSide;
        _southCourtSide = southCourtSide;
        _northCourtSide = northCourtSide;
        _lastBoucedSide = 0;
        _lastHitter = 0;
        _previousToLastHitter = 0;
        _isServing = true;
        _serviceTimes = 0;
        _twoPlayers = twoPlayers; 
        _difficulty = difficulty;

        _southServiceWall = southServiceWall;
        _southEastServiceWall = southEastServiceWall;
        _southWestServiceWall = southWestServiceWall;
        _southMiddleServiceWall = southMiddleServiceWall;
        _northServiceWall = northServiceWall;
        _northEastServiceWall = northEastServiceWall;
        _northWestServiceWall = northWestServiceWall;
        _northMiddleServiceWall = northMiddleServiceWall;

        _southServiceDelimiter = southServiceDelimiter;
        _eastServiceDelimiter = eastServiceDelimiter;
        _westServiceDelimiter = westServiceDelimiter;
        _northServiceDelimiter = northServiceDelimiter;

        _player1 = player1;
        _aiPlayer = aiPlayer;
        _player2 = player2;
        SetGameMode();
        DisableExtraPlayer();
    }

    private void SetGameMode()
    {
        _player1.SetGameMode(_twoPlayers);
        if (_twoPlayers)
        {
            _player2.SetGameMode(_twoPlayers);
        }
        else
        {
            _aiPlayer.SetDifficulty(_difficulty);
        }
    }

    private void DisableExtraPlayer()
    {
        if (_twoPlayers)
        {
            _aiPlayer.gameObject.SetActive(false);
            _aiPlayer.GetComponent<Collider>().enabled = false;
        }
        else
        {
            foreach(var collider in _player2.GetComponents<Collider>())
            {
                collider.enabled = false;
            }
            _player2.gameObject.SetActive(false);

        }
    }

    // Returns -1 if is point for opponent, 1 if is point for hitting team or zero if it is not point
    public int IsPoint(Vector3 bouncePosition, int hitter)
    {
        int returnValue = 0;
        if (hitter != 0)
        {
            if (BallLogic.Instance.IsEnabled() == false)
            {
                return -1;
            }
            int currentSide = GetBouncingSide(bouncePosition);
            int hittingSide = GetHittingSide(hitter);
            if (currentSide == _lastBoucedSide && _lastBoucedSide != 0 && _previousToLastHitter == 0)
            {

                if (hittingSide == currentSide)
                {
                    Debug.Log("bounce on same side as hitter");
                    returnValue = -1;
                }
                else
                {
                    Debug.Log("bounced two times" + "\n _lastBouncedSize: " + _lastBoucedSide + " currentSide: " + currentSide +
                              " previousToLastHitter: " + _previousToLastHitter);
                    returnValue = 1;
                }
            }
            else if (IsOut(bouncePosition))
            {
                Debug.Log("out");
                returnValue = -1;
            }
        
            if (returnValue == 0)
            {
                SetServing(false);
                _lastBoucedSide = currentSide;
            }
            else
            {
                _lastBoucedSide = 0;
            }
        }
        
        return returnValue;
    }

    public void MakeCelebrateAndAngry(int hitter, bool celebrate)
    {
        if (hitter == 1)
        {
            if (celebrate)
            {
                _player1.Celebrate();
            }
            else
            {
                _player1.Angry();                
            }
        }
        else if (_twoPlayers)
        {
            if (celebrate)
            {
                _player2.Celebrate();
            }
            else
            {
                _player2.Angry();
            }
        }
        else
        {
            if (celebrate)
            {
                _aiPlayer.Celebrate();
            }
            else
            {
                _aiPlayer.Angry();
            }
        }
    }

    public void UpdateLastHitter(int hitter)
    {
//        Debug.Log(hitter);
        _previousToLastHitter = _lastHitter;
        _lastHitter = hitter;
    }

    public void ResetHitters()
    {
        _previousToLastHitter = 0;
        _lastHitter = 0;
    }

    private int GetBouncingSide(Vector3 bouncePosition)
    {
        return bouncePosition.x < 0 ? -1 : 1;
    }

    private int GetHittingSide(int hitter)
    {
        if (hitter == 1)
        {
            return -1;
        }
        
        if (hitter == 2)
        {
            return 1;
        }
        
        return 2;
    }

    public bool IsOut(Vector3 bouncePosition)
    {
        if (_isServing)
        {
            int bouncingSide = GetBouncingSide(bouncePosition);
            int hittingSide = GetHittingSide(BallLogic.Instance.GetHittingPlayer());
            if (hittingSide == bouncingSide)
            {
                return true;
            }
            return CheckServiceLimits(bouncePosition);
        }

        if (bouncePosition.x < _southCourtSide.x || bouncePosition.x > _northCourtSide.x)
        {
//            Debug.Log("bounce long out");

            return true;
        }

        if (bouncePosition.z < _westCourtSide.z || bouncePosition.z > _eastCourtSide.z)
        {
//            Debug.Log("bounce wide out");

            return true;
        }

        return false;
    }

    private bool CheckServiceLimits(Vector3 bouncePosition)
    {
        Side servingSide = ScoreManager.GetInstance().GetServingSide();
        int servingTeam = ScoreManager.GetInstance().GetServingTeam();
        Side expectedSide = servingSide;
        if (servingTeam == 1)
        {
             expectedSide = servingSide == Side.RIGHT ? Side.LEFT : Side.RIGHT;
        }

        return IsOutsideServiceBox(bouncePosition, servingTeam, expectedSide);
    }

    // returns true if ball bounce outside expected service box and false if ball bounce in
    private bool IsOutsideServiceBox(Vector3 bouncePosition, int servingTeam, Side expectedSide)
    {
        bool isOutside = false;
//        // servingTeam 1 is south, servingTeam 2 is north
        if (servingTeam == 1 && expectedSide == Side.LEFT)
        {
            isOutside = IsOutsideNorthEastServiceBox(bouncePosition);
        }

        else if (servingTeam == 1 && expectedSide == Side.RIGHT)
        {
            isOutside = IsOutsideNorthWestServiceBox(bouncePosition);
        }
        
        else if (servingTeam == 2 && expectedSide == Side.LEFT)
        {
            isOutside = IsOutsideSouthEastServiceBox(bouncePosition);
        }

        else if (servingTeam == 2 && expectedSide == Side.RIGHT)
        {
            isOutside = IsOutsideSouthWestServiceBox(bouncePosition);
        }

        return isOutside;
    }

    private bool IsOutsideSouthEastServiceBox(Vector3 bouncePosition)
    {
        if (bouncePosition.x < _southServiceDelimiter.x)
        {
            return true;
        } 
        
        if (bouncePosition.z > _eastCourtSide.z || bouncePosition.z < _eastServiceDelimiter.z)
        {
            return true;
        }

        return false;
    }

    private bool IsOutsideSouthWestServiceBox(Vector3 bouncePosition)
    {
        if (bouncePosition.x < _southServiceDelimiter.x)
        {
            return true;
        }
        
        if (bouncePosition.z > _westServiceDelimiter.z || bouncePosition.z < _westCourtSide.z)
        {
            return true;
        }

        return false;
    }

    private bool IsOutsideNorthEastServiceBox(Vector3 bouncePosition)
    {
       if (bouncePosition.x > _northServiceDelimiter.x)
       {
                   return true;
       }

       if (bouncePosition.z > _eastCourtSide.z || bouncePosition.z < _eastServiceDelimiter.z)
       {
           return true;
       }

       return false;
    }

    private bool IsOutsideNorthWestServiceBox(Vector3 bouncePosition)
    {
        if (bouncePosition.x > _northServiceDelimiter.x)
        {
            return true;
        }

        if (bouncePosition.z > _westServiceDelimiter.z || bouncePosition.z < _westCourtSide.z)
        {
            return true;
        }

        return false;
    }


    public void SetServing(bool serving)
    {
        _isServing = serving;
        int servingTeam = ScoreManager.GetInstance().GetServingTeam();
       if (servingTeam == 1)
        {
            _player1.SetServing(serving);
        }
        else
        {
            _aiPlayer.SetServing(serving);
            ActivateServingWalls(servingTeam);

        }
        
        if (!serving)
        {
            DeactivateServingWalls(servingTeam);
        }
    }

    public bool GetIsServing()
    {
        return _isServing;
    }

    public void ActivateServingWalls(int id)
    {
       if (id == 1) //south
       {
           _southServiceWall.GetComponent<Collider>().enabled = true;
           _southEastServiceWall.GetComponent<Collider>().enabled = true;
           _southWestServiceWall.GetComponent<Collider>().enabled = true;
           _southMiddleServiceWall.GetComponent<Collider>().enabled = true;
       }
       else
       {
           _northServiceWall.GetComponent<Collider>().enabled = true;
           _northEastServiceWall.GetComponent<Collider>().enabled = true;
           _northWestServiceWall.GetComponent<Collider>().enabled = true;
           _northMiddleServiceWall.GetComponent<Collider>().enabled = true;
       }
    }
    public void DeactivateServingWalls(int id)
    {
        if (id == 1) //south
        {
            _southServiceWall.GetComponent<Collider>().enabled = false;
            _southEastServiceWall.GetComponent<Collider>().enabled = false;
            _southWestServiceWall.GetComponent<Collider>().enabled = false;
            _southMiddleServiceWall.GetComponent<Collider>().enabled = false;
        }
        else
        {
            _northServiceWall.GetComponent<Collider>().enabled = false;
            _northEastServiceWall.GetComponent<Collider>().enabled = false;
            _northWestServiceWall.GetComponent<Collider>().enabled = false;
            _northMiddleServiceWall.GetComponent<Collider>().enabled = false;
        }
    }

    public void MakePlayerServe(int hitterId)
    {
        BallLogic.Instance.DesapearBall();
        SetServing(true);
        if (hitterId == 1)
        {
            _player1.SetServing(true);
            _player1.SetInitialPosition();
            if (!_twoPlayers)
            {
                _aiPlayer.SetServing(false);
                _aiPlayer.Setinitialposition();
            }
            else
            {
                _player2.SetServing(false);
                _player2.SetInitialPosition();
            }
        }
        else if(hitterId == 2)
        {
            _player1.SetServing(false);
            _player1.SetInitialPosition();
            if (!_twoPlayers)
            {
                _aiPlayer.SetServing(true);
                _aiPlayer.Setinitialposition();
            }
            else
            {
                _player2.SetServing(true);
                _player2.SetInitialPosition();
            }
        }
    }

    public int GetServiceTimes()
    {
        return _serviceTimes;
    }

    public void IncreaseServiceTimes()
    {
        _serviceTimes++;
    }

    public void ResetServiceTimes()
    {
        _serviceTimes = 0;
    }
}
