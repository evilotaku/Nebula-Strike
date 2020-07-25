/* Written by Kaz Crowe */
/* AlternateStateHandler.cs */
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Ultimate UI/Alternate State Handler" )]
public class AlternateStateHandler : MonoBehaviour
{
	// ----- < ULTIMATE STATUS BAR > ----- //
	public UltimateStatusBar ultimateStatusBar;

	// ----- < ALTERNATE STATES > ----- //
	[Serializable]
	public class AlternateState
	{
		// ULTIMATE STATUS //
		public int statusIndex = 0;
		public string alternateStateName = "";
		public Color defaultStateColor = Color.white;
		public enum TriggerOption
		{
			Manual,
			Percentage
		}
		public TriggerOption triggerOption = TriggerOption.Manual;
		public Image alternateStateImage;
		public Color alternateStateColor = Color.white;
		public bool flashing = false;
		public Color flashingColor = Color.white;
		public float flashingSpeed = 1.0f;
		public float flashingDuration = 0.0f;
		public event Action<AlternateState> flashingEvent;
		public enum TriggerBy
		{
			LessThan,
			GreaterThan,
			EqualTo
		}
		public TriggerBy triggerBy = TriggerBy.LessThan;
		public float triggerValue = 0.0f;

		public enum StateType
		{
			Image,
			Text
		}
		public StateType stateType = StateType.Image;
		public Text alternateStateText;

		bool _currentState = false;
		public bool currentState
		{
			get
			{
				return _currentState;
			}
		}


		/// <summary>
		/// Switches the current state of this Alternate State.
		/// </summary>
		/// <param name="state">The target state to switch to.</param>
		public void SwitchState ( bool state )
		{
			if( state == true && _currentState == false )
			{
				_currentState = true;

				if( flashing == true )
					flashingEvent( this );
				else
					ApplyColor( alternateStateColor );
			}
			else if( state == false && _currentState == true )
			{
				_currentState = false;

				if( flashing == false )
					ApplyColor( defaultStateColor );
			}
		}
		
		/// <summary>
		/// Applies the color variable to the alternate state image or text.
		/// </summary>
		/// <param name="col">The target color to apply.</param>
		public void ApplyColor ( Color col )
		{
			if( stateType == StateType.Image && alternateStateImage != null )
				alternateStateImage.color = col;
			else if( stateType == StateType.Text && alternateStateText != null )
				alternateStateText.color = col;
		}
		
		/// <summary>
		/// This function is called from the Handler each time the targeted Ultimate Status is updated.
		/// </summary>
		/// <param name="amt">The percentage value of the targeted Ultimate Status.</param>
		public void OnStatusUpdated ( float amt )
		{
			switch( triggerBy )
			{
				case TriggerBy.LessThan:
				{
					if( amt < triggerValue && _currentState == false )
					{
						_currentState = true;
						if( flashing == true )
							flashingEvent( this );
						else
							ApplyColor( alternateStateColor );
					}
					else if( amt > triggerValue && _currentState == true )
					{
						_currentState = false;
						if( flashing == false )
							ApplyColor( defaultStateColor );
					}
				}break;
				case TriggerBy.GreaterThan:
				{
					if( amt > triggerValue && _currentState == false )
					{
						_currentState = true;
						if( flashing == true )
							flashingEvent( this );
						else
							ApplyColor( alternateStateColor );
					}
					else if( amt < triggerValue && _currentState == true )
					{
						_currentState = false;
						if( flashing == false )
							ApplyColor( defaultStateColor );
					}
				}break;
				case TriggerBy.EqualTo:
				{
					if( amt == triggerValue && _currentState == false )
					{
						_currentState = true;
						if( flashing == true )
							flashingEvent( this );
						else
							ApplyColor( alternateStateColor );
					}
					else if( amt != triggerValue && _currentState == true )
					{
						_currentState = false;
						if( flashing == false )
							ApplyColor( defaultStateColor );
					}
				}break;
				default:
				{
					Debug.Log( "Something went wrong with the selection of the Trigger Option." );
				}break;
			}
		}
	}
	public List<AlternateState> AlternateStateList = new List<AlternateState>();
	Dictionary<string, AlternateState> AlternateStateDict = new Dictionary<string, AlternateState>();
	static Dictionary<string,AlternateStateHandler> AlternateStatusHandlerDict = new Dictionary<string, AlternateStateHandler>();


	void OnEnable ()
	{
		for( int i = 0; i < AlternateStateList.Count; i++ )
		{
			if( AlternateStateList[ i ].flashing == true )
				AlternateStateList[ i ].flashingEvent += ReceiveFlashingRequest;

			if( AlternateStateList[ i ].triggerOption == AlternateState.TriggerOption.Percentage )
				ultimateStatusBar.UltimateStatusList[ AlternateStateList[ i ].statusIndex ].OnStatusUpdated += AlternateStateList[ i ].OnStatusUpdated;
		}
	}

	void OnDisable ()
	{
		for( int i = 0; i < AlternateStateList.Count; i++ )
		{
			if( AlternateStateList[ i ].flashing == true )
				AlternateStateList[ i ].flashingEvent -= ReceiveFlashingRequest;

			if( AlternateStateList[ i ].triggerOption == AlternateState.TriggerOption.Percentage )
				ultimateStatusBar.UltimateStatusList[ AlternateStateList[ i ].statusIndex ].OnStatusUpdated -= AlternateStateList[ i ].OnStatusUpdated;
		}
	}

	void Awake ()
	{
		if( ultimateStatusBar.statusBarName != string.Empty )
		{
			if( AlternateStatusHandlerDict.ContainsKey( ultimateStatusBar.statusBarName ) )
				AlternateStatusHandlerDict.Remove( ultimateStatusBar.statusBarName );

			AlternateStatusHandlerDict.Add( ultimateStatusBar.statusBarName, this );
		}

		for( int i = 0; i < AlternateStateList.Count; i++ )
		{
			if( AlternateStateList[ i ].alternateStateName != string.Empty )
				AlternateStateDict.Add( AlternateStateList[ i ].alternateStateName, AlternateStateList[ i ] );
		}
	}

	void ReceiveFlashingRequest ( AlternateState alternateState )
	{
		StartCoroutine( AlternateStateFlashing( alternateState ) );
	}

	IEnumerator AlternateStateFlashing ( AlternateState alternateState )
	{
		float flashDuration = alternateState.flashingDuration;
		float step = -90.0f;
		Color col = alternateState.alternateStateColor;
		// This is multiplying by 6 in order to represent a "per second" option. No idea why the number 6.
		float flashingSpeed = alternateState.flashingSpeed * 6;

		while( alternateState.currentState == true )
		{
			if( alternateState.flashingDuration > 0 )
			{
				flashDuration -= Time.deltaTime;

				if( flashDuration <= 0 )
					alternateState.SwitchState( false );
			}
			step += Time.deltaTime * flashingSpeed;
			if( step > 270 )
				step -= 360;

			col = Color.Lerp( alternateState.alternateStateColor, alternateState.flashingColor, ( Mathf.Sin( step ) + 1 ) / 2 );
			alternateState.ApplyColor( col );
			yield return null;
		}
		alternateState.ApplyColor( alternateState.defaultStateColor );
	}

	/* ----------------------------------< PUBLIC FUNCTIONS >----------------------------------- */
	/// <summary>
	/// Switches the targeted Alternate State to the desired state.
	/// </summary>
	/// <param name="stateName">The name of the state.</param>
	/// <param name="state">The state to apply to the Alternate State.</param>
	public void SwitchState ( string stateName, bool state )
	{
		if( !ConfirmAlternateState( stateName ) )
			return;

		AlternateStateDict[ stateName ].SwitchState( state );
	}

	/// <summary>
	/// Returns the Alternate State that has been registered with the stateName.
	/// </summary>
	/// <param name="stateName">The name of the state.</param>
	public AlternateState GetAlternateState ( string stateName )
	{
		if( !ConfirmAlternateState( stateName ) )
			return null;

		return AlternateStateDict[ stateName ];
	}

	bool ConfirmAlternateState ( string stateName )
	{
		if( AlternateStateDict.ContainsKey( stateName ) )
			return true;

		Debug.LogError( "Alternate State Handler - No State has been registered with the name: " + stateName + "." );
		return false;
	}
	/* ---------------------------------< END PUBLIC FUNCTIONS >-------------------------------- */

	/* -------------------------------< PUBLIC STATIC FUNCTIONS >------------------------------- */
	/// <summary>
	/// Switches the targeted Alternate State to the desired state.
	/// </summary>
	/// <param name="statusBarName">The name of the Ultimate Status Bar associated with the Alternate State Handler.</param>
	/// <param name="stateName">The name the desired state to update.</param>
	/// <param name="state">The targeted state.</param>
	public static void SwitchState ( string statusBarName, string stateName, bool state )
	{
		if( !ConfirmAlternateStateHandler( statusBarName ) )
			return;

		AlternateStatusHandlerDict[ statusBarName ].SwitchState( stateName, state );
	}

	/// <summary>
	/// Returns the Alternate State that has been registered with the statusBarName.
	/// </summary>
	/// <param name="statusBarName">The name of the Ultimate Status Bar associated with the Alternate State Handler.</param>
	public static AlternateStateHandler GetAlternateStateHandler ( string statusBarName )
	{
		if( !ConfirmAlternateStateHandler( statusBarName ) )
			return null;

		return AlternateStatusHandlerDict[ statusBarName ];
	}

	static bool ConfirmAlternateStateHandler ( string statusBarName )
	{
		if( AlternateStatusHandlerDict.ContainsKey( statusBarName ) )
			return true;

		Debug.LogError( "Alternate State Handler - No Alternate State Handler has been registered with the Ultimate Status Bar name: " + statusBarName + "." );
		return false;
	}
	/* -----------------------------< END PUBLIC STATIC FUNCTIONS >----------------------------- */
}