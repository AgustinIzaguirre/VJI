﻿using System;
using FrameLord;
using Game.Player;
using Game.Score;
using UnityEngine;
using Random = UnityEngine.Random;


public class AIPlayer : MonoBehaviour
{
    public Transform ballPosition;

    //Ball to be animated on serve
    public GameObject animatableServeBallPrefab;

    private float _speed = 10;
    
    public Transform aimTarget;

    public Transform otherPlayer;
    public int difficulty;
    private AIStrategy _AIStrategy;
    
    private CharacterController _characterController;

    private PlayerAnimation _playerAnimation;
    private Vector3 _basePositionFromBall;
    private Vector3 _desiredPosition;
    private Vector3 _serveTarget;
    private ScoreManager _scoreManager;
    private bool _newPosition;
    private bool _isServing;
    private float _elapsedTime;
    private float _timeToServe;
    private float _timeToBounce = 1.8f;
    private GameObject _animatedServingBall;
    /* player id according to court side,
    * 1 if player is on team one or
    * 2 if player is on team two
   */
    private int _id;
    
    void Start()
    {
        _isServing = false;
        _AIStrategy = new AIStrategy(otherPlayer);
        _characterController = GetComponent<CharacterController>();
        _playerAnimation =  new PlayerAnimation(GetComponent<Animator>());
        _basePositionFromBall = new Vector3(7.705f,0f,0.633f);
        _newPosition = true;
        _scoreManager = ScoreManager.GetInstance();
        _timeToServe = 0;
        difficulty = difficulty > 4 ? 4 : difficulty;
        difficulty = difficulty < 1 ? 1 : difficulty;
        
        
        if (transform.position.x < 0)
        {
            _id = 1;
        }
        else
        {
            _id = 2;
        }
        Setinitialposition();
        SetDifficulty(ScoreManager.GetInstance().GetGameDifficulty());
        SetSpeed();
        SetTimeToBounce();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isServing)
        {
            if (!_playerAnimation.isCelebratingOrAngry())
            {
                if (Math.Abs(_timeToServe) < 0.01)
                {
                    _timeToServe = Random.Range(0.0f, 1.0f) + 1.0f;
                }

                _elapsedTime = _elapsedTime + Time.deltaTime;
                if (_elapsedTime >= _timeToServe)
                {
                    AimServe();
                    _elapsedTime = 0;
                    _timeToServe = 0;
                    SetServing(false);
                }
            }

        }
        else
        {
            BallLogic ballLogic = BallLogic.Instance;
            bool hasMoved = false;
            if (ballLogic.IsEnabled() && ballLogic.GetHittingPlayer() != _id
                                      && ballLogic.GetHittingPlayer() != 0)
            {
                hasMoved = MoveToBall();
            }

            if (!hasMoved)
            {
                _playerAnimation.StartMoveAnimation(MovementDirection.IDLE);
            }
        }
    }

    private void AimServe()
    {
        Side servingSide = _scoreManager.GetServingSide();
        _serveTarget = _AIStrategy.GetServeTarget(servingSide);
        _playerAnimation.StartServeAnimation();
        _animatedServingBall = Instantiate(animatableServeBallPrefab, transform.position + Vector3.up * animatableServeBallPrefab.GetComponent<BallServeAnimation>().verticalAppearOffset, Quaternion.identity);
    }

    public void Serve()
    {
        Vector3 currentPosition = transform.position;
        BallLogic ball = BallLogic.Instance;
        ball.AppearBall(new Vector3(currentPosition.x + 0.1f, 4.05f, currentPosition.z), Vector3.zero );
        Vector3 ballVelocity = ball.GetVelocity(_serveTarget, 1.5f);
        ball.GetComponent<Rigidbody>().velocity = ballVelocity;
        BallLogic.Instance.SetHittingPlayer(_id);
        Destroy(_animatedServingBall);
    }
    private bool MoveToBall()
    {
        if (_newPosition)
        {
            _desiredPosition = BallLogic.Instance.GetBouncePosition();
            _desiredPosition = _desiredPosition + _basePositionFromBall;
            if (difficulty > 1 && BallLogic.Instance.GetCurrentVelocity().z < 0)
            {
                _desiredPosition = _desiredPosition + new Vector3(0, 0, -1.5f);
            }
            if (difficulty == 4 && BallLogic.Instance.GetCurrentVelocity().z > 0)
            {
                _desiredPosition = _desiredPosition + new Vector3(0, 0, 1.0f);
            }
            _newPosition = false;
        }

        if (_desiredPosition.x < 0)
        {
            return false ;
        }
        
        if (Math.Abs(transform.position.x - _desiredPosition.x) > 0.05 ||
            Math.Abs(transform.position.z - _desiredPosition.z) > 0.05)
        {
            float xDirection = _desiredPosition.x - transform.position.x;
            float zDirection = _desiredPosition.z - transform.position.z;
            Vector3 movingDirection = new Vector3(xDirection, transform.position.y, zDirection).normalized;
            _characterController.Move(Time.deltaTime * _speed * movingDirection);
            _playerAnimation.StartMoveAnimation(GetMovementDirection(movingDirection));
            return true;
        }

        return false;
    }

    private MovementDirection GetMovementDirection(Vector3 movingDirection)
    {
        //prioritize LEFT and RIGHT over UP and DOWN because it looks better on animation
        if (ballPosition.position.z - transform.position.z < 0)
        {
            return MovementDirection.LEFT;
        }
        else if (ballPosition.position.z - transform.position.z > 0)
        {
            return MovementDirection.RIGHT;
        }
        else if ((ballPosition.position.x - transform.position.x < 0))
        {
            return MovementDirection.UP;
        }
        else if (ballPosition.position.z - transform.position.z > 0)
        {
            return MovementDirection.DOWN;
        }
        else
        {
            return MovementDirection.IDLE;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            _playerAnimation.StartHittingAnimation(Side.RIGHT);
            HitBall();
        }
    }

    public void SetServing(bool serving)
    {
        _isServing = serving;
        
    }

    public void DoNothing()
    {
        
    }
    private void HitBall()
    {
        BallLogic ball = BallLogic.Instance;
        Vector3 aimPosition = _AIStrategy.GeneratePositionBasedOnDifficulty(difficulty);
        Vector3 velocity = BallLogic.Instance.GetVelocity(aimPosition, _timeToBounce);//change time in function of currentHitForce
        AudioManager.Instance.PlaySound(ball.transform.position, (int) SoundId.SOUND_HIT);
        ball.GetComponent<Rigidbody>().velocity = velocity;
        ball.SetHittingPlayer(_id);
        _newPosition = true;
    }
    
    private void PlayServeSound()
    {
        AudioManager.Instance.PlaySound(transform.position, (int) SoundId.SOUND_SERVE);
    }
    
    private void DeleteBallReference()
    {
        // its here just to use same animation as player 1
    }

    public void Setinitialposition()
    {
        _newPosition = true;
        Vector3 currentPosition = transform.position;

        float x, z; 
        Side servingSide = _scoreManager.GetServingSide();
        if (servingSide == Side.RIGHT)
        {
            z = 6.57f;
        }
        else
        {
            z = -7.24f;
        }

        if (_isServing)
        {
            ScoreManager.GetInstance().ActivateServingWalls(_id);
            x = 32f;
        }
        else
        {
            ScoreManager.GetInstance().DeactivateServingWalls(_id);
            x = 25.5f;
        }

        _characterController.enabled = false;
        Vector3 newPosition = new Vector3(x, currentPosition.y, z);
        transform.position = newPosition;
        _characterController.enabled = true;

    }

    public void Celebrate()
    {
        _playerAnimation.StartCelebrateAnimation();
    }

    public void Angry()
    {
        _playerAnimation.StartAngryAnimation();
    }
    
    
    private void SetSpeed()
    {
        switch (difficulty)
        {
            case 1:
                _speed = 6f;
                break;
            case 2:
                _speed = 8f;
                break;
            case 3:
                _speed = 10f;
                break;
            case 4:
                _speed = 12f;
                break;
        }
    }

    private void SetTimeToBounce()
    {
        switch (difficulty)
        {
            case 1:
                _timeToBounce = 2.5f;
                break;
            case 2:
                _timeToBounce = 2.0f;
                break;
            case 3:
                _timeToBounce = 1.8f;
                break;
            case 4:
                _timeToBounce = 1.7f;
                break;
        }
    }

    public void SetDifficulty(int currentDifficulty)
    {
        difficulty = currentDifficulty;
    }
}