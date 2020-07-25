/* Written by Kaz Crowe */
/* AlternateStateHandlerEditor.cs */
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Collections.Generic;

[CustomEditor( typeof( AlternateStateHandler ) )]
public class AlternateStateHandlerEditor : Editor
{
	AlternateStateHandler targ;
	List<string> statusOptions, alternateStates;
	int selectionIndex = 0;

	SerializedProperty ultimateStatusBar;
	List<SerializedProperty> statusIndex, alternateStateName;
	List<SerializedProperty> triggerOption;
	List<SerializedProperty> alternateStateImage;
	List<SerializedProperty> alternateStateColor;
	List<SerializedProperty> flashing;
	List<SerializedProperty> flashingColor;
	List<SerializedProperty> flashingSpeed;
	List<SerializedProperty> triggerValue;
	List<SerializedProperty> triggerBy;
	List<SerializedProperty> stateType;
	List<SerializedProperty> alternateStateText;
	List<SerializedProperty> defaultStateColor;
	List<SerializedProperty> flashingDuration;
	
	AnimBool AlternateStates, ScriptReference;

	AnimBool StatusBarAssigned, StatusBarUnassigned, StatusIndexUnassigned;

	List<AnimBool> AlternateStateBase, AlternateStateAdvanced;
	List<AnimBool> DefaultColorHelp;
	List<AnimBool> TriggerOptionPercentage;
	List<AnimBool> StateTypeImage, StateTypeText;
	List<AnimBool> AlternateStateAdvancedFlashing;
	List<AnimBool> AlternateStateNameError;

	// ERRORS AND WARNINGS //
	AnimBool ScriptReferenceError, ScriptReferenceExampleCode;
	AnimBool ScriptReferenceStates, ScriptReferenceErrorNoStates;


	void OnEnable ()
	{
		StoreReferences();
		
		AlternateStateBase = new List<AnimBool>();
		AlternateStateAdvanced = new List<AnimBool>();

		for( int i = 0; i < targ.AlternateStateList.Count; i++ )
		{
			AlternateStateBase.Add( new AnimBool( true ) );
			AlternateStateAdvanced.Add( new AnimBool( EditorPrefs.GetBool( "UUI_USB_ALT_Advanced" + i.ToString() ) ) );
		}

		Undo.undoRedoPerformed += UndoRedoCallback;
	}

	void OnDisable ()
	{
		Undo.undoRedoPerformed -= UndoRedoCallback;
	}

	void UndoRedoCallback ()
	{
		StoreReferences();
	}
	
	void DisplayHeaderDropdown ( string headerName, string editorPref, AnimBool targetAnim )
	{
		EditorGUILayout.BeginVertical( "Toolbar" );
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField( headerName, EditorStyles.boldLabel );
		if( GUILayout.Button( EditorPrefs.GetBool( editorPref ) == true ? "Hide" : "Show", EditorStyles.miniButton, GUILayout.Width( 50 ), GUILayout.Height( 14f ) ) )
		{
			EditorPrefs.SetBool( editorPref, EditorPrefs.GetBool( editorPref ) == true ? false : true );
			targetAnim.target = EditorPrefs.GetBool( editorPref );
		}
		GUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
	}
		
	public override void OnInspectorGUI ()
	{
		serializedObject.Update();
		EditorGUILayout.Space();

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField( ultimateStatusBar );
		if( EditorGUI.EndChangeCheck() )
		{
			serializedObject.ApplyModifiedProperties();
			StatusBarAssigned.target = targ.ultimateStatusBar != null;
			StatusBarUnassigned.target = targ.ultimateStatusBar == null;

			ScriptReferenceExampleCode.value = targ.ultimateStatusBar != null && targ.ultimateStatusBar.statusBarName != string.Empty;
			ScriptReferenceError.value = targ.ultimateStatusBar != null && targ.ultimateStatusBar.statusBarName == string.Empty;
			
			StoreStatusOptions();
		}
		
		#region STATUS BAR UNASSIGNED
		if( EditorGUILayout.BeginFadeGroup( StatusBarUnassigned.faded ) )
		{
			EditorGUILayout.BeginVertical( "Box" );
			EditorGUILayout.HelpBox( "Please assign the targeted Ultimate Status Bar before continuing.", MessageType.Warning );
			if( GUILayout.Button( "Find Status Bar" ) )
			{
				targ.ultimateStatusBar = targ.gameObject.GetComponentInParent<UltimateStatusBar>();
				EditorUtility.SetDirty( targ );
				
				StatusBarAssigned.target = targ.ultimateStatusBar != null;
				StatusBarUnassigned.target = targ.ultimateStatusBar == null;

				ScriptReferenceExampleCode.value = targ.ultimateStatusBar != null && targ.ultimateStatusBar.statusBarName != string.Empty;
				ScriptReferenceError.value = targ.ultimateStatusBar != null && targ.ultimateStatusBar.statusBarName == string.Empty;

				StoreStatusOptions();

				if( targ.ultimateStatusBar == null )
					Debug.LogWarning( "Alternate State Handler - Could not find an Ultimate Status Bar component in any parent GameObjects." );
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndFadeGroup();
		#endregion

		if( EditorGUILayout.BeginFadeGroup( StatusBarAssigned.faded ) )
		{
			EditorGUILayout.Space();

			#region ALTERNATE STATES
			/* -------------------------------------- > ALTERNATE STATES < -------------------------------------- */
			DisplayHeaderDropdown( "Alternate States", "UUI_ASH_AlternateStates", AlternateStates );
			if( EditorGUILayout.BeginFadeGroup( AlternateStates.faded ) )
			{
				for( int i = 0; i < targ.AlternateStateList.Count; i++ )
				{
					if( EditorGUILayout.BeginFadeGroup( AlternateStateBase[ i ].faded ) )
					{
						EditorGUILayout.BeginVertical( "Box" );
						GUILayout.Space( 1 );
						
						// ----- < STATE NAME > ----- //
						if( alternateStateName[ i ].stringValue == string.Empty && Event.current.type == EventType.Repaint )
						{
							GUIStyle style = new GUIStyle( GUI.skin.textField );
							style.normal.textColor = new Color( 0.5f, 0.5f, 0.5f, 0.75f );
							EditorGUILayout.TextField( new GUIContent( "State Name", "The unique name to be used in reference to this state." ), "State Name", style );
						}
						else
						{
							EditorGUI.BeginChangeCheck();
							EditorGUILayout.PropertyField( alternateStateName[ i ], new GUIContent( "State Name", "The unique name to be used in reference to this state." ) );
							if( EditorGUI.EndChangeCheck() )
							{
								serializedObject.ApplyModifiedProperties();
								StoreAlternateStates();
								ScriptReferenceStates.target = alternateStates.Count > 0;
								ScriptReferenceErrorNoStates.target = alternateStates.Count == 0;
								AlternateStateNameError[ i ].target = DuplicateAlternateStateName( i );
							}
						}

						if( EditorGUILayout.BeginFadeGroup( AlternateStateNameError[ i ].faded ) )
						{
							EditorGUILayout.HelpBox( "State Name is already in use.", MessageType.Error );
							EditorGUILayout.Space();
						}
						if( AlternateStates.faded == 1.0f && AlternateStateBase[ i ].faded == 1.0f )
							EditorGUILayout.EndFadeGroup();
						// ----- < END STATE NAME > ----- //

						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField( defaultStateColor[ i ], new GUIContent( "Default Color", "The default color for the state to return to." ) );
						if( EditorGUI.EndChangeCheck() )
						{
							serializedObject.ApplyModifiedProperties();
							DefaultColorHelp[ i ].target = targ.ultimateStatusBar != null && targ.AlternateStateList[ i ].triggerOption != AlternateStateHandler.AlternateState.TriggerOption.Manual && targ.AlternateStateList[ i ].defaultStateColor != targ.ultimateStatusBar.UltimateStatusList[ targ.AlternateStateList[ i ].statusIndex ].statusColor;
						}

						if( EditorGUILayout.BeginFadeGroup( DefaultColorHelp[ i ].faded ) )
						{
							if( GUILayout.Button( "Copy Status Color", EditorStyles.miniButton ) )
							{
								defaultStateColor[ i ].colorValue = targ.ultimateStatusBar.UltimateStatusList[ targ.AlternateStateList[ i ].statusIndex ].statusColor;
								serializedObject.ApplyModifiedProperties();
								DefaultColorHelp[ i ].target = targ.ultimateStatusBar != null && targ.AlternateStateList[ i ].triggerOption != AlternateStateHandler.AlternateState.TriggerOption.Manual && targ.AlternateStateList[ i ].defaultStateColor != targ.ultimateStatusBar.UltimateStatusList[ targ.AlternateStateList[ i ].statusIndex ].statusColor;
							}
						}
						if( StatusBarAssigned.faded == 1.0f && AlternateStates.faded == 1.0f && AlternateStateBase[ i ].faded == 1.0f )
							EditorGUILayout.EndFadeGroup();

						if( EditorGUILayout.BeginFadeGroup( AlternateStateAdvanced[ i ].faded ) )
						{
							EditorGUILayout.Space();

							EditorGUILayout.LabelField( alternateStateName[ i ].stringValue == string.Empty ? "Advanced Options" : alternateStateName[ i ].stringValue + " Options", EditorStyles.boldLabel );

							EditorGUI.BeginChangeCheck();
							EditorGUILayout.PropertyField( triggerOption[ i ], new GUIContent( "Trigger Option", "Determines how the state will be switched." ) );
							if( EditorGUI.EndChangeCheck() )
							{
								serializedObject.ApplyModifiedProperties();
								TriggerOptionPercentage[ i ].target = targ.AlternateStateList[ i ].triggerOption == AlternateStateHandler.AlternateState.TriggerOption.Percentage;
								StoreAlternateStates();
								ScriptReferenceStates.target = alternateStates.Count > 0;
								ScriptReferenceErrorNoStates.target = alternateStates.Count == 0;
								DefaultColorHelp[ i ].target = targ.ultimateStatusBar != null && targ.AlternateStateList[ i ].triggerOption != AlternateStateHandler.AlternateState.TriggerOption.Manual && targ.AlternateStateList[ i ].defaultStateColor != targ.ultimateStatusBar.UltimateStatusList[ targ.AlternateStateList[ i ].statusIndex ].statusColor;
							}

							if( EditorGUILayout.BeginFadeGroup( TriggerOptionPercentage[ i ].faded ) )
							{
								EditorGUI.indentLevel = 1;
								EditorGUI.BeginChangeCheck();
								statusIndex[ i ].intValue = EditorGUILayout.Popup( "Status Name", statusIndex[ i ].intValue, statusOptions.ToArray() );
								if( EditorGUI.EndChangeCheck() )
								{
									serializedObject.ApplyModifiedProperties();
									DefaultColorHelp[ i ].target = targ.ultimateStatusBar != null && targ.AlternateStateList[ i ].triggerOption != AlternateStateHandler.AlternateState.TriggerOption.Manual && targ.AlternateStateList[ i ].defaultStateColor != targ.ultimateStatusBar.UltimateStatusList[ targ.AlternateStateList[ i ].statusIndex ].statusColor;
								}

								EditorGUI.BeginChangeCheck();
								EditorGUILayout.PropertyField( triggerBy[ i ], new GUIContent( "Trigger By", "Determines how to compare the status amount to the trigger value." ) );
								EditorGUI.indentLevel = 2;
								EditorGUILayout.Slider( triggerValue[ i ], 0.0f, 1.0f, new GUIContent( "Value", "The value at which the state will trigger." ) );
								if( EditorGUI.EndChangeCheck() )
									serializedObject.ApplyModifiedProperties();
								EditorGUI.indentLevel = 0;
								EditorGUILayout.Space();
							}
							if( StatusBarAssigned.faded == 1.0f && AlternateStateBase[ i ].faded == 1.0f && AlternateStateAdvanced[ i ].faded == 1.0f )
								EditorGUILayout.EndFadeGroup();

							EditorGUI.BeginChangeCheck();
							EditorGUILayout.PropertyField( alternateStateColor[ i ], new GUIContent( "State Color", "The color to be applied for the state." ) );
							if( EditorGUI.EndChangeCheck() )
							{
								serializedObject.ApplyModifiedProperties();
								AlternateStateAdvancedFlashing[ i ].target = targ.AlternateStateList[ i ].flashing;
							}

							EditorGUI.BeginChangeCheck();
							EditorGUILayout.PropertyField( stateType[ i ], new GUIContent( "State Type", "Determines what should be used to visually display the state." ) );
							if( EditorGUI.EndChangeCheck() )
							{
								serializedObject.ApplyModifiedProperties();
								StateTypeImage[ i ].target = targ.AlternateStateList[ i ].stateType == AlternateStateHandler.AlternateState.StateType.Image;
								StateTypeText[ i ].target = targ.AlternateStateList[ i ].stateType == AlternateStateHandler.AlternateState.StateType.Text;
							}

							EditorGUI.BeginChangeCheck();
							EditorGUI.indentLevel = 1;
							if( EditorGUILayout.BeginFadeGroup( StateTypeImage[ i ].faded ) )
							{
								EditorGUILayout.PropertyField( alternateStateImage[ i ], new GUIContent( "State Image", "The image to be used for the state." ) );
							}
							if( StatusBarAssigned.faded == 1.0f && AlternateStateBase[ i ].faded == 1.0f && AlternateStateAdvanced[ i ].faded == 1.0f )
								EditorGUILayout.EndFadeGroup();
							
							if( EditorGUILayout.BeginFadeGroup( StateTypeText[ i ].faded ) )
							{
								EditorGUILayout.PropertyField( alternateStateText[ i ], new GUIContent( "State Text", "The Text component to be used for the state." ) );
							}
							if( StatusBarAssigned.faded == 1.0f && AlternateStateBase[ i ].faded == 1.0f && AlternateStateAdvanced[ i ].faded == 1.0f )
								EditorGUILayout.EndFadeGroup();
							EditorGUI.indentLevel = 0;
							EditorGUILayout.Space();
							if( EditorGUI.EndChangeCheck() )
							{
								serializedObject.ApplyModifiedProperties();
								AlternateStateAdvancedFlashing[ i ].target = targ.AlternateStateList[ i ].flashing;
							}

							EditorGUI.BeginChangeCheck();
							EditorGUILayout.PropertyField( flashing[ i ], new GUIContent( "Flashing", "Determines whether of not the state should flash between two colors or not." ) );
							if( EditorGUI.EndChangeCheck() )
							{
								serializedObject.ApplyModifiedProperties();
								AlternateStateAdvancedFlashing[ i ].target = targ.AlternateStateList[ i ].flashing;
							}

							if( EditorGUILayout.BeginFadeGroup( AlternateStateAdvancedFlashing[ i ].faded ) )
							{
								EditorGUI.indentLevel = 1;
								EditorGUI.BeginChangeCheck();
								EditorGUILayout.PropertyField( flashingColor[ i ], new GUIContent( "Flashing Color", "The color of the flash." ) );
								if( EditorGUI.EndChangeCheck() )
									serializedObject.ApplyModifiedProperties();

								EditorGUI.BeginChangeCheck();
								EditorGUILayout.PropertyField( flashingSpeed[ i ], new GUIContent( "Flashing Speed", "Controls the speed of the flash." ) );
								EditorGUILayout.PropertyField( flashingDuration[ i ], new GUIContent( "Flashing Duration", "Controls how long the state should flash. Use 0 for no duration." ) );
								if( EditorGUI.EndChangeCheck() )
								{
									if( flashingSpeed[ i ].floatValue < 0 )
										flashingSpeed[ i ].floatValue = 0;
									if( flashingDuration[ i ].floatValue < 0 )
										flashingDuration[ i ].floatValue = 0;
									serializedObject.ApplyModifiedProperties();
								}
								EditorGUI.indentLevel = 0;
							}
							if( StatusBarAssigned.faded == 1.0f && AlternateStates.faded == 1.0f && AlternateStateBase[ i ].faded == 1.0f && AlternateStateAdvanced[ i ].faded == 1.0f )
								EditorGUILayout.EndFadeGroup();
							
							EditorGUILayout.Space();
						}
						if( StatusBarAssigned.faded == 1.0f && AlternateStates.faded == 1.0f && AlternateStateBase[ i ].faded == 1.0f )
							EditorGUILayout.EndFadeGroup();

						// ----- < EDIT TOOLBAR > ---- //
						EditorGUILayout.BeginHorizontal();
						if( GUILayout.Button( AlternateStateAdvanced[ i ].target == true ? "Hide Options" : "Show Options", EditorStyles.miniButtonLeft ) )
						{
							AlternateStateAdvanced[ i ].target = !AlternateStateAdvanced[ i ].target;
							EditorPrefs.SetBool( "UUI_USB_ALT_Advanced" + i.ToString(), AlternateStateAdvanced[ i ].target );
						}
						EditorGUI.BeginDisabledGroup( Application.isPlaying == true );
						if( GUILayout.Button( "Create", EditorStyles.miniButtonMid ) )
						{
							AddNewState( i + 1 );
						}
						EditorGUI.BeginDisabledGroup( targ.AlternateStateList.Count == 1 );
						if( GUILayout.Button( "Delete", EditorStyles.miniButtonRight ) )
						{
							if( EditorUtility.DisplayDialog( "Alternate State Handler", "Warning!\n\nAre you sure that you want to delete " + ( alternateStateName[ i ].stringValue != string.Empty ? "the " + alternateStateName[ i ].stringValue : "this" ) + " state?", "Yes", "No" ) )
							{
								RemoveState( i );
								continue;
							}
						}
						EditorGUI.EndDisabledGroup();
						EditorGUI.EndDisabledGroup();
						EditorGUILayout.EndHorizontal();
						// ----- < END EDIT TOOLBAR > ---- //

						GUILayout.Space( 1 );
						EditorGUILayout.EndVertical();
					}
					if( StatusBarAssigned.faded == 1.0f && AlternateStates.faded == 1.0f )
						EditorGUILayout.EndFadeGroup();
				}
			}
			if( StatusBarAssigned.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();
			/* ------------------------------------ > END ALTERNATE STATES < ------------------------------------ */
			#endregion

			EditorGUILayout.Space();

			#region SCRIPT REFERENCE
			/* -------------------------------------- > REFERENCE < -------------------------------------- */
			DisplayHeaderDropdown( "Script Reference", "UUI_ASH_ScriptReference", ScriptReference );
			if( EditorGUILayout.BeginFadeGroup( ScriptReference.faded ) )
			{
				EditorGUILayout.Space();

				if( targ.ultimateStatusBar != null && targ.ultimateStatusBar.statusBarName != string.Empty && ScriptReferenceError.target == true )
					ScriptReferenceError.target = false;
				else if( targ.ultimateStatusBar != null && targ.ultimateStatusBar.statusBarName == string.Empty && ScriptReferenceError.target == false )
					ScriptReferenceError.target = true;

				if( EditorGUILayout.BeginFadeGroup( ScriptReferenceError.faded ) )
				{
					EditorGUILayout.HelpBox( "The assigned Ultimate Status Bar has not been named.", MessageType.Error );
				}
				if( StatusBarAssigned.faded == 1.0f && ScriptReference.faded == 1.0f )
					EditorGUILayout.EndFadeGroup();

				if( targ.ultimateStatusBar != null && targ.ultimateStatusBar.statusBarName == string.Empty && ScriptReferenceExampleCode.target == true )
					ScriptReferenceExampleCode.target = false;
				else if( targ.ultimateStatusBar != null && targ.ultimateStatusBar.statusBarName != string.Empty && ScriptReferenceExampleCode.target == false )
					ScriptReferenceExampleCode.target = true;

				if( EditorGUILayout.BeginFadeGroup( ScriptReferenceExampleCode.faded ) )
				{
					EditorGUILayout.LabelField( "Ultimate Status Bar: " + ( targ.ultimateStatusBar.statusBarName == string.Empty ? "Unknown" : targ.ultimateStatusBar.statusBarName ) );

					if( EditorGUILayout.BeginFadeGroup( ScriptReferenceErrorNoStates.faded ) )
					{
						EditorGUILayout.HelpBox( "There are no manual states to change through code.", MessageType.Warning );
					}
					if( StatusBarAssigned.faded == 1.0f && ScriptReference.faded == 1.0f && ScriptReferenceExampleCode.target == true )
						EditorGUILayout.EndFadeGroup();

					if( EditorGUILayout.BeginFadeGroup( ScriptReferenceStates.faded ) )
					{
						if( selectionIndex > ( alternateStates.Count - 1 ) )
							selectionIndex = 0;

						EditorGUILayout.BeginVertical( "Box" );
						GUILayout.Space( 1 );
						EditorGUILayout.LabelField( "Example Code Generator", EditorStyles.boldLabel );
						selectionIndex = EditorGUILayout.Popup( "Alternate State", selectionIndex, alternateStates.ToArray() );

						if( selectionIndex <= ( alternateStates.Count - 1 ) )
							EditorGUILayout.TextField( "AlternateStateHandler.SwitchState( \"" + targ.ultimateStatusBar.statusBarName + "\", \"" + alternateStates[ selectionIndex ] + "\", targetState );" );
						GUILayout.Space( 1 );
						EditorGUILayout.EndVertical();
					}
					if( StatusBarAssigned.faded == 1.0f && ScriptReference.faded == 1.0f && ScriptReferenceExampleCode.target == true )
						EditorGUILayout.EndFadeGroup();
				}
				if( StatusBarAssigned.faded == 1.0f && ScriptReference.faded == 1.0f )
					EditorGUILayout.EndFadeGroup();
			}
			if( StatusBarAssigned.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();
			/* ------------------------------------ > END REFERENCE < ------------------------------------ */
			#endregion
		}
		EditorGUILayout.EndFadeGroup();

		EditorGUILayout.Space();
		Repaint();
	}

	void AddNewState ( int index )
	{
		serializedObject.FindProperty( "AlternateStateList" ).InsertArrayElementAtIndex( index );
		serializedObject.ApplyModifiedProperties();
		
		// Assign default values so that the previous index values are not copied.
		targ.AlternateStateList[ index ] = new AlternateStateHandler.AlternateState();

		AlternateStateBase.Insert( index, new AnimBool( false ) );
		AlternateStateBase[ index ].target = true;
		AlternateStateAdvanced.Insert( index, new AnimBool( false ) );

		EditorUtility.SetDirty( targ );

		// Store the references to get the information.
		StoreReferences();
	}

	void RemoveState ( int index )
	{
		serializedObject.FindProperty( "AlternateStateList" ).DeleteArrayElementAtIndex( index );
		serializedObject.ApplyModifiedProperties();

		AlternateStateBase.RemoveAt( index );
		AlternateStateAdvanced.RemoveAt( index );

		StoreReferences();
	}

	void StoreStatusOptions ()
	{
		statusOptions = new List<string>();
		if( targ.ultimateStatusBar != null )
		{
			for( int i = 0; i < targ.ultimateStatusBar.UltimateStatusList.Count; i++ )
			{
				if( targ.ultimateStatusBar.UltimateStatusList[ i ].statusName != string.Empty )
					statusOptions.Add( targ.ultimateStatusBar.UltimateStatusList[ i ].statusName );
			}
		}
	}

	void StoreAlternateStates ()
	{
		alternateStates = new List<string>();
		if( targ.AlternateStateList.Count > 0 )
		{
			for( int i = 0; i < targ.AlternateStateList.Count; i++ )
			{
				if( targ.AlternateStateList[ i ].triggerOption == AlternateStateHandler.AlternateState.TriggerOption.Manual )
					alternateStates.Add( targ.AlternateStateList[ i ].alternateStateName );
			}
		}
	}

	bool DuplicateAlternateStateName ( int index )
	{
		if( alternateStateName[ index ].stringValue == string.Empty )
			return false;

		for( int i = 0; i < alternateStateName.Count; i++ )
		{
			if( i == index )
				continue;

			if( alternateStateName[ i ].stringValue == alternateStateName[ index ].stringValue )
				return true;
		}
		return false;
	}

	void StoreReferences ()
	{
		targ = ( AlternateStateHandler )target;

		ultimateStatusBar = serializedObject.FindProperty( "ultimateStatusBar" );

		// If the status bar has no alt state information registered, then create a new state.
		if( targ.AlternateStateList.Count == 0 )
		{
			serializedObject.FindProperty( "AlternateStateList" ).arraySize++;
			serializedObject.ApplyModifiedProperties();
			targ.AlternateStateList[ 0 ] = new AlternateStateHandler.AlternateState();
			EditorUtility.SetDirty( targ );
		}

		// Reset property lists
		defaultStateColor = new List<SerializedProperty>();
		alternateStateName = new List<SerializedProperty>();
		triggerOption = new List<SerializedProperty>();
		alternateStateImage = new List<SerializedProperty>();
		alternateStateColor = new List<SerializedProperty>();
		flashing = new List<SerializedProperty>();
		flashingColor = new List<SerializedProperty>();
		flashingSpeed = new List<SerializedProperty>();
		triggerValue = new List<SerializedProperty>();
		triggerBy = new List<SerializedProperty>();
		stateType = new List<SerializedProperty>();
		alternateStateText = new List<SerializedProperty>();
		statusIndex = new List<SerializedProperty>();
		flashingDuration = new List<SerializedProperty>();

		// Reset sections
		TriggerOptionPercentage = new List<AnimBool>();
		AlternateStateAdvancedFlashing = new List<AnimBool>();

		StateTypeImage = new List<AnimBool>();
		StateTypeText = new List<AnimBool>();
		DefaultColorHelp = new List<AnimBool>();
		AlternateStateNameError = new List<AnimBool>();

		for( int i = 0; i < targ.AlternateStateList.Count; i++ )
		{
			// Add properties
			statusIndex.Add( serializedObject.FindProperty( string.Format( "AlternateStateList.Array.data[{0}].statusIndex", i ) ) );
			alternateStateName.Add( serializedObject.FindProperty( string.Format( "AlternateStateList.Array.data[{0}].alternateStateName", i ) ) );
			triggerOption.Add( serializedObject.FindProperty( string.Format( "AlternateStateList.Array.data[{0}].triggerOption", i ) ) );
			alternateStateImage.Add( serializedObject.FindProperty( string.Format( "AlternateStateList.Array.data[{0}].alternateStateImage", i ) ) );
			alternateStateColor.Add( serializedObject.FindProperty( string.Format( "AlternateStateList.Array.data[{0}].alternateStateColor", i ) ) );
			flashing.Add( serializedObject.FindProperty( string.Format( "AlternateStateList.Array.data[{0}].flashing", i ) ) );
			flashingColor.Add( serializedObject.FindProperty( string.Format( "AlternateStateList.Array.data[{0}].flashingColor", i ) ) );
			flashingSpeed.Add( serializedObject.FindProperty( string.Format( "AlternateStateList.Array.data[{0}].flashingSpeed", i ) ) );
			triggerValue.Add( serializedObject.FindProperty( string.Format( "AlternateStateList.Array.data[{0}].triggerValue", i ) ) );
			triggerBy.Add( serializedObject.FindProperty( string.Format( "AlternateStateList.Array.data[{0}].triggerBy", i ) ) );
			stateType.Add( serializedObject.FindProperty( string.Format( "AlternateStateList.Array.data[{0}].stateType", i ) ) );
			alternateStateText.Add( serializedObject.FindProperty( string.Format( "AlternateStateList.Array.data[{0}].alternateStateText", i ) ) );
			defaultStateColor.Add( serializedObject.FindProperty( string.Format( "AlternateStateList.Array.data[{0}].defaultStateColor", i ) ) );
			flashingDuration.Add( serializedObject.FindProperty( string.Format( "AlternateStateList.Array.data[{0}].flashingDuration", i ) ) );

			// Add sections
			TriggerOptionPercentage.Add( new AnimBool( targ.AlternateStateList[ i ].triggerOption == AlternateStateHandler.AlternateState.TriggerOption.Percentage ) );
			AlternateStateAdvancedFlashing.Add( new AnimBool( targ.AlternateStateList[ i ].flashing == true ) );

			StateTypeImage.Add( new AnimBool( targ.AlternateStateList[ i ].stateType == AlternateStateHandler.AlternateState.StateType.Image ) );
			StateTypeText.Add( new AnimBool( targ.AlternateStateList[ i ].stateType == AlternateStateHandler.AlternateState.StateType.Text ) );

			DefaultColorHelp.Add( new AnimBool( targ.ultimateStatusBar != null && targ.AlternateStateList[ i ].triggerOption != AlternateStateHandler.AlternateState.TriggerOption.Manual && targ.AlternateStateList[ i ].defaultStateColor != targ.ultimateStatusBar.UltimateStatusList[ targ.AlternateStateList[ i ].statusIndex ].statusColor ) );

			AlternateStateNameError.Add( new AnimBool( targ.AlternateStateList[ i ].alternateStateName != string.Empty && DuplicateAlternateStateName( i ) ) );
		}

		StoreStatusOptions();
		StoreAlternateStates();

		// Sections
		AlternateStates = new AnimBool( EditorPrefs.GetBool( "UUI_ASH_AlternateStates" ) );
		ScriptReference = new AnimBool( EditorPrefs.GetBool( "UUI_ASH_ScriptReference" ) );

		StatusBarAssigned = new AnimBool( targ.ultimateStatusBar != null );
		StatusBarUnassigned = new AnimBool( targ.ultimateStatusBar == null );
		
		// ERRORS AND WARNINGS //
		ScriptReferenceExampleCode = new AnimBool( targ.ultimateStatusBar != null && targ.ultimateStatusBar.statusBarName != string.Empty );
		ScriptReferenceError = new AnimBool( targ.ultimateStatusBar != null && targ.ultimateStatusBar.statusBarName == string.Empty );
		ScriptReferenceStates = new AnimBool( alternateStates.Count > 0 );
		ScriptReferenceErrorNoStates = new AnimBool( alternateStates.Count == 0 );
	}
}