/* Written by Kaz Crowe */
/* DramaticStatusFillEditor.cs */
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;

[CustomEditor( typeof( DramaticStatusFill ) )]
public class DramaticStatusFillEditor : Editor
{
	DramaticStatusFill targ;
	
	List<string> statusOptions;

	// Properties
	SerializedProperty statusIndex, ultimateStatusBar;
	SerializedProperty statusImage, statusColor;
	SerializedProperty secondsDelay, resetSensitivity;
	SerializedProperty fillSpeed, dramaticStyle;
	
	// Sections
	AnimBool UltimateStatusBarAssigned, UltimateStatusBarUnassigned;
	AnimBool FollowStatusDecrease, ResetSensitivity;
	AnimBool UltimateStatusSelection;
	AnimBool StatusImageAssigned, StatusImageUnassigned;


	void OnEnable ()
	{
		// Store the references to all variables.
		StoreReferences();

		// Register the UndoRedoCallback function to be called when an undo/redo is performed.
		Undo.undoRedoPerformed += UndoRedoCallback;
	}

	void OnDisable ()
	{
		// Remove the UndoRedoCallback from the Undo event.
		Undo.undoRedoPerformed -= UndoRedoCallback;
	}

	// Function called for Undo/Redo operations.
	void UndoRedoCallback ()
	{
		// Re-reference all variables on undo/redo.
		StoreReferences();
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
			UltimateStatusBarAssigned.target = targ.ultimateStatusBar == null ? false : true;
			UltimateStatusBarUnassigned.target = targ.ultimateStatusBar != null ? false : true;
			UltimateStatusSelection.target = targ.ultimateStatusBar.UltimateStatusList.Count > 1;
			StoreStatusOptions();
		}

		if( EditorGUILayout.BeginFadeGroup( UltimateStatusBarUnassigned.faded ) )
		{
			EditorGUILayout.BeginVertical( "Box" );
			EditorGUILayout.HelpBox( "Please assign the targeted Ultimate Status Bar before continuing.", MessageType.Warning );
			if( GUILayout.Button( "Find Status Bar" ) )
			{
				targ.ultimateStatusBar = targ.gameObject.GetComponentInParent<UltimateStatusBar>();
				EditorUtility.SetDirty( targ );

				UltimateStatusBarAssigned.target = targ.ultimateStatusBar == null ? false : true;
				UltimateStatusBarUnassigned.target = targ.ultimateStatusBar != null ? false : true;

				StoreStatusOptions();

				if( targ.ultimateStatusBar == null )
					Debug.LogWarning( "Dramatic Status Fill - Could not find an Ultimate Status Bar component in any parent GameObjects." );
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndFadeGroup();

		if( EditorGUILayout.BeginFadeGroup( UltimateStatusBarAssigned.faded ) )
		{
			if( EditorGUILayout.BeginFadeGroup( UltimateStatusSelection.faded ) )
			{
				EditorGUI.BeginChangeCheck();
				statusIndex.intValue = EditorGUILayout.Popup( "Status Name", statusIndex.intValue, statusOptions.ToArray() );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();
			}
			if( UltimateStatusBarAssigned.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( statusImage, new GUIContent( "Status Image", "The image component to be used." ) );
			if( EditorGUI.EndChangeCheck() )
			{
				serializedObject.ApplyModifiedProperties();

				if( targ.statusImage != null )
				{
					statusColor.colorValue = targ.statusImage.color;
					serializedObject.ApplyModifiedProperties();
				}

				StatusImageAssigned.target = targ.ultimateStatusBar != null && targ.statusImage != null;
				StatusImageUnassigned.target = targ.ultimateStatusBar != null && targ.statusImage == null;
			}

			if( EditorGUILayout.BeginFadeGroup( StatusImageAssigned.faded ) )
			{
				EditorGUI.indentLevel = 1;
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( statusColor, new GUIContent( "Status Color", "The color of the status image." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();
					targ.statusImage.color = statusColor.colorValue;
					EditorUtility.SetDirty( targ.statusImage );
				}
				EditorGUI.indentLevel = 0;
			}
			if( UltimateStatusBarAssigned.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();

			if( EditorGUILayout.BeginFadeGroup( StatusImageUnassigned.faded ) )
			{
				EditorGUILayout.BeginVertical( "Box" );
				EditorGUILayout.HelpBox( "The Status Image component needs to be assigned.", MessageType.Error );
				if( GUILayout.Button( "Find", EditorStyles.miniButton ) )
				{
					statusImage.objectReferenceValue = targ.GetComponent<Image>();
					serializedObject.ApplyModifiedProperties();

					StatusImageAssigned.target = targ.ultimateStatusBar != null && targ.statusImage != null;
					StatusImageUnassigned.target = targ.ultimateStatusBar != null && targ.statusImage == null;
				}
				EditorGUILayout.EndVertical();
			}
			if( UltimateStatusBarAssigned.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( dramaticStyle, new GUIContent( "Dramatic Style", "Should this component draw attention to the decrease or increase of the status?" ) );
			if( EditorGUI.EndChangeCheck() )
			{
				serializedObject.ApplyModifiedProperties();
				FollowStatusDecrease.target = targ.dramaticStyle == DramaticStatusFill.DramaticStyle.Decrease;
			}
			if( EditorGUILayout.BeginFadeGroup( FollowStatusDecrease.faded ) )
			{
				EditorGUI.BeginChangeCheck();
				EditorGUI.indentLevel = 1;
				EditorGUILayout.PropertyField( secondsDelay, new GUIContent( "Seconds Delay", "Time is seconds to delay before moving towards the target amount of fill." ) );

				if( EditorGUILayout.BeginFadeGroup( ResetSensitivity.faded ) )
					EditorGUILayout.Slider( resetSensitivity, 0.0f, 1.0f, new GUIContent( "Reset Sensitivity", "If the difference between the current and target fills are within the sensitivity amount, then the wait time will be applied again." ) );
				if( UltimateStatusBarAssigned.faded == 1.0f && FollowStatusDecrease.faded == 1.0f )
					EditorGUILayout.EndFadeGroup();

				EditorGUILayout.Slider( fillSpeed, 0.0f, 1.0f, new GUIContent( "Fill Speed", "Amount of fill per second." ) );
				EditorGUI.indentLevel = 0;
				if( EditorGUI.EndChangeCheck() )
				{
					if( secondsDelay.floatValue < 0 )
						secondsDelay.floatValue = 0;
					serializedObject.ApplyModifiedProperties();

					ResetSensitivity.target = secondsDelay.floatValue > 0;
				}
			}
			if( UltimateStatusBarAssigned.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();
		}
		EditorGUILayout.EndFadeGroup();

		EditorGUILayout.Space();

		Repaint();
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
			UltimateStatusSelection = new AnimBool( targ.ultimateStatusBar != null && targ.ultimateStatusBar.UltimateStatusList.Count > 1 );
		}
	}

	void StoreReferences ()
	{
		targ = ( DramaticStatusFill )target;

		statusIndex = serializedObject.FindProperty( "statusIndex" );
		ultimateStatusBar = serializedObject.FindProperty( "ultimateStatusBar" );
		statusImage = serializedObject.FindProperty( "statusImage" );
		statusColor = serializedObject.FindProperty( "statusColor" );
		secondsDelay = serializedObject.FindProperty( "secondsDelay" );
		resetSensitivity = serializedObject.FindProperty( "resetSensitivity" );
		fillSpeed = serializedObject.FindProperty( "fillSpeed" );
		dramaticStyle = serializedObject.FindProperty( "dramaticStyle" );

		StoreStatusOptions();

		// SECTIONS //
		FollowStatusDecrease = new AnimBool( targ.dramaticStyle == DramaticStatusFill.DramaticStyle.Decrease );
		ResetSensitivity = new AnimBool( targ.secondsDelay > 0 );
		
		// ERRROS AND WARNINGS //
		UltimateStatusBarAssigned = new AnimBool( targ.ultimateStatusBar != null );
		UltimateStatusBarUnassigned = new AnimBool( targ.ultimateStatusBar == null );
		UltimateStatusSelection = new AnimBool( targ.ultimateStatusBar != null && targ.ultimateStatusBar.UltimateStatusList.Count > 1 );
		StatusImageAssigned = new AnimBool( targ.ultimateStatusBar != null && targ.statusImage != null );
		StatusImageUnassigned = new AnimBool( targ.ultimateStatusBar != null && targ.statusImage == null );
	}
}