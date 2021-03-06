﻿using System;
using System.Collections;
using System.Collections.Generic;
using FrameLord;
using Game.Input;
using Game.Score;
using TMPro;
using UnityEngine;

public class PlayerLogic : MonoBehaviour
{
    // Button to move left
    public KeyCode leftButton = KeyCode.LeftArrow;

    // Button to move right
    public KeyCode rightButton = KeyCode.RightArrow;

    // Button to move forward
    public KeyCode forwardButton = KeyCode.UpArrow;

    // Button to move backward
    public KeyCode bacwardButton = KeyCode.DownArrow;

    // Button to hit
    public KeyCode hitButton = KeyCode.A;

    //Ball to be animated on serve
    public GameObject animatableServeBallPrefab;
    
    public delegate void OnInitialPositionSetDelegate();
    public OnInitialPositionSetDelegate initialPositionSetEvent;

    public Transform aimTarget;
    public float aimTargetSpeed = 18;
    public MeshRenderer aimTargetRenderer;
    public float movementSpeed = 14f;
    private float _currentHitForce;
    private float _playerSpeed = 50f;
    private float _maxReach;
    public float minHitForce = 18f;
    public float deltaHitForce = 20.0f; // How much force is added per second while charging
     public float maxHitForce = 40f;

    public float _minimumOffSetToHitBall = -3.0f;

    protected ScoreManager _scoreManager;
    private Side _ballSide;
    protected bool _isServing;
    
    // Is true after hit button is pressed
    protected bool _isCharging;

    // Is true after hit button is released;
    private bool _finishHitting;

    // Moving Left or Right (-1: left, 1: right, 0: none)
    private int moveLeftRightValue;

    // Moving Up or Down (-1: down, 1: up, 0: none)
    private int moveForwardBackwardValue;
    
    protected CharacterController _characterController;

    // Start position of aim target
    private Vector3 _aimStartPosition;

    /* player id according to court side,
     * 1 if player is on team one or
     * 2 if player is on team two
    */
    protected int _id;

    private GameObject _ball;

    private PlayerAnimation _playerAnimation;

    private GameObject _animatedServingBall;
    private bool _twoPlayers = false;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _isCharging = false;
        _finishHitting = false;
        moveLeftRightValue = 0;
        moveForwardBackwardValue = 0;
        _ballSide = Side.RIGHT;
        _currentHitForce = minHitForce;
        _playerAnimation =  new PlayerAnimation(GetComponent<Animator>());
        _aimStartPosition = aimTarget.position;
        _maxReach = 5f;
        _scoreManager = ScoreManager.GetInstance();
        SetID();
        SetIsServing();
        SetInitialPosition();
        if (_isServing)
        {
            ScoreManager.GetInstance().ActivateServingWalls(_id);
        }
        BallLogic.Instance.DesapearBall();

    }

    public virtual void SetInitialPosition()
    {
        Vector3 currentPosition = transform.position;
        _isCharging = false;
        ResetToIdle();
        if (_twoPlayers)
        {
            DisableTarget();
        }
        float x, z; 
        Side servingSide = _scoreManager.GetServingSide();
        if (servingSide == Side.RIGHT)
        {
            z = -5f;
        }
        else
        {
            z = 5f;
        }

        if (_isServing)
        {
            SetAimPosition(servingSide);
            ScoreManager.GetInstance().ActivateServingWalls(_id);
            x = -32f;
        }
        else
        {
            ScoreManager.GetInstance().DeactivateServingWalls(_id);
            x = -27f;
        }

        _characterController.enabled = false;
        Vector3 newPosition = new Vector3(x, currentPosition.y, z);
        transform.position = newPosition;
        _characterController.enabled = true;
        initialPositionSetEvent();
    }

    public virtual void SetAimPosition(Side servingSide)
    {
        float x = 12.0f, z;
        if (servingSide == Side.RIGHT)
        {
            z = 7.1f;
        }
        else
        {
            z = -5.1f;
        }
        aimTarget.position = new Vector3(x, aimTarget.position.y, z);
    }

    private void SetIsServing()
    {
        _isServing = _id == _scoreManager.GetServingTeam();
    }

    private void SetID()
    {
        if (transform.position.x < 0)
        {
            _id = 1;
        }
        else
        {
            _id = 2;
        }

    }

    void Update()
    {
        if (!_playerAnimation.IsPlayingHitAnimation())
        {
            ReadInput();

            if (!_isCharging)
            {
                UpdatePosition();
            }
            else
            {
                UpdateAimTargetPosition();
            }
        }
    }

    private void ReadInput()
    {
        moveLeftRightValue = 0;
        moveForwardBackwardValue = 0;
        _finishHitting = false;
        BallLogic ball = BallLogic.Instance;


        if (ActionMapper.GetMoveLeft(leftButton))
        {
            moveLeftRightValue += -1;
        }

        if (ActionMapper.GetMoveRight(rightButton))
        {
            moveLeftRightValue += 1;
        }

        if (ActionMapper.GetMoveForward(forwardButton))
        {
            moveForwardBackwardValue += 1;
        }

        if (ActionMapper.GetMoveBackward(bacwardButton))
        {
            moveForwardBackwardValue += -1;
        }

        if (ActionMapper.GetHitPressed(hitButton) && ball.GetHittingPlayer() != _id)
        {
            if (!_isCharging && _isServing)
            {
                _currentHitForce = minHitForce;
                _isCharging = true;
            }
            else if (!_isCharging && ball.GetHittingPlayer() != 0)
            {
                _currentHitForce = minHitForce;
                if (!_isServing)
                {
                    aimTarget.position = _aimStartPosition;
                    _ballSide = BallLogic.Instance.GetSide(transform.position);
                    _playerAnimation.StartHittingAnimation(_ballSide);
                }
                _isCharging = true;
            }
            else if(_isCharging)
            {
                _currentHitForce += deltaHitForce * Time.deltaTime;
                _currentHitForce = Math.Min(_currentHitForce, maxHitForce);
            }
        }

        if (ActionMapper.GetHitReleased(hitButton))
        {
            if (_isServing && ball.GetHittingPlayer() == 0)
            {
                _isCharging = false;
                _playerAnimation.StartServeAnimation();

                _animatedServingBall = Instantiate(animatableServeBallPrefab, transform.position + Vector3.up*animatableServeBallPrefab.GetComponent<BallServeAnimation>().verticalAppearOffset, Quaternion.identity);
            }
           
        }
    }

    private void UpdatePosition()
    {
        float leftRightMove = movementSpeed * moveLeftRightValue * Time.deltaTime;
        float forwardBackardMove = movementSpeed * moveForwardBackwardValue * Time.deltaTime;
        _playerAnimation.AnimateMovement(leftRightMove, forwardBackardMove);
        var vec = new Vector3(forwardBackardMove, 0, -leftRightMove);
        _characterController.SimpleMove(vec * _playerSpeed);
    }

   

    private void UpdateAimTargetPosition()
    {
        aimTarget.Translate(new Vector3(aimTargetSpeed * moveForwardBackwardValue * Time.deltaTime, 0,
            -aimTargetSpeed * moveLeftRightValue * Time.deltaTime));
    }
    
    private bool IsValidBallPositionToHit(GameObject ballToHit)
    {
        return (ballToHit.transform.position - transform.position).x >= _minimumOffSetToHitBall;
    }
    
    private void HitBall()
    {
        if (_ball != null && _isCharging)
        {
            AudioManager.Instance.PlaySound(_ball.transform.position, (int) SoundId.SOUND_HIT);
            float time = GetTimeToBounce(1.0f, 2.5f);
           Vector3 velocity = BallLogic.Instance.GetVelocity(aimTarget.position, time);
           _ball.GetComponent<Rigidbody>().velocity = velocity;
            _currentHitForce = minHitForce;
            BallLogic.Instance.SetHittingPlayer(_id);
            BallLogic.Instance.ballHitDelegate(_id);
        }

        _finishHitting = true;
        _isCharging = false;
    }

    private float GetTimeToBounce(float minTime, float maxTime)
    {
        float difference = maxTime - minTime;
        float value = _currentHitForce - minHitForce;
        float totalForce = maxHitForce - minHitForce;
        float percentage = 1.0f - (value / totalForce);
        return difference * percentage + minTime;
    }

    private void Serve()
    {
        Vector3 currentPosition = transform.position;
        BallLogic ball = BallLogic.Instance;
        ball.AppearBall(new Vector3(currentPosition.x + 0.1f, 4.05f, currentPosition.z), Vector3.zero);
        float time = GetTimeToBounce(0.8f, 2.0f);
        Vector3 velocity = BallLogic.Instance.GetVelocity(aimTarget.position, time);
        ball.GetComponent<Rigidbody>().velocity = velocity;
        BallLogic.Instance.SetHittingPlayer(_id);
        Destroy(_animatedServingBall);
    }


    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Ball"))
        {
            _ball = null;
            _playerAnimation.EndHittingAnimation();
            float distance = BallLogic.Instance.GetDistance(transform.position);

            if (distance <= _maxReach && BallLogic.Instance.GetHeight() < 3.85f)
            {
                _ball = other.gameObject;
                HitBall();
            }
        }
    }
    
    public void SetServing(bool serving)
    {
        _isServing = serving;
    }

    public int GetId()
    {
        return _id;
    }

    private void PlayServeSound()
    {
        AudioManager.Instance.PlaySound(transform.position, (int) SoundId.SOUND_SERVE);
    }

    public float GetCurrentHitForce()
    {
        return _currentHitForce;
    }

    public bool IsChargingHit()
    {
        return _isCharging;
    }

    public bool IsServing()
    {
        return _isServing;
    }

    public void ResetToIdle()
    {
        if (_playerAnimation.IsStuckOnHitAnimation())
        {
            _playerAnimation.EndHittingAnimation();
        } 
    }

    public void Celebrate()
    {
        _playerAnimation.StartCelebrateAnimation();
    }
    
    
    public void Angry()
    {
        _playerAnimation.StartAngryAnimation();
    }

    public void DisableTarget()
    {
        aimTargetRenderer.enabled = false;
    }
    public void EnableTarget()
    {
        aimTargetRenderer.enabled = true;
    }

    public void SetGameMode(bool twoPlayers)
    {
        _twoPlayers = twoPlayers;
    }

    public bool IsTwoPlayers()
    {
        return _twoPlayers;
    }
}
