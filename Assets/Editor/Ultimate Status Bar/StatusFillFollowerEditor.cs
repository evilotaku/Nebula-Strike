/* Written by Kaz Crowe */
/* StatusFillFollowerEditor.cs */
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.AnimatedValues;
using System.Collections.Generic;

[CustomEditor( typeof( StatusFillFollower ) )]
public class StatusFillFollowerEditor : Editor
{
	#region VARIABLES
	StatusFillFollower targ;

	// ---- < ULTIMATE STATUS BAR > ---- //
	AnimBool UltimateStatusBarAssigned, UltimateStatusBarUnassigned;
	AnimBool UltimateStatusSelection;
	List<string> statusOptions;
	SerializedProperty ultimateStatusBar;
	SerializedProperty statusIndex;

	// ---- < SCALE > ---- //
	AnimBool ImageRatioImageUnassigned, ImageRatioSpriteUnassigned;
	AnimBool ImageRatioPreserve, ImageRatioCustom;
	AnimBool UltimateStatusBarPositioningError;
	SerializedProperty scaleDirection, imageSize;
	SerializedProperty imageAspectRatio, targetImage;
	SerializedProperty xRatio, yRatio;

	// ---- < POSITIONING > ---- //
	SerializedProperty minimumPosition, maximumPosition;
	bool showMinimumPosition = false;
	bool showMaximumPosition = false;
	#endregion

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

			UltimateStatusBarPositioningError.target = targ.ultimateStatusBar != null && targ.ultimateStatusBar.positioningOption != UltimateStatusBar.PositioningOption.ScreenSpace;
			UltimateStatusBarAssigned.target = targ.ultimateStatusBar != null && UltimateStatusBarPositioningError.target == false;
			UltimateStatusBarUnassigned.target = targ.ultimateStatusBar == null && UltimateStatusBarPositioningError.target == false;

			StoreStatusOptions();

			UltimateStatusSelection.target = targ.ultimateStatusBar != null && statusOptions.Count > 1;
		}

		if( EditorGUILayout.BeginFadeGroup( UltimateStatusBarUnassigned.faded ) )
		{
			EditorGUILayout.BeginVertical( "Box" );
			EditorGUILayout.HelpBox( "Please assign the targeted Ultimate Status Bar before continuing.", MessageType.Warning );
			if( GUILayout.Button( "Find Status Bar" ) )
			{
				ultimateStatusBar.objectReferenceValue = targ.gameObject.GetComponentInParent<UltimateStatusBar>();
				serializedObject.ApplyModifiedProperties();

				UltimateStatusBarAssigned.target = targ.ultimateStatusBar != null;
				UltimateStatusBarUnassigned.target = targ.ultimateStatusBar == null;

				StoreStatusOptions();

				if( targ.ultimateStatusBar == null )
					Debug.LogWarning( "Status Fill Follower - Could not find an Ultimate Status Bar component in any parent GameObjects." );
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndFadeGroup();

		if( EditorGUILayout.BeginFadeGroup( UltimateStatusBarPositioningError.faded ) )
		{
			EditorGUILayout.HelpBox( "The Ultimate Status Bar associated with this component is not set to Screen Space for Positioning.", MessageType.Error );
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
			EditorGUILayout.PropertyField( scaleDirection, new GUIContent( "Scale Direction", "Determines which direction should be referenced from the Ultimate Status Bar to be sized by." ) );
			EditorGUILayout.Slider( imageSize, 0.0f, 1.0f, new GUIContent( "Image Size", "Determines the overall size of the image." ) );
			if( EditorGUI.EndChangeCheck() )
				serializedObject.ApplyModifiedProperties();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( imageAspectRatio, new GUIContent( "Image Aspect Ratio", "Determines if the aspect ratio should be calculated or manually set." ) );
			if( EditorGUI.EndChangeCheck() )
			{
				serializedObject.ApplyModifiedProperties();

				ImageRatioPreserve.target = targ.imageAspectRatio == StatusFillFollower.ImageAspectRatio.Preserve;
				ImageRatioCustom.target = targ.imageAspectRatio == StatusFillFollower.ImageAspectRatio.Custom;
			}

			if( EditorGUILayout.BeginFadeGroup( ImageRatioPreserve.faded ) )
			{
				EditorGUI.indentLevel = 1;
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( targetImage, new GUIContent( "Target Image", "The targeted image to preserve the aspect ratio of." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();
					ImageRatioImageUnassigned.target = targ.targetImage == null;
					ImageRatioSpriteUnassigned.target = targ.targetImage != null && targ.targetImage.sprite == null;
				}
				EditorGUI.indentLevel = 0;

				if( EditorGUILayout.BeginFadeGroup( ImageRatioImageUnassigned.faded ) )
				{
					EditorGUILayout.BeginVertical( "Box" );
					EditorGUILayout.HelpBox( "The Target Image component needs to be assigned in order to preserve the aspect of the image.", MessageType.Error );
					if( GUILayout.Button( "Find", EditorStyles.miniButton ) )
					{
						targetImage.objectReferenceValue = targ.GetComponent<Image>();
						serializedObject.ApplyModifiedProperties();

						ImageRatioImageUnassigned.target = targ.targetImage == null;
						ImageRatioSpriteUnassigned.target = targ.targetImage != null && targ.targetImage.sprite == null;
					}
					EditorGUILayout.EndVertical();
				}
				if( UltimateStatusBarAssigned.faded == 1.0f && ImageRatioPreserve.faded == 1.0f )
					EditorGUILayout.EndFadeGroup();

				if( ImageRatioSpriteUnassigned.target == true && targ.targetImage != null && targ.targetImage.sprite != null )
					ImageRatioSpriteUnassigned.target = false;

				if( EditorGUILayout.BeginFadeGroup( ImageRatioSpriteUnassigned.faded ) )
					EditorGUILayout.HelpBox( "The Target Image does not have a Source Image assigned to it.", MessageType.Error );
				if( UltimateStatusBarAssigned.faded == 1.0f && ImageRatioPreserve.faded == 1.0f )
					EditorGUILayout.EndFadeGroup();
			}
			if( UltimateStatusBarAssigned.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();

			if( EditorGUILayout.BeginFadeGroup( ImageRatioCustom.faded ) )
			{
				EditorGUI.indentLevel = 1;
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Slider( xRatio, 0.0f, 1.0f, new GUIContent( "X Ratio", "The desired width of the image." ) );
				EditorGUILayout.Slider( yRatio, 0.0f, 1.0f, new GUIContent( "Y Ratio", "The desired height of the image." ) );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();
				EditorGUI.indentLevel = 0;
			}
			if( UltimateStatusBarAssigned.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();

			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical( "Box" );
			EditorGUILayout.LabelField( "Position Anchors", EditorStyles.boldLabel );
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField( "Minimum Position: " + ( minimumPosition.vector2Value != Vector2.zero ? minimumPosition.vector2Value.ToString() : "NaN" ) );
			EditorGUI.BeginDisabledGroup( showMinimumPosition == true );
			if( GUILayout.Button( "Set", EditorStyles.miniButton ) )
			{
				if( minimumPosition.vector2Value != Vector2.zero )
				{
					if( EditorUtility.DisplayDialog( "Status Fill Follower", "Warning! You are about to overwrite a previously registered position.\n\nContinue?", "Yes", "No" ) )
					{
						minimumPosition.vector2Value = ConfigureMinimumPosition( targ.GetComponent<RectTransform>().position );
						serializedObject.ApplyModifiedProperties();
					}
				}
				else
				{
					minimumPosition.vector2Value = ConfigureMinimumPosition( targ.GetComponent<RectTransform>().position );
					serializedObject.ApplyModifiedProperties();
				}
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel = 1;
			EditorGUI.BeginChangeCheck();
			showMinimumPosition = EditorGUILayout.Toggle( "Edit In Scene", showMinimumPosition );
			if( EditorGUI.EndChangeCheck() )
				EditorUtility.SetDirty( targ );
			EditorGUI.indentLevel = 0;
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField( "Maximim Position: " + ( maximumPosition.vector2Value != Vector2.zero ? maximumPosition.vector2Value.ToString() : "NaN" ) );
			EditorGUI.BeginDisabledGroup( showMaximumPosition == true );
			if( GUILayout.Button( "Set", EditorStyles.miniButton ) )
			{
				if( maximumPosition.vector2Value != Vector2.zero )
				{
					if( EditorUtility.DisplayDialog( "Status Fill Follower", "Warning! You are about to overwrite a previously registered position.\n\nContinue?", "Yes", "No" ) )
					{
						maximumPosition.vector2Value = ConfigureMaximumPosition( targ.GetComponent<RectTransform>().position );
						serializedObject.ApplyModifiedProperties();
					}
				}
				else
				{
					maximumPosition.vector2Value = ConfigureMaximumPosition( targ.GetComponent<RectTransform>().position );
					serializedObject.ApplyModifiedProperties();
				}
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel = 1;
			EditorGUI.BeginChangeCheck();
			showMaximumPosition = EditorGUILayout.Toggle( "Edit In Scene", showMaximumPosition );
			if( EditorGUI.EndChangeCheck() )
				EditorUtility.SetDirty( targ );
			EditorGUI.indentLevel = 0;
			GUILayout.Space( 1 );
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndFadeGroup();

		EditorGUILayout.Space();

		Repaint();
	}

	void OnSceneGUI ()
	{
		if( showMinimumPosition == true )
		{
			Vector2 tempVec = targ.ultimateStatusBar.GetComponent<RectTransform>().position;
			tempVec.x -= ( minimumPosition.vector2Value.x * targ.ultimateStatusBar.GetComponent<RectTransform>().sizeDelta.x );
			tempVec.y -= ( minimumPosition.vector2Value.y * targ.ultimateStatusBar.GetComponent<RectTransform>().sizeDelta.y );
			EditorGUI.BeginChangeCheck();
			tempVec = Handles.PositionHandle( tempVec,  Quaternion.identity );
			if( EditorGUI.EndChangeCheck() )
			{
				minimumPosition.vector2Value = ConfigureMinimumPosition( tempVec );
				serializedObject.ApplyModifiedProperties();
			}
		}
		if( showMaximumPosition == true )
		{
			Vector3 tempVec = targ.ultimateStatusBar.GetComponent<RectTransform>().position;
			tempVec.x -= ( maximumPosition.vector2Value.x * targ.ultimateStatusBar.GetComponent<RectTransform>().sizeDelta.x );
			tempVec.y -= ( maximumPosition.vector2Value.y * targ.ultimateStatusBar.GetComponent<RectTransform>().sizeDelta.y );
			EditorGUI.BeginChangeCheck();
			tempVec = Handles.PositionHandle( tempVec,  Quaternion.identity );
			if( EditorGUI.EndChangeCheck() )
			{
				maximumPosition.vector2Value = ConfigureMaximumPosition( tempVec );
				serializedObject.ApplyModifiedProperties();
			}
		}
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

	void StoreReferences ()
	{
		targ = ( StatusFillFollower )target;

		if( targ == null )
			return;

		ultimateStatusBar = serializedObject.FindProperty( "ultimateStatusBar" );
		statusIndex = serializedObject.FindProperty( "statusIndex" );
		scaleDirection = serializedObject.FindProperty( "scaleDirection" );
		imageSize = serializedObject.FindProperty( "imageSize" );
		minimumPosition = serializedObject.FindProperty( "minimumPosition" );
		maximumPosition = serializedObject.FindProperty( "maximumPosition" );

		imageAspectRatio = serializedObject.FindProperty( "imageAspectRatio" );
		targetImage = serializedObject.FindProperty( "targetImage" );
		xRatio = serializedObject.FindProperty( "xRatio" );
		yRatio = serializedObject.FindProperty( "yRatio" );

		UltimateStatusBarPositioningError = new AnimBool( targ.ultimateStatusBar != null && targ.ultimateStatusBar.positioningOption != UltimateStatusBar.PositioningOption.ScreenSpace );

		UltimateStatusBarAssigned = new AnimBool( targ.ultimateStatusBar != null && UltimateStatusBarPositioningError.target == false );
		UltimateStatusBarUnassigned = new AnimBool( targ.ultimateStatusBar == null && UltimateStatusBarPositioningError.target == false );

		ImageRatioPreserve = new AnimBool( targ.imageAspectRatio == StatusFillFollower.ImageAspectRatio.Preserve );
		ImageRatioCustom = new AnimBool( targ.imageAspectRatio == StatusFillFollower.ImageAspectRatio.Custom );
		ImageRatioImageUnassigned = new AnimBool( targ.targetImage == null );
		ImageRatioSpriteUnassigned = new AnimBool( targ.targetImage != null && targ.targetImage.sprite == null );

		StoreStatusOptions();
		UltimateStatusSelection = new AnimBool( targ.ultimateStatusBar != null && statusOptions.Count > 1 && UltimateStatusBarPositioningError.target == false );
	}

	Vector3 ConfigureMinimumPosition ( Vector3 pos )
	{
		if( GetParentCanvas() == null )
			return Vector2.zero;
		
		if( targ.statusBarTransform == null )
			targ.statusBarTransform = targ.ultimateStatusBar.GetComponent<RectTransform>();
		
		Vector3 tempVector = targ.statusBarTransform.position - pos;
		tempVector.x = tempVector.x / targ.statusBarTransform.sizeDelta.x;
		tempVector.y = tempVector.y / targ.statusBarTransform.sizeDelta.y;
		return tempVector;
	}

	Vector3 ConfigureMaximumPosition ( Vector3 pos )
	{
		if( GetParentCanvas() == null )
			return Vector2.zero;

		if( targ.statusBarTransform == null )
			targ.statusBarTransform = targ.ultimateStatusBar.GetComponent<RectTransform>();
		
		Vector3 tempVector = targ.statusBarTransform.position - pos;
		tempVector.x = tempVector.x / targ.statusBarTransform.sizeDelta.x;
		tempVector.y = tempVector.y / targ.statusBarTransform.sizeDelta.y;
		return tempVector;
	}

	Canvas GetParentCanvas ()
	{
		// Store the current parent.
		Transform parent = targ.transform.parent;

		// Loop through parents as long as there is one.
		while( parent != null )
		{ 
			// If there is a Canvas component, return the component.
			if( parent.transform.GetComponent<Canvas>() )
				return parent.transform.GetComponent<Canvas>();

			// Else, shift to the next parent.
			parent = parent.transform.parent;
		}
		return null;
	}
}