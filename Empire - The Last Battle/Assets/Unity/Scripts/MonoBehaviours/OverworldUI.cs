﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OverworldUI : MonoBehaviour
{
    public delegate void BoardAction(TileData tile);
    public event BoardAction OnCommanderMove = delegate { };

    public event System.Action OnPause = delegate { };
    public event System.Action OnUnPause = delegate { };

    public bool _TileHover;
    public CommanderUI _CommanderUI;
    public CameraMovement _CameraMovement;
    public BoardUI _BoardUI;
    public GameObject _PauseScreen;
    public CardDisplayUI _CardDisplayUI;

    bool _paused;
    public bool _Paused
    {
        get { return _paused; }
        set
        {
            if (value)
            {
                _PauseScreen.SetActive(true);
                Disable();
            }
            else
            {
                _PauseScreen.SetActive(false);
                Enable();
            }

            _paused = value;
        }
    }

    // Use this for initialization
    public void Initialise()
    {
        _BoardUI.Init();
		_CommanderUI.Initialise();
        _CardDisplayUI.Init();

        //add event listeners
        Enable();
    }

    public void Disable()
    {
        //remove event listeners
        _CommanderUI.OnCommanderMoved -= _CommanderUI_OnCommanderMoved;
        _CommanderUI.OnStartDrag -= _CommanderUI_OnStartDrag;
        _CommanderUI.OnCommanderDrop -= _CommanderUI_OnCommanderDrop;
        _CommanderUI.OnCommanderGrounded -= _CommanderUI_Grounded;
        _CommanderUI.OnDropCommander -= _CommanderUI_OnDropCommander;
        _CardDisplayUI.OnCardUse -= _CardDisplayUI_OnCardUse;

        //disable components
        _CommanderUI._Paused = true;
        _CameraMovement.DisableCameraMovement();
    }

    public void Enable()
    {
        //add event listeners
        _CommanderUI.OnCommanderMoved += _CommanderUI_OnCommanderMoved;
        _CommanderUI.OnStartDrag += _CommanderUI_OnStartDrag;
        _CommanderUI.OnCommanderDrop += _CommanderUI_OnCommanderDrop;
        _CommanderUI.OnCommanderGrounded += _CommanderUI_Grounded;
        _CommanderUI.OnDropCommander += _CommanderUI_OnDropCommander;
        _CardDisplayUI.OnCardUse += _CardDisplayUI_OnCardUse;

        //enable components
        _CommanderUI._Paused = false;
        _CameraMovement.EnableCameraMovement();
    }

    public void _CardDisplayUI_OnCardUse(CardData cardData)
    {
        Debug.Log("CardData: "+cardData.name);
    }

    void _CommanderUI_OnDropCommander(TileData tile)
    {
        _BoardUI.PlayerPrompt_DefaultTiles();
    }

	void _CommanderUI_Grounded()
	{
		//if camera is not moving to a new position then enable it 
		if (!_CameraMovement.IsLerping ())
			_CameraMovement.EnableCameraMovement ();
	}

	void _CommanderUI_OnCommanderDrop(Vector3 vec)
	{
		_CameraMovement.EnableCameraMovement(vec);
	}

    void _CommanderUI_OnStartDrag()
    {
        _CameraMovement.DisableCameraMovement();
        _BoardUI.PlayerPrompt_MovableTiles(_CommanderUI.GetReachableTiles());
    }

    void _CommanderUI_OnCommanderMoved(TileData tile)
    {
        OnCommanderMove(tile);
    }

    public void AllowPlayerMovement(HashSet<TileData> reachableTiles)
    {
        _CommanderUI.AllowPlayerMovement(reachableTiles);
    }

    public void DisablePlayerMovement()
    {
        _CommanderUI.DisablePlayerMovement();
    }

    public void UpdateCommanderPosition()
    {
        _CommanderUI.UpdateToPlayerPosition();
    }

    public void PauseScreenClickHandler()
    {
        OnUnPause();
    }

    public void SetPlayerFocus(Player player)
    {
        //set up arpropriate ui
        _CommanderUI = player.GetComponent<CommanderUI>();

        //update display
        _CommanderUI.DisplayInfo();

        //focus camera
        SwitchFocus(player.transform);
    }

	public void SwitchFocus(Transform transform){
		_CameraMovement._TargetObject = transform;
		_CameraMovement.EnableCameraMovement(new Vector3(transform.position.x, transform.position.y, transform.position.z));
	}

	public void AddPlayerCard(PlayerType pType, CardData cData)
	{
		//update the hand ui here
		if (_CommanderUI._Player.Type == pType) 
		{
			_CommanderUI.DisplayInfo();
		}
	}

	public void RemovePlayerCard(PlayerType pType, CardData cData)
	{
		//update the hand ui here
		if (_CommanderUI._Player.Type == pType) 
		{
			_CommanderUI.DisplayInfo();
		}
	}

    void Update()
    {
        //check for pause switch by key 
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_Paused)
                OnUnPause();
            else
                OnPause();
        }
    }
}
