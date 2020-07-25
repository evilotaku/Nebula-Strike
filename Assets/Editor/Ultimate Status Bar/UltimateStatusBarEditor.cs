/* Written by Kaz Crowe */
/* UltimateStatusBarEditor.cs */
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor( typeof( UltimateStatusBar ) )]
public class UltimateStatusBarEditor : Editor
{
	#region VARIABLES
	UltimateStatusBar targ;

	// ---- < STATUS BAR POSITIONING > ---- //
	AnimBool StatusBarPositioning;
	Canvas parentCanvas;
	// ----->>> SCREEN SPACE //
	AnimBool PositioningScreenSpace, PositioningFollowCam;
	AnimBool PositioningScreenSpaceError;
	AnimBool ImageRatioPreserve, ImageRatioCustom;
	AnimBool ImageRatioImageUnassigned, ImageRatioSpriteUnassigned;
	SerializedProperty positioningOption, scalingAxis;
	SerializedProperty statusBarSize, imageAspectRatio, targetImage;
	SerializedProperty xRatio, yRatio;
	SerializedProperty xPosition, yPosition;
	// ----->>> FOLLOW CAM ROTATION //
	AnimBool FindByTransform, FindByName, FindByTag;
	AnimBool FollowCamUnassigned, WarningCanvasScale;
	SerializedProperty findBy, targetName, cameraTransform;

	// ---- < STATUS BAR OPTIONS > ---- //
	AnimBool StatusBarOptions;
	// ----->>> ICON //
	AnimBool StatusBarIconSection;
	SerializedProperty statusBarIcon;
	Color statusBarIconColor = Color.white;
	// ----->>> TEXT //
	AnimBool StatusBarTextSection;
	SerializedProperty statusBarText;
	Color statusBarTextColor = Color.white;
	string tempStatusString = "";
	// ----->>> VISIBILITY //
	AnimBool UpdateVisibilty, UpdateVisibilityOnStatusUpdated;
	AnimBool UpdateVisibiltyAnimation, UpdateVisibilityFade;
	SerializedProperty updateVisibility, updateUsing;
	SerializedProperty idleSeconds;
	SerializedProperty enableDuration, disableDuration;
	SerializedProperty enabledAlpha, disabledAlpha;
	SerializedProperty statusBarAnimator;
	SerializedProperty initialState;

	// ---- < STATUS INFORMATION > ---- //
	AnimBool StatusInformation;
	List<AnimBool> UltimateStatusBase, UltimateStatusAdvanced;
	// ----->>> NAME //
	List<AnimBool> UltimateStatusNameWarning, UltimateStatusNameError;
	List<SerializedProperty> statusName;
	// ----->>> IMAGE //
	List<AnimBool> UltimateStatusBarImageWarning;
	List<SerializedProperty> statusImage;
	// ----->>> COLOR //
	List<AnimBool> UltimateStatusColorWarning;
	List<AnimBool> StatusColor, StatusGradient;
	List<SerializedProperty> colorMode, statusColor, statusGradient;
	// ----->>> TEST VALUE //
	List<float> testValue;
	// ----->>> TEXT //
	List<AnimBool> UltimateStatusAdvancedText;
	List<Color> statusTextColor;
	List<SerializedProperty> statusText;
	List<SerializedProperty> displayText, additionalText;
	// ----->>> SMOOTH FILL //
	List<AnimBool> UltimateStatusAdvancedSmoothFill;
	List<SerializedProperty> smoothFill, smoothFillDuration;
	// ----->>> FILL CONSTRAINT //
	List<AnimBool> UltimateStatusAdvancedFillConstraint;
	List<SerializedProperty> fillConstraint, fillConstraintMin, fillConstraintMax;
	// ----->>> KEEP VISIBLE //
	List<AnimBool> UltimateStatusAdvancedOnStatusUpdated, UltimateStatusAdvancedKeepVisible;
	List<SerializedProperty> keepVisible, triggerValue;

	// ---- < SCRIPT REFERENCE > ---- //
	AnimBool ScriptReference;
	AnimBool UltimateStatusBarNameUnassigned, UltimateStatusBarNameAssigned;
	AnimBool UltimateStatusBarNameDuplicate, UltimateStatusBarStatusNames;
	SerializedProperty statusBarName;
	List<string> statusNameList;
	int statusNameListIndex;
	// ----->>> EXAMPLE CODE //
	class ExampleCode
	{
		public string optionName = "";
		public string basicCode = "";
	}
	ExampleCode updateStatusGlobal = new ExampleCode() { optionName = "Update Status", basicCode = "UltimateStatusBar.UpdateStatus( \"{0}\", currentValue, maxValue );" };
	ExampleCode updateIconGlobal = new ExampleCode() { optionName = "Update Icon", basicCode = "UltimateStatusBar.UpdateStatusBarIcon( \"{0}\", newIcon );" };
	ExampleCode updateTextGlobal = new ExampleCode() { optionName = "Update Text", basicCode = "UltimateStatusBar.UpdateStatusBarText( \"{0}\", newText );" };
	ExampleCode getStatusBarGlobal = new ExampleCode() { optionName = "Get Status Bar", basicCode = "UltimateStatusBar.GetUltimateStatusBar( \"{0}\" );" };
	List<ExampleCode> exampleCodeList = new List<ExampleCode>();
	List<string> exampleCodeOptions = new List<string>();
	int exampleCodeIndex = 0;
	#endregion

	
	void OnEnable ()
	{
		// Store the references to all variables.
		StoreReferences();
		
		// Store these in OnEnable to allow for a Fade when creating a new status.
		UltimateStatusBase = new List<AnimBool>();
		UltimateStatusAdvanced = new List<AnimBool>();

		for( int i = 0; i < targ.UltimateStatusList.Count; i++ )
		{
			UltimateStatusBase.Add( new AnimBool( true ) );
			UltimateStatusAdvanced.Add( new AnimBool( EditorPrefs.GetBool( "UUI_USB_Advanced" + i.ToString() ) ) );
		}

		// Register the UndoRedoCallback function to be called when an undo/redo is performed.
		Undo.undoRedoPerformed += UndoRedoCallback;
		
		exampleCodeList.Add( updateStatusGlobal );
		exampleCodeOptions.Add( updateStatusGlobal.optionName );
		exampleCodeList.Add( updateIconGlobal );
		exampleCodeOptions.Add( updateIconGlobal.optionName );
		exampleCodeList.Add( updateTextGlobal );
		exampleCodeOptions.Add( updateTextGlobal.optionName );
		exampleCodeList.Add( getStatusBarGlobal );
		exampleCodeOptions.Add( getStatusBarGlobal.optionName );
	}

	void OnDisable ()
	{
		// Remove the UndoRedoCallback from the Undo event.
		Undo.undoRedoPerformed -= UndoRedoCallback;
	}

	// Function called for Undo/Redo operations.
	void UndoRedoCallback ()
	{
		if( targ != null )
		{
			for( int i = 0; i < targ.UltimateStatusList.Count; i++ )
				UpdateStatusColor( i );
		}

		// Re-reference all variables on undo/redo.
		StoreReferences();
	}

	// Function called to display an interactive header.
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
	
	/*
	For more information on the OnInspectorGUI and adding your own variables
	in the UltimateStatusBar.cs script and displaying them in this script,
	see the EditorGUILayout section in the Unity Documentation to help out.
	*/
	public override void OnInspectorGUI ()
	{
		serializedObject.Update();

		EditorGUILayout.Space();

		#region STATUS BAR POSITIONING
		DisplayHeaderDropdown( "Status Bar Positioning", "UUI_SizeAndPlacement", StatusBarPositioning );
		if( EditorGUILayout.BeginFadeGroup( StatusBarPositioning.faded ) )
		{
			EditorGUILayout.Space();

			// POSITIONING OPTION //
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( positioningOption, new GUIContent( "Positioning", "Determines how the Ultimate Status Bar should position itself." ) );
			if( EditorGUI.EndChangeCheck() )
			{
				serializedObject.ApplyModifiedProperties();

				PositioningScreenSpace.target = !GetPositioningScreenSpaceError() && targ.positioningOption == UltimateStatusBar.PositioningOption.ScreenSpace;
				PositioningFollowCam.target = targ.positioningOption == UltimateStatusBar.PositioningOption.WorldSpace;
				PositioningScreenSpaceError.target = GetPositioningScreenSpaceError();
				WarningCanvasScale.target = GetPositioningScreenSpaceCanvasScale();
			}

			// POSITIONING ERROR //
			if( EditorGUILayout.BeginFadeGroup( PositioningScreenSpaceError.faded ) )
			{
				EditorGUILayout.BeginVertical( "Box" );
				EditorGUILayout.HelpBox( "The parent Canvas needs to be set to 'Screen Space - Overlay' in order for the Ultimate Status Bar to function correctly.", MessageType.Error );
				if( GUILayout.Button( "Update Canvas", EditorStyles.miniButton ) )
				{
					parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
					PositioningScreenSpace.target = !GetPositioningScreenSpaceError() && targ.positioningOption == UltimateStatusBar.PositioningOption.ScreenSpace;
					PositioningScreenSpaceError.target = GetPositioningScreenSpaceError();
					EditorUtility.SetDirty( parentCanvas );
				}
				if( GUILayout.Button( "Update Status Bar", EditorStyles.miniButton ) )
				{
					UltimateStatusBarCreator.RequestCanvas( Selection.activeGameObject );
					parentCanvas = GetParentCanvas();
					PositioningScreenSpace.target = !GetPositioningScreenSpaceError() && targ.positioningOption == UltimateStatusBar.PositioningOption.ScreenSpace ? true : false;
					PositioningScreenSpaceError.target = GetPositioningScreenSpaceError();
					EditorUtility.SetDirty( parentCanvas );
				}
				EditorGUILayout.EndVertical();
			}
			if( StatusBarPositioning.faded == 1 )
				EditorGUILayout.EndFadeGroup();

			// POSITIONING SCREEN SPACE //
			if( EditorGUILayout.BeginFadeGroup( PositioningScreenSpace.faded ) )
			{
				// AXIS AND SIZE //
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( scalingAxis, new GUIContent( "Scaling Axis", "Determines whether the Ultimate Status Bar is sized according to Screen Height or Screen Width." ) );
				EditorGUILayout.Slider( statusBarSize, 0.0f, 10.0f, new GUIContent( "Status Bar Size", "Determines the overall size of the status bar." ) );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();

				// IMAGE ASPECT RATIO //
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( imageAspectRatio, new GUIContent( "Image Aspect Ratio", "Determines if the aspect ratio should be calculated or manually set." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();
					ImageRatioCustom.target = targ.screenSpaceOptions.imageAspectRatio == UltimateStatusBar.ScreenSpaceOptions.ImageAspectRatio.Custom;
					ImageRatioPreserve.target = targ.screenSpaceOptions.imageAspectRatio == UltimateStatusBar.ScreenSpaceOptions.ImageAspectRatio.Preserve;
				}

				// PRESERVE ASPECT RATIO //
				if( EditorGUILayout.BeginFadeGroup( ImageRatioPreserve.faded ) )
				{
					EditorGUI.indentLevel = 1;
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( targetImage, new GUIContent( "Target Image", "The targeted image to preserve the aspect ratio of." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						serializedObject.ApplyModifiedProperties();
						ImageRatioImageUnassigned.target = targ.screenSpaceOptions.targetImage == null;
						ImageRatioSpriteUnassigned.target = targ.screenSpaceOptions.targetImage != null && targ.screenSpaceOptions.targetImage.sprite == null;
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

							ImageRatioImageUnassigned.target = targ.screenSpaceOptions.targetImage == null;
							ImageRatioSpriteUnassigned.target = targ.screenSpaceOptions.targetImage != null && targ.screenSpaceOptions.targetImage.sprite == null;
						}
						EditorGUILayout.EndVertical();
					}
					if( StatusBarPositioning.faded == 1 && PositioningScreenSpace.faded == 1 && ImageRatioPreserve.faded == 1.0f )
						EditorGUILayout.EndFadeGroup();

					if( EditorGUILayout.BeginFadeGroup( ImageRatioSpriteUnassigned.faded ) )
					{
						if( targ.screenSpaceOptions.targetImage != null && targ.screenSpaceOptions.targetImage.sprite != null && ImageRatioSpriteUnassigned.target == true )
							ImageRatioSpriteUnassigned.target = false;

						EditorGUILayout.HelpBox( "The Target Image does not have a Source Image assigned to it.", MessageType.Error );
					}
					if( StatusBarPositioning.faded == 1 && PositioningScreenSpace.faded == 1 && ImageRatioPreserve.faded == 1.0f )
						EditorGUILayout.EndFadeGroup();
				}
				if( StatusBarPositioning.faded == 1 && PositioningScreenSpace.faded == 1 )
					EditorGUILayout.EndFadeGroup();

				// CUSTOM IMAGE RATIO //
				EditorGUI.BeginChangeCheck();
				if( EditorGUILayout.BeginFadeGroup( ImageRatioCustom.faded ) )
				{
					EditorGUI.indentLevel = 1;
					EditorGUILayout.Slider( xRatio, 0.0f, 1.0f, new GUIContent( "X Ratio", "The desired width of the image." ) );
					EditorGUILayout.Slider( yRatio, 0.0f, 1.0f, new GUIContent( "Y Ratio", "The desired height of the image." ) );
					EditorGUI.indentLevel = 0;
				}
				if( StatusBarPositioning.faded == 1 && PositioningScreenSpace.faded == 1 )
					EditorGUILayout.EndFadeGroup();
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();


				EditorGUILayout.Space();

				// STATUS BAR POSTION //
				EditorGUILayout.BeginVertical( "Box" );
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField( "Status Bar Position", EditorStyles.boldLabel );
				GUILayout.EndHorizontal();
				EditorGUI.indentLevel = 1;
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Slider( xPosition, 0.0f, 100.0f, new GUIContent( "X Position", "The horizontal position of the image." ) );
				EditorGUILayout.Slider( yPosition, 0.0f, 100.0f, new GUIContent( "Y Position", "The vertical position of the image." ) );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();
				
				GUILayout.Space( 1 );

				EditorGUI.indentLevel = 0;
				EditorGUILayout.EndVertical();
			}
			if( StatusBarPositioning.faded == 1 )
				EditorGUILayout.EndFadeGroup();

			// POSITIONING FOLLOW CAMERA //
			if( EditorGUILayout.BeginFadeGroup( PositioningFollowCam.faded ) )
			{
				// FIND BY //
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( findBy, new GUIContent( "Find Camera By", "The method in which to find the camera to face towards." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();
					FindByTransform.target = targ.worldSpaceOptions.findBy == UltimateStatusBar.WorldSpaceOptions.FindBy.Transform;
					FollowCamUnassigned.value = targ.worldSpaceOptions.cameraTransform == null;
					FindByName.target = targ.worldSpaceOptions.findBy == UltimateStatusBar.WorldSpaceOptions.FindBy.Name;
					FindByTag.target = targ.worldSpaceOptions.findBy == UltimateStatusBar.WorldSpaceOptions.FindBy.Tag;
				}

				EditorGUI.indentLevel = 1;

				// FIND BY CAMERA //
				if( EditorGUILayout.BeginFadeGroup( FindByTransform.faded ) )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( cameraTransform, new GUIContent( "Transform", "The Transform component of the desired camera." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						serializedObject.ApplyModifiedProperties();
						FollowCamUnassigned.target = targ.worldSpaceOptions.cameraTransform == null;
					}
					if( EditorGUILayout.BeginFadeGroup( FollowCamUnassigned.faded ) )
					{
						EditorGUI.indentLevel = 0;
						EditorGUILayout.HelpBox( "The Camera Transform component must be assigned in order for the Ultimate Status Bar to face the camera correctly.", MessageType.Error );
					}
					if( StatusBarPositioning.faded == 1.0f && PositioningFollowCam.faded == 1.0f && FindByTransform.faded == 1.0f )
						EditorGUILayout.EndFadeGroup();
				}
				if( StatusBarPositioning.faded == 1.0f && PositioningFollowCam.faded == 1.0f )
					EditorGUILayout.EndFadeGroup();

				// FIND BY NAME //
				if( EditorGUILayout.BeginFadeGroup( FindByName.faded ) )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( targetName, new GUIContent( "Target Name", "The name of the desired camera to find." ) );
					if( EditorGUI.EndChangeCheck() )
						serializedObject.ApplyModifiedProperties();
				}
				if( StatusBarPositioning.faded == 1.0f && PositioningFollowCam.faded == 1.0f )
					EditorGUILayout.EndFadeGroup();

				// FIND BY TAG //
				if( EditorGUILayout.BeginFadeGroup( FindByTag.faded ) )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( targetName, new GUIContent( "Target Tag", "The tag of the desired camera to find." ) );
					if( EditorGUI.EndChangeCheck() )
						serializedObject.ApplyModifiedProperties();
				}
				if( StatusBarPositioning.faded == 1.0f && PositioningFollowCam.faded == 1.0f )
					EditorGUILayout.EndFadeGroup();

				EditorGUI.indentLevel = 0;

				// WARNING: CANVAS SCALE //
				if( EditorGUILayout.BeginFadeGroup( WarningCanvasScale.faded ) )
				{
					EditorGUILayout.BeginVertical( "Box" );
					EditorGUILayout.HelpBox( "Canvas needs to be set to World Space in order to follow the rotation of a camera.", MessageType.Warning );

					// MODIFY CURRENT CANVAS //
					if( GUILayout.Button( "Modify Current Canvas", EditorStyles.miniButton ) )
					{
						if( EditorUtility.DisplayDialog( "Ultimate Status Bar", "Warning!\n\nAre you sure you want to modify the current Canvas? This may affect other UI elements that are children of it.", "Yes", "No" ) )
						{
							parentCanvas.renderMode = RenderMode.WorldSpace;
							parentCanvas.GetComponent<RectTransform>().localScale = new Vector3( 0.035f, 0.035f, 1 );
							parentCanvas.GetComponent<RectTransform>().position = Vector3.zero;
							EditorUtility.SetDirty( parentCanvas );
							WarningCanvasScale.target = GetPositioningScreenSpaceCanvasScale();
						}
					}

					// CREATE NEW CANVAS //
					if( GUILayout.Button( "Create New Canvas", EditorStyles.miniButton ) )
					{
						UltimateStatusBarCreator.RequestNewWorldSpaceCanvas( targ.gameObject );
						Vector2 oldDimentions = parentCanvas.GetComponent<RectTransform>().sizeDelta;
						parentCanvas = GetParentCanvas();
						parentCanvas.renderMode = RenderMode.WorldSpace;
						parentCanvas.GetComponent<RectTransform>().sizeDelta = oldDimentions;
						parentCanvas.GetComponent<RectTransform>().localScale = new Vector3( 0.035f, 0.035f, 1 );
						parentCanvas.GetComponent<RectTransform>().position = Vector3.zero;
						WarningCanvasScale.target = GetPositioningScreenSpaceCanvasScale();
					}
					EditorGUILayout.EndVertical();
				}
				if( StatusBarPositioning.faded == 1.0f && PositioningFollowCam.faded == 1.0f )
					EditorGUILayout.EndFadeGroup();
			}
			if( StatusBarPositioning.faded == 1 )
				EditorGUILayout.EndFadeGroup();
		}
		EditorGUILayout.EndFadeGroup();
		#endregion

		EditorGUILayout.Space();

		#region STATUS BAR OPTIONS
		/* ----------------------------------------- > STATUS BAR OPTIONS < ----------------------------------------- */
		DisplayHeaderDropdown( "Status Bar Options", "UUI_StyleAndOptions", StatusBarOptions );
		if( EditorGUILayout.BeginFadeGroup( StatusBarOptions.faded ) )
		{
			EditorGUILayout.Space();
			
			// ----- < STATUS BAR ICON > ----- //
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( statusBarIcon, new GUIContent( "Status Bar Icon", "The icon image associated with this status bar." ) );
			if( EditorGUI.EndChangeCheck() )
			{
				serializedObject.ApplyModifiedProperties();
				StatusBarIconSection.target = targ.statusBarIcon != null;
			}
			if( EditorGUILayout.BeginFadeGroup( StatusBarIconSection.faded ) )
			{
				EditorGUI.BeginChangeCheck();
				EditorGUI.indentLevel = 1;
				statusBarIconColor = EditorGUILayout.ColorField( "Icon Color", statusBarIconColor );
				EditorGUI.indentLevel = 0;
				if( EditorGUI.EndChangeCheck() )
				{
					if( targ.statusBarIcon != null )
					{
						targ.statusBarIcon.enabled = false;
						targ.statusBarIcon.color = statusBarIconColor;
						targ.statusBarIcon.enabled = true;
					}
				}
				EditorGUILayout.Space();
			}
			if( StatusBarOptions.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();
			// ----- < END STATUS BAR ICON > ----- //

			// ----- < STATUS BAR TEXT > ----- //
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( statusBarText, new GUIContent( "Status Bar Text", "The text component associated with this status bar." ) );
			if( EditorGUI.EndChangeCheck() )
			{
				serializedObject.ApplyModifiedProperties();
				StatusBarTextSection.target = targ.statusBarText != null;
				tempStatusString = targ.statusBarText != null && targ.statusBarText.text != "New Text" ? targ.statusBarText.text : "";
			}
			if( EditorGUILayout.BeginFadeGroup( StatusBarTextSection.faded ) )
			{
				EditorGUI.indentLevel = 1;
				EditorGUI.BeginChangeCheck();
				statusBarTextColor = EditorGUILayout.ColorField( "Text Color", statusBarTextColor );
				tempStatusString = EditorGUILayout.TextField( tempStatusString );
				if( EditorGUI.EndChangeCheck() )
				{
					if( targ.statusBarText != null )
					{
						targ.statusBarText.enabled = false;
						targ.statusBarText.color = statusBarTextColor;
						if( tempStatusString != string.Empty )
							targ.statusBarText.text = tempStatusString;
						targ.statusBarText.enabled = true;
					}
				}
				EditorGUI.indentLevel = 0;
				EditorGUILayout.Space();
			}
			if( StatusBarOptions.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();
			// ----- < END STATUS BAR TEXT > ----- //

			// ----- < STATUS BAR VISIBILITY > ----- //
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( updateVisibility, new GUIContent( "Update Visibility", "Determines whether or not the visibility of the Ultimate Status Bar should ever be updated." ) );
			if( EditorGUI.EndChangeCheck() )
			{
				serializedObject.ApplyModifiedProperties();
				for( int i = 0; i < targ.UltimateStatusList.Count; i++ )
					UltimateStatusAdvancedOnStatusUpdated[ i ].target = targ.updateVisibility == UltimateStatusBar.UpdateVisibility.OnStatusUpdated;

				UpdateVisibilty.target = targ.updateVisibility != UltimateStatusBar.UpdateVisibility.Never;
				UpdateVisibilityOnStatusUpdated.target = targ.updateVisibility == UltimateStatusBar.UpdateVisibility.OnStatusUpdated;
			}

			EditorGUI.indentLevel = 1;

			if( EditorGUILayout.BeginFadeGroup( UpdateVisibilty.faded ) )
			{
				if( EditorGUILayout.BeginFadeGroup( UpdateVisibilityOnStatusUpdated.faded ) )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( idleSeconds, new GUIContent( "Idle Seconds", "Time in seconds before the visibility should be updated." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						if( idleSeconds.floatValue < 0 )
							idleSeconds.floatValue = 0;

						serializedObject.ApplyModifiedProperties();
					}
				}
				if( StatusBarOptions.faded == 1.0f && UpdateVisibilty.faded == 1.0f )
					EditorGUILayout.EndFadeGroup();

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( initialState, new GUIContent( "Initial State", "The initial state that the Ultimate Status Bar visibility should be." ) );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();

				EditorGUI.indentLevel = 0;

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( updateUsing, new GUIContent( "Update Using", "Determines how the visibility of the Ultimate Status Bar should be updated." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();
					UpdateVisibiltyAnimation.target = targ.updateUsing == UltimateStatusBar.UpdateUsing.Animation;
					UpdateVisibilityFade.target = targ.updateUsing == UltimateStatusBar.UpdateUsing.Fade;
				}

				EditorGUI.indentLevel = 1;

				if( EditorGUILayout.BeginFadeGroup( UpdateVisibilityFade.faded ) )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( enableDuration, new GUIContent( "Fade In Duration", "Time in seconds for the visibility to fade in." ) );
					EditorGUILayout.PropertyField( disableDuration, new GUIContent( "Fade Out Duration", "Time in seconds for the visibility to fade out." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						if( enableDuration.floatValue < 0 )
							enableDuration.floatValue = 0;
						if( disableDuration.floatValue < 0 )
							disableDuration.floatValue = 0;

						serializedObject.ApplyModifiedProperties();
					}

					EditorGUI.BeginChangeCheck();
					EditorGUILayout.Slider( enabledAlpha, targ.disabledAlpha, 1.0f, new GUIContent( "Enabled Alpha", "The desired alpha value for the enabled state." ) );
					EditorGUILayout.Slider( disabledAlpha, 0.0f, targ.enabledAlpha, new GUIContent( "Disabled Alpha", "The desired alpha value for the disabled state." ) );
					if( EditorGUI.EndChangeCheck() )
						serializedObject.ApplyModifiedProperties();
				}
				if( StatusBarOptions.faded == 1.0f && UpdateVisibilty.faded == 1.0f )
					EditorGUILayout.EndFadeGroup();

				if( EditorGUILayout.BeginFadeGroup( UpdateVisibiltyAnimation.faded ) )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( statusBarAnimator, new GUIContent( "Animator", "The animator component to be used." ) );
					if( EditorGUI.EndChangeCheck() )
						serializedObject.ApplyModifiedProperties();
				}
				if( StatusBarOptions.faded == 1.0f && UpdateVisibilty.faded == 1.0f )
					EditorGUILayout.EndFadeGroup();
			}
			if( StatusBarOptions.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();

			EditorGUI.indentLevel = 0;
			// ----- < END STATUS BAR VISIBILITY > ----- //
		}
		EditorGUILayout.EndFadeGroup();
		/* --------------------------------------- > END STATUS BAR OPTIONS < --------------------------------------- */
		#endregion

		EditorGUILayout.Space();

		#region STATUS INFORMATION
		/* -------------------------------------- > STATUS INFORMATION < -------------------------------------- */
		DisplayHeaderDropdown( "Status Information", "UUI_USB_StatusInformation", StatusInformation );
		if( EditorGUILayout.BeginFadeGroup( StatusInformation.faded ) )
		{
			if( targ.UltimateStatusList.Count > 0 )
			{
				for( int i = 0; i < targ.UltimateStatusList.Count; i++ )
				{
					if( EditorGUILayout.BeginFadeGroup( UltimateStatusBase[ i ].faded ) )
					{
						EditorGUILayout.BeginVertical( "Box" );
						GUILayout.Space( 1 );

						// ----- < STATUS NAME > ----- //
						if( statusName[ i ].stringValue == string.Empty && Event.current.type == EventType.Repaint )
						{
							GUIStyle style = new GUIStyle( GUI.skin.textField );
							style.normal.textColor = new Color( 0.5f, 0.5f, 0.5f, 0.75f );
							EditorGUILayout.TextField( new GUIContent( "Status Name", "The unique name to be used in reference to this status." ), "Status Name", style );
						}
						else
						{
							EditorGUI.BeginChangeCheck();
							EditorGUILayout.PropertyField( statusName[ i ], new GUIContent( "Status Name", "The unique name to be used in reference to this status." ) );
							if( EditorGUI.EndChangeCheck() )
							{
								serializedObject.ApplyModifiedProperties();
								UltimateStatusNameWarning[ i ].target = statusName[ i ].stringValue == string.Empty && targ.UltimateStatusList.Count > 1;
								UltimateStatusNameError[ i ].target = DuplicateStatusName( i );
								StoreNameList();
								UltimateStatusBarStatusNames.target = statusNameList.Count > 0 && exampleCodeIndex == 0;
							}
						}
						// ----- < END STATUS NAME > ----- //

						// ----- < NAME ERRORS > ----- //
						if( EditorGUILayout.BeginFadeGroup( UltimateStatusNameWarning[ i ].faded ) )
						{
							EditorGUILayout.HelpBox( "Status Name is unassigned.", MessageType.Warning );
							EditorGUILayout.Space();
						}
						if( StatusInformation.faded == 1.0f && UltimateStatusBase[ i ].faded == 1.0f )
							EditorGUILayout.EndFadeGroup();

						if( EditorGUILayout.BeginFadeGroup( UltimateStatusNameError[ i ].faded ) )
						{
							EditorGUILayout.HelpBox( "Status Name is already in use.", MessageType.Error );
							EditorGUILayout.Space();
						}
						if( StatusInformation.faded == 1.0f && UltimateStatusBase[ i ].faded == 1.0f )
							EditorGUILayout.EndFadeGroup();
						// ----- < END NAME ERRORS > ----- //

						// ----- < STATUS IMAGE > ----- //
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField( statusImage[ i ], new GUIContent( "Status Image", "The image component to be used for this status." ) );
						if( EditorGUI.EndChangeCheck() )
						{
							serializedObject.ApplyModifiedProperties();
							if( targ.UltimateStatusList[ i ].statusImage != null && targ.UltimateStatusList[ i ].statusImage.type != Image.Type.Filled )
							{
								targ.UltimateStatusList[ i ].statusImage.type = Image.Type.Filled;
								targ.UltimateStatusList[ i ].statusImage.fillMethod = Image.FillMethod.Horizontal;
								EditorUtility.SetDirty( targ.UltimateStatusList[ i ].statusImage );
							}
							if( targ.UltimateStatusList[ i ].statusImage != null )
							{
								statusColor[ i ].colorValue = targ.UltimateStatusList[ i ].statusImage.color;
								serializedObject.ApplyModifiedProperties();
							}
							targ.UltimateStatusList[ i ].UpdateStatus( testValue[ i ], 100.0f );

							UltimateStatusBarImageWarning[ i ].target = GetStatusBarImageWarning( i );
						}
						// ----- < END STATUS IMAGE > ----- //
						
						// ----- < STATUS IMAGE ERROR > ----- //
						if( EditorGUILayout.BeginFadeGroup( UltimateStatusBarImageWarning[ i ].faded ) )
						{
							EditorGUILayout.BeginVertical( "Box" );
							EditorGUILayout.HelpBox( "Invalid Image Type: " + targ.UltimateStatusList[ i ].statusImage.type.ToString(), MessageType.Warning );
							if( GUILayout.Button( "Fix", EditorStyles.miniButton ) )
							{
								targ.UltimateStatusList[ i ].statusImage.type = Image.Type.Filled;
								EditorUtility.SetDirty( targ.UltimateStatusList[ i ].statusImage );

								UltimateStatusBarImageWarning[ i ].target = GetStatusBarImageWarning( i );
							}
							EditorGUILayout.EndVertical();
						}
						if( StatusInformation.faded == 1.0f && UltimateStatusBase[ i ].faded == 1.0f )
							EditorGUILayout.EndFadeGroup();
						// ----- < END STATUS IMAGE ERROR > ----- //

						// ----- < STATUS COLORS > ----- //
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField( colorMode[ i ], new GUIContent( "Color Mode", "The mode in which to display the color of the status to the image component." ) );
						if( EditorGUI.EndChangeCheck() )
						{
							serializedObject.ApplyModifiedProperties();
							UpdateStatusColor( i );
							UltimateStatusColorWarning[ i ].target = GetStatusColorWarning( i );
						}

						EditorGUI.BeginChangeCheck();
						EditorGUI.indentLevel = 1;
						if( targ.UltimateStatusList[ i ].colorMode == UltimateStatusBar.UltimateStatus.ColorMode.Single )
							EditorGUILayout.PropertyField( statusColor[ i ], new GUIContent( "Status Color", "The color of this status image." ) );
						else
							EditorGUILayout.PropertyField( statusGradient[ i ], new GUIContent( "Status Gradient", "The color gradient of this status image." ) );
						EditorGUI.indentLevel = 0;
						if( EditorGUI.EndChangeCheck() )
						{
							serializedObject.ApplyModifiedProperties();
							UpdateStatusColor( i );
							UltimateStatusColorWarning[ i ].target = GetStatusColorWarning( i );
						}

						if( EditorGUILayout.BeginFadeGroup( UltimateStatusColorWarning[ i ].faded ) )
						{
							EditorGUILayout.BeginVertical( "Box" );
							EditorGUILayout.HelpBox( "Image color has been modified incorrectly.", MessageType.Warning );
							EditorGUILayout.BeginHorizontal();
							if( GUILayout.Button( "Update Image", EditorStyles.miniButtonLeft ) )
							{
								targ.UltimateStatusList[ i ].statusImage.color = statusColor[ i ].colorValue;
								EditorUtility.SetDirty( targ.UltimateStatusList[ i ].statusImage );
								UltimateStatusColorWarning[ i ].target = GetStatusColorWarning( i );
							}
							if( GUILayout.Button( "Update Status", EditorStyles.miniButtonRight ) )
							{
								statusColor[ i ].colorValue = targ.UltimateStatusList[ i ].statusImage.color;
								serializedObject.ApplyModifiedProperties();
								UltimateStatusColorWarning[ i ].target = GetStatusColorWarning( i );
							}
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.EndVertical();
						}
						if( StatusInformation.faded == 1.0f && UltimateStatusBase[ i ].faded == 1.0f )
							EditorGUILayout.EndFadeGroup();
						// ----- < END STATUS COLORS > ----- //

						// ----- < TEST VALUE > ----- //
						EditorGUI.BeginChangeCheck();
						testValue[ i ] = EditorGUILayout.Slider( new GUIContent( "Test Value" ), testValue[ i ], 0.0f, 100.0f );
						if( EditorGUI.EndChangeCheck() )
						{
							if( targ.UltimateStatusList[ i ].statusImage != null )
							{
								targ.UltimateStatusList[ i ].statusImage.enabled = false;
								targ.UltimateStatusList[ i ].UpdateStatus( testValue[ i ], 100.0f );
								targ.UltimateStatusList[ i ].statusImage.enabled = true;

								EditorUtility.SetDirty( targ.UltimateStatusList[ i ].statusImage );
							}
						}
						// ----- < END TEST VALUE > ----- //

						if( EditorGUILayout.BeginFadeGroup( UltimateStatusAdvanced[ i ].faded ) )
						{
							EditorGUILayout.Space();
							EditorGUILayout.LabelField( statusName[ i ].stringValue == string.Empty ? "Advanced Options" : statusName[ i ].stringValue + " Options", EditorStyles.boldLabel );

							// ------- < TEXT OPTIONS > ------- //
							EditorGUI.BeginChangeCheck();
							EditorGUILayout.PropertyField( displayText[ i ], new GUIContent( "Display Text", "Determines how this status will display text to the user." ) );
							if( EditorGUI.EndChangeCheck() )
							{
								serializedObject.ApplyModifiedProperties();
								UltimateStatusAdvancedText[ i ].target = targ.UltimateStatusList[ i ].displayText != UltimateStatusBar.UltimateStatus.DisplayText.Disabled;

								targ.UltimateStatusList[ i ].UpdateStatus( testValue[ i ], 100.0f );
								if( statusText[ i ].objectReferenceValue != null )
									EditorUtility.SetDirty( targ.UltimateStatusList[ i ].statusText );
							}

							if( EditorGUILayout.BeginFadeGroup( UltimateStatusAdvancedText[ i ].faded ) )
							{
								EditorGUI.indentLevel = 1;

								EditorGUI.BeginChangeCheck();
								EditorGUILayout.PropertyField( statusText[ i ], new GUIContent( "Status Text", "The Text component to be used for the status text." ) );
								if( EditorGUI.EndChangeCheck() )
								{
									serializedObject.ApplyModifiedProperties();
									targ.UltimateStatusList[ i ].UpdateStatusTextColor( statusTextColor[ i ] );
									targ.UltimateStatusList[ i ].UpdateStatus( testValue[ i ], 100.0f );
									if( statusText[ i ].objectReferenceValue != null )
										EditorUtility.SetDirty( targ.UltimateStatusList[ i ].statusText );
								}

								EditorGUI.BeginChangeCheck();
								statusTextColor[ i ] = EditorGUILayout.ColorField( new GUIContent( "Text Color", "The color of the Text component." ), statusTextColor[ i ] );
								if( EditorGUI.EndChangeCheck() )
								{
									serializedObject.ApplyModifiedProperties();
									targ.UltimateStatusList[ i ].UpdateStatusTextColor( statusTextColor[ i ] );
									if( statusText[ i ].objectReferenceValue != null )
										EditorUtility.SetDirty( targ.UltimateStatusList[ i ].statusText );
								}

								EditorGUI.BeginChangeCheck();
								EditorGUILayout.PropertyField( additionalText[ i ], new GUIContent( "Additional Text", "Additional text to be displayed before the current status information." ) );
								if( EditorGUI.EndChangeCheck() )
								{
									serializedObject.ApplyModifiedProperties();
									targ.UltimateStatusList[ i ].UpdateStatus( testValue[ i ], 100.0f );
									if( statusText[ i ].objectReferenceValue != null )
										EditorUtility.SetDirty( targ.UltimateStatusList[ i ].statusText );
								}

								EditorGUI.indentLevel = 2;
								switch( targ.UltimateStatusList[ i ].displayText )
								{
									case UltimateStatusBar.UltimateStatus.DisplayText.Percentage:
									{
										EditorGUILayout.LabelField( "Text Preview: " + targ.UltimateStatusList[ i ].additionalText + testValue[ i ] + "%" );
									}
									break;
									case UltimateStatusBar.UltimateStatus.DisplayText.CurrentValue:
									{
										EditorGUILayout.LabelField( "Text Preview: " + targ.UltimateStatusList[ i ].additionalText + testValue[ i ] );
									}
									break;
									case UltimateStatusBar.UltimateStatus.DisplayText.CurrentAndMaxValues:
									{
										EditorGUILayout.LabelField( "Text Preview: " + targ.UltimateStatusList[ i ].additionalText + testValue[ i ] + " / 100" );
									}
									break;
									default:
									{
										EditorGUILayout.LabelField( "Text Preview: Default" );
									}
									break;
								}
								EditorGUI.indentLevel = 0;
								EditorGUILayout.Space();
							}
							if( UltimateStatusBase[ i ].faded == 1.0f && UltimateStatusAdvanced[ i ].faded == 1.0f )
								EditorGUILayout.EndFadeGroup();
							// ----- < END TEXT OPTIONS > ----- //

							// ------- < SMOOTH FILL > ------- //
							EditorGUI.BeginChangeCheck();
							EditorGUILayout.PropertyField( smoothFill[ i ], new GUIContent( "Smooth Fill", "Determines if the status should smoothly transition from it's current value." ) );
							if( EditorGUI.EndChangeCheck() )
							{
								serializedObject.ApplyModifiedProperties();
								UltimateStatusAdvancedSmoothFill[ i ].target = targ.UltimateStatusList[ i ].smoothFill;
							}

							if( EditorGUILayout.BeginFadeGroup( UltimateStatusAdvancedSmoothFill[ i ].faded ) )
							{
								EditorGUI.indentLevel = 1;

								EditorGUI.BeginChangeCheck();
								EditorGUILayout.PropertyField( smoothFillDuration[ i ], new GUIContent( "Fill Duration", "The time in seconds to reach the target fill amount." ) );
								if( EditorGUI.EndChangeCheck() )
								{
									if( smoothFillDuration[ i ].floatValue < 0 )
										smoothFillDuration[ i ].floatValue = 0;
									serializedObject.ApplyModifiedProperties();
								}

								EditorGUI.indentLevel = 0;
								EditorGUILayout.Space();
							}
							if( UltimateStatusBase[ i ].faded == 1.0f && UltimateStatusAdvanced[ i ].faded == 1.0f )
								EditorGUILayout.EndFadeGroup();
							// ----- < END SMOOTH FILL > ----- //

							// ----- < FILL CONSTRAINT > ----- //
							EditorGUI.BeginChangeCheck();
							EditorGUILayout.PropertyField( fillConstraint[ i ], new GUIContent( "Fill Constraint", "Determines whether or not the image fill should be constrained." ) );
							if( EditorGUI.EndChangeCheck() )
							{
								serializedObject.ApplyModifiedProperties();
								UltimateStatusAdvancedFillConstraint[ i ].target = targ.UltimateStatusList[ i ].fillConstraint;
							}

							if( EditorGUILayout.BeginFadeGroup( UltimateStatusAdvancedFillConstraint[ i ].faded ) )
							{
								EditorGUI.indentLevel = 1;

								EditorGUI.BeginChangeCheck();
								EditorGUILayout.Slider( fillConstraintMin[ i ], 0.0f, targ.UltimateStatusList[ i ].fillConstraintMax, new GUIContent( "Fill Minimum", "The minimum fill amount." ) );
								EditorGUILayout.Slider( fillConstraintMax[ i ], targ.UltimateStatusList[ i ].fillConstraintMin, 1.0f, new GUIContent( "Fill Maximum", "The maximum fill amount." ) );
								if( EditorGUI.EndChangeCheck() )
								{
									serializedObject.ApplyModifiedProperties();
									if( targ.UltimateStatusList[ i ].statusImage != null )
									{
										targ.UltimateStatusList[ i ].statusImage.enabled = false;
										targ.UltimateStatusList[ i ].UpdateStatus( testValue[ i ], 100.0f );
										targ.UltimateStatusList[ i ].statusImage.enabled = true;
									}
								}

								EditorGUI.indentLevel = 0;
								if( UltimateStatusAdvancedOnStatusUpdated[ i ].faded == 1.0f )
									EditorGUILayout.Space();
							}
							if( UltimateStatusBase[ i ].faded == 1.0f && UltimateStatusAdvanced[ i ].faded == 1.0f )
								EditorGUILayout.EndFadeGroup();
							// --- < END FILL CONSTRAINT > --- //

							// ------- < KEEP VISIBLE > ------ //
							if( EditorGUILayout.BeginFadeGroup( UltimateStatusAdvancedOnStatusUpdated[ i ].faded ) )
							{
								EditorGUI.BeginChangeCheck();
								EditorGUILayout.PropertyField( keepVisible[ i ], new GUIContent( "Keep Visible", "Determines if this status will force the Ultimate Status Bar to stay visible." ) );
								if( EditorGUI.EndChangeCheck() )
								{
									serializedObject.ApplyModifiedProperties();
									UltimateStatusAdvancedKeepVisible[ i ].target = targ.UltimateStatusList[ i ].keepVisible;
								}

								if( EditorGUILayout.BeginFadeGroup( UltimateStatusAdvancedKeepVisible[ i ].faded ) )
								{
									EditorGUI.indentLevel = 1;
									EditorGUI.BeginChangeCheck();
									EditorGUILayout.Slider( triggerValue[ i ], 0.0f, 1.0f, new GUIContent( "Trigger Value", "The percentage value to keep the Ultimate Status Bar visible." ) );
									if( EditorGUI.EndChangeCheck() )
										serializedObject.ApplyModifiedProperties();
									EditorGUI.indentLevel = 0;
								}
								if( UltimateStatusBase[ i ].faded == 1.0f && UltimateStatusAdvanced[ i ].faded == 1.0f && UltimateStatusAdvancedOnStatusUpdated[ i ].faded == 1.0f )
									EditorGUILayout.EndFadeGroup();
							}
							if( UltimateStatusBase[ i ].faded == 1.0f && UltimateStatusAdvanced[ i ].faded == 1.0f )
								EditorGUILayout.EndFadeGroup();
							// ----- < END KEEP VISIBLE > ---- //

							EditorGUILayout.Space();
						}
						if( StatusInformation.faded == 1.0f && UltimateStatusBase[ i ].faded == 1.0f )
							EditorGUILayout.EndFadeGroup();

						// ----- < EDIT TOOLBAR > ---- //
						EditorGUILayout.BeginHorizontal();
						if( GUILayout.Button( UltimateStatusAdvanced[ i ].target == true ? "Hide Options" : "Show Options", EditorStyles.miniButtonLeft ) )
						{
							UltimateStatusAdvanced[ i ].target = !UltimateStatusAdvanced[ i ].target;
							EditorPrefs.SetBool( "UUI_USB_Advanced" + i.ToString(), UltimateStatusAdvanced[ i ].target );
						}
						EditorGUI.BeginDisabledGroup( Application.isPlaying );
						if( GUILayout.Button( "Create", EditorStyles.miniButtonMid ) )
						{
							AddNewStatus( i + 1 );
						}
						EditorGUI.BeginDisabledGroup( targ.UltimateStatusList.Count == 1 );
						if( GUILayout.Button( "Delete", EditorStyles.miniButtonRight ) )
						{
							if( EditorUtility.DisplayDialog( "Ultimate Status Bar", "Warning!\n\nAre you sure that you want to delete " + ( statusName[ i ].stringValue != string.Empty ? "the " + statusName[ i ].stringValue : "this" ) + " status?", "Yes", "No" ) )
							{
								RemoveStatus( i );
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
					if( StatusInformation.faded == 1.0f )
						EditorGUILayout.EndFadeGroup();
				}
			}
		}
		EditorGUILayout.EndFadeGroup();
		/* ------------------------------------ > END STATUS INFORMATION < ------------------------------------ */
		#endregion

		EditorGUILayout.Space();

		#region SCRIPT REFERENCE
		/* -------------------------------------- > REFERENCE < -------------------------------------- */
		DisplayHeaderDropdown( "Script Reference", "UUI_ScriptReference", ScriptReference );
		if( EditorGUILayout.BeginFadeGroup( ScriptReference.faded ) )
		{
			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( statusBarName, new GUIContent( "Status Bar Name", "The name to be used for reference from scripts." ) );
			if( EditorGUI.EndChangeCheck() )
			{
				serializedObject.ApplyModifiedProperties();

				UltimateStatusBarNameDuplicate.target = DuplicateUltimateStatusBarName();
				UltimateStatusBarNameUnassigned.target = targ.statusBarName == string.Empty ? true : false;
				UltimateStatusBarNameAssigned.target = UltimateStatusBarNameDuplicate.target == false && targ.statusBarName != string.Empty ? true : false;
			}

			if( EditorGUILayout.BeginFadeGroup( UltimateStatusBarNameUnassigned.faded ) )
			{
				EditorGUILayout.HelpBox( "Please make sure to assign a name so that this status bar can be referenced from your scripts.", MessageType.Warning );
			}
			if( ScriptReference.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();

			if( EditorGUILayout.BeginFadeGroup( UltimateStatusBarNameDuplicate.faded ) )
			{
				EditorGUILayout.HelpBox( "This name has already been used in your scene. Please make sure to make the Status Bar Name unique.", MessageType.Error );
			}
			if( ScriptReference.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();

			if( EditorGUILayout.BeginFadeGroup( UltimateStatusBarNameAssigned.faded ) )
			{
				EditorGUILayout.BeginVertical( "Box" );
				GUILayout.Space( 1 );
				EditorGUILayout.LabelField( "Example Code Generator", EditorStyles.boldLabel );
				EditorGUI.BeginChangeCheck();
				exampleCodeIndex = EditorGUILayout.Popup( "Function", exampleCodeIndex, exampleCodeOptions.ToArray() );
				if( EditorGUI.EndChangeCheck() )
				{
					if( exampleCodeIndex == 0 )
					{
						if( statusNameList.Count > 0 && UltimateStatusBarStatusNames.target == false )
							UltimateStatusBarStatusNames.target = true;
					}
					else
					{
						if( UltimateStatusBarStatusNames.target == true )
							UltimateStatusBarStatusNames.target = false;
					}
				}
				
				if( EditorGUILayout.BeginFadeGroup( UltimateStatusBarStatusNames.faded ) )
				{
					statusNameListIndex = EditorGUILayout.Popup( "Status Name", statusNameListIndex, statusNameList.ToArray() );
				}
				if( ScriptReference.faded == 1.0f && UltimateStatusBarNameAssigned.faded == 1.0f )
					EditorGUILayout.EndFadeGroup();

				EditorGUILayout.TextField( GenerateExampleCode( exampleCodeList[ exampleCodeIndex ].basicCode, statusBarName.stringValue, ( UltimateStatusBarStatusNames.target == true ? statusNameList[ statusNameListIndex ] : "" ) ) );

				GUILayout.Space( 1 );
				EditorGUILayout.EndVertical();
			}
			if( ScriptReference.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();
		}
		EditorGUILayout.EndFadeGroup();
		/* ------------------------------------ > END REFERENCE < ------------------------------------ */
		#endregion

		EditorGUILayout.Space();

		Repaint ();
	}

	void StoreNameList ()
	{
		statusNameList = new List<string>();

		if( Selection.gameObjects.Length > 1 )
			return;

		for( int i = 0; i < statusName.Count; i++ )
		{
			if( statusName[ i ].stringValue != string.Empty )
				statusNameList.Add( statusName[ i ].stringValue );
		}
	}

	void AddNewStatus ( int index )
	{
		serializedObject.FindProperty( "UltimateStatusList" ).InsertArrayElementAtIndex( index );
		//serializedObject.FindProperty( "UltimateStatusList" ).arraySize++;// THIS IS THE KEY!!!
		serializedObject.ApplyModifiedProperties();
		
		// Assign default values so that the previous index values are not copied.
		targ.UltimateStatusList[ index ] = new UltimateStatusBar.UltimateStatus();

		UltimateStatusBase.Insert( index, new AnimBool( false ) );
		UltimateStatusBase[ index ].target = true;
		UltimateStatusAdvanced.Insert( index, new AnimBool( false ) );

		EditorUtility.SetDirty( targ );

		// Store the references to get the information.
		StoreReferences();
	}

	void RemoveStatus ( int index )
	{
		serializedObject.FindProperty( "UltimateStatusList" ).DeleteArrayElementAtIndex( index );
		serializedObject.ApplyModifiedProperties();

		UltimateStatusBase.RemoveAt( index );
		UltimateStatusAdvanced.RemoveAt( index );

		StoreReferences();
	}

	void StoreReferences ()
	{
		// Assign the current target.
		targ = ( UltimateStatusBar ) target;

		if( targ.statusBarIcon != null )
			statusBarIconColor = targ.statusBarIcon.color;
		StatusBarIconSection = new AnimBool( targ.statusBarIcon != null );
		
		if( targ.statusBarText != null )
		{
			statusBarTextColor = targ.statusBarText.color;
			tempStatusString = targ.statusBarText.text;
		}
		StatusBarTextSection = new AnimBool( targ.statusBarText != null );

		// STATUS BAR POSITIONING // SCREEN SPACE //
		PositioningScreenSpace = new AnimBool( targ.positioningOption == UltimateStatusBar.PositioningOption.ScreenSpace );
		PositioningFollowCam = new AnimBool( targ.positioningOption == UltimateStatusBar.PositioningOption.WorldSpace );
		PositioningScreenSpaceError = new AnimBool( GetPositioningScreenSpaceError() );
		ImageRatioPreserve = new AnimBool( targ.screenSpaceOptions.imageAspectRatio == UltimateStatusBar.ScreenSpaceOptions.ImageAspectRatio.Preserve );
		ImageRatioCustom = new AnimBool( targ.screenSpaceOptions.imageAspectRatio == UltimateStatusBar.ScreenSpaceOptions.ImageAspectRatio.Custom );
		ImageRatioImageUnassigned = new AnimBool( targ.screenSpaceOptions.targetImage == null ? true : false );
		ImageRatioSpriteUnassigned = new AnimBool( targ.screenSpaceOptions.targetImage != null && targ.screenSpaceOptions.targetImage.sprite == null ? true : false );
		positioningOption = serializedObject.FindProperty( "positioningOption" );
		scalingAxis = serializedObject.FindProperty( "screenSpaceOptions.scalingAxis" );
		statusBarSize = serializedObject.FindProperty( "screenSpaceOptions.statusBarSize" );
		imageAspectRatio = serializedObject.FindProperty( "screenSpaceOptions.imageAspectRatio" );
		targetImage = serializedObject.FindProperty( "screenSpaceOptions.targetImage" );
		xRatio = serializedObject.FindProperty( "screenSpaceOptions.xRatio" );
		yRatio = serializedObject.FindProperty( "screenSpaceOptions.yRatio" );
		xPosition = serializedObject.FindProperty( "screenSpaceOptions.xPosition" );
		yPosition = serializedObject.FindProperty( "screenSpaceOptions.yPosition" );
		// STATUS BAR POSITIONING // WORLD SPACE //
		FindByTransform = new AnimBool( targ.worldSpaceOptions.findBy == UltimateStatusBar.WorldSpaceOptions.FindBy.Transform );
		FindByName = new AnimBool( targ.worldSpaceOptions.findBy == UltimateStatusBar.WorldSpaceOptions.FindBy.Name );
		FindByTag = new AnimBool( targ.worldSpaceOptions.findBy == UltimateStatusBar.WorldSpaceOptions.FindBy.Tag );
		FollowCamUnassigned = new AnimBool( targ.worldSpaceOptions.cameraTransform == null );
		WarningCanvasScale = new AnimBool( GetPositioningScreenSpaceCanvasScale() );
		findBy = serializedObject.FindProperty( "worldSpaceOptions.findBy" );
		targetName = serializedObject.FindProperty( "worldSpaceOptions.targetName" );
		cameraTransform = serializedObject.FindProperty( "worldSpaceOptions.cameraTransform" );

		statusBarIcon = serializedObject.FindProperty( "statusBarIcon" );
		statusBarText = serializedObject.FindProperty( "statusBarText" );
		updateVisibility = serializedObject.FindProperty( "updateVisibility" );
		updateUsing = serializedObject.FindProperty( "updateUsing" );
		idleSeconds = serializedObject.FindProperty( "idleSeconds" );
		enableDuration = serializedObject.FindProperty( "enableDuration" );
		disableDuration = serializedObject.FindProperty( "disableDuration" );
		enabledAlpha = serializedObject.FindProperty( "enabledAlpha" );
		disabledAlpha = serializedObject.FindProperty( "disabledAlpha" );
		statusBarAnimator = serializedObject.FindProperty( "statusBarAnimator" );
		initialState = serializedObject.FindProperty( "initialState" );

		// If the status bar has no status information registered, then create a new status.
		if( targ.UltimateStatusList.Count == 0 )
		{
			serializedObject.FindProperty( "UltimateStatusList" ).arraySize++;
			serializedObject.ApplyModifiedProperties();
			targ.UltimateStatusList[ 0 ] = new UltimateStatusBar.UltimateStatus();
			EditorUtility.SetDirty( targ );
		}

		// ------------------------------ < ANIMATED SECTIONS  > ------------------------------ //
		StatusBarPositioning = new AnimBool( EditorPrefs.GetBool( "UUI_SizeAndPlacement" ) );
		StatusBarOptions = new AnimBool( EditorPrefs.GetBool( "UUI_StyleAndOptions" ) );
		StatusInformation = new AnimBool( EditorPrefs.GetBool( "UUI_USB_StatusInformation" ) );
		ScriptReference = new AnimBool( EditorPrefs.GetBool( "UUI_ScriptReference" ) );
		// ---------------------------- < END ANIMATED SECTIONS  > ---------------------------- //

		// SCRIPT REFERENCE //
		UltimateStatusBarNameDuplicate = new AnimBool( DuplicateUltimateStatusBarName() );
		UltimateStatusBarNameUnassigned = new AnimBool( targ.statusBarName == string.Empty );
		UltimateStatusBarNameAssigned = new AnimBool( UltimateStatusBarNameDuplicate.target == false && targ.statusBarName != string.Empty );

		UpdateVisibilty = new AnimBool( targ.updateVisibility != UltimateStatusBar.UpdateVisibility.Never );
		UpdateVisibilityOnStatusUpdated = new AnimBool( targ.updateVisibility == UltimateStatusBar.UpdateVisibility.OnStatusUpdated );
		UpdateVisibiltyAnimation = new AnimBool( targ.updateUsing == UltimateStatusBar.UpdateUsing.Animation );
		UpdateVisibilityFade = new AnimBool( targ.updateUsing == UltimateStatusBar.UpdateUsing.Fade );

		statusBarName = serializedObject.FindProperty( "statusBarName" );

		// Reset all list properties.
		statusName = new List<SerializedProperty>();
		statusImage = new List<SerializedProperty>();
		colorMode = new List<SerializedProperty>();
		statusColor = new List<SerializedProperty>();
		statusGradient = new List<SerializedProperty>();
		displayText = new List<SerializedProperty>();
		statusText = new List<SerializedProperty>();
		additionalText = new List<SerializedProperty>();
		smoothFill = new List<SerializedProperty>();
		smoothFillDuration = new List<SerializedProperty>();
		fillConstraint = new List<SerializedProperty>();
		fillConstraintMin = new List<SerializedProperty>();
		fillConstraintMax = new List<SerializedProperty>();
		keepVisible = new List<SerializedProperty>();
		triggerValue = new List<SerializedProperty>();
		testValue = new List<float>();
		statusTextColor = new List<Color>();

		// Reset list sections.
		UltimateStatusNameWarning = new List<AnimBool>();
		UltimateStatusNameError = new List<AnimBool>();
		UltimateStatusAdvancedText = new List<AnimBool>();
		UltimateStatusAdvancedSmoothFill = new List<AnimBool>();
		UltimateStatusAdvancedFillConstraint = new List<AnimBool>();
		UltimateStatusAdvancedOnStatusUpdated = new List<AnimBool>();
		UltimateStatusAdvancedKeepVisible = new List<AnimBool>();
		UltimateStatusColorWarning = new List<AnimBool>();
		UltimateStatusBarImageWarning = new List<AnimBool>();

		for( int i = 0; i < targ.UltimateStatusList.Count; i++ )
		{
			testValue.Add( 100.0f );
			if( targ.UltimateStatusList[ i ].statusImage != null )
				testValue[ i ]= targ.UltimateStatusList[ i ].GetCurrentCalculatedFraction * 100;

			statusName.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].statusName", i ) ) );
			statusImage.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].statusImage", i ) ) );
			colorMode.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].colorMode", i ) ) );
			statusColor.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].statusColor", i ) ) );
			statusGradient.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].statusGradient", i ) ) );
			displayText.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].displayText", i ) ) );
			statusText.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].statusText", i ) ) );
			additionalText.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].additionalText", i ) ) );
			smoothFill.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].smoothFill", i ) ) );
			smoothFillDuration.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].smoothFillDuration", i ) ) );
			fillConstraint.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].fillConstraint", i ) ) );
			fillConstraintMin.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].fillConstraintMin", i ) ) );
			fillConstraintMax.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].fillConstraintMax", i ) ) );
			keepVisible.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].keepVisible", i ) ) );
			triggerValue.Add( serializedObject.FindProperty( string.Format( "UltimateStatusList.Array.data[{0}].triggerValue", i ) ) );

			statusTextColor.Add( targ.UltimateStatusList[ i ].statusText != null ? targ.UltimateStatusList[ i ].statusText.color : Color.white );

			UltimateStatusNameWarning.Add( new AnimBool( targ.UltimateStatusList[ i ].statusName == string.Empty && targ.UltimateStatusList.Count > 1 ) );
			UltimateStatusNameError.Add( new AnimBool( targ.UltimateStatusList[ i ].statusName != string.Empty && DuplicateStatusName( i ) ) );
			UltimateStatusAdvancedText.Add( new AnimBool( targ.UltimateStatusList[ i ].displayText != UltimateStatusBar.UltimateStatus.DisplayText.Disabled ) );
			UltimateStatusAdvancedSmoothFill.Add( new AnimBool( targ.UltimateStatusList[ i ].smoothFill ) );
			UltimateStatusAdvancedFillConstraint.Add( new AnimBool( targ.UltimateStatusList[ i ].fillConstraint ) );
			UltimateStatusAdvancedOnStatusUpdated.Add( new AnimBool( targ.updateVisibility == UltimateStatusBar.UpdateVisibility.OnStatusUpdated ) );
			UltimateStatusAdvancedKeepVisible.Add( new AnimBool( targ.UltimateStatusList[ i ].keepVisible == true ) );

			UltimateStatusBarImageWarning.Add( new AnimBool( GetStatusBarImageWarning( i ) ) );
			UltimateStatusColorWarning.Add( new AnimBool( GetStatusColorWarning( i ) ) );

			// Update the status bar with the current value so that options will be applied.
			if( Application.isPlaying == false )
				targ.UltimateStatusList[ i ].UpdateStatus( testValue[ i ], 100 );
		}
		StoreNameList();

		UltimateStatusBarStatusNames = new AnimBool( statusNameList.Count > 0 && exampleCodeIndex == 0 );
	}

	#region ANIMBOOL GET FUNCTIONS
	bool DuplicateUltimateStatusBarName ()
	{
		UltimateStatusBar[] ultimateStatusBars = FindObjectsOfType<UltimateStatusBar>();
		
		for( int i = 0; i < ultimateStatusBars.Length; i++ )
		{
			if( ultimateStatusBars[ i ].statusBarName == string.Empty )
				continue;

			if( ultimateStatusBars[ i ] != targ && ultimateStatusBars[ i ].statusBarName == targ.statusBarName )
				return true;
		}

		return false;
	}

	bool DuplicateStatusName ( int index )
	{
		if( statusName[ index ].stringValue == string.Empty )
			return false;

		for( int i = 0; i < statusName.Count; i++ )
		{
			if( i == index )
				continue;

			if( statusName[ i ].stringValue == statusName[ index ].stringValue )
				return true;
		}
		return false;
	}

	bool GetStatusBarImageWarning ( int index )
	{
		if( targ.UltimateStatusList[ index ].statusImage != null && targ.UltimateStatusList[ index ].statusImage.type != UnityEngine.UI.Image.Type.Filled )
			return true;
		return false;
	}

	bool GetStatusColorWarning ( int index )
	{
		if( Application.isPlaying == true )
			return false;

		if( targ.UltimateStatusList[ index ].statusImage == null )
			return false;

		if( targ.UltimateStatusList[ index ].colorMode == UltimateStatusBar.UltimateStatus.ColorMode.Single && targ.UltimateStatusList[ index ].statusImage.color != targ.UltimateStatusList[ index ].statusColor )
			return true;

		return false;
	}

	bool GetPositioningScreenSpaceError ()
	{
		// If the selection is currently empty, then return false.
		if( Selection.activeGameObject == null )
			return false;

		// If the selection is actually the prefab within the Project window, then return no errors.
		if( PrefabUtility.GetPrefabType( Selection.activeGameObject ) == PrefabType.Prefab )
			return false;

		// If parentCanvas is unassigned, then get a new canvas and return no errors.
		if( parentCanvas == null )
		{
			parentCanvas = GetParentCanvas();
			return false;
		}

		if( targ.positioningOption != UltimateStatusBar.PositioningOption.ScreenSpace )
			return false;
		
		// If the parentCanvas is not enabled, then return true for errors.
		if( parentCanvas.enabled == false )
			return true;

		// If the canvas' renderMode is not the needed one, then return true for errors.
		if( parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay )
			return true;

		// If the canvas has a CanvasScaler component and it is not the correct option.
		if( parentCanvas.GetComponent<CanvasScaler>() && parentCanvas.GetComponent<CanvasScaler>().uiScaleMode != CanvasScaler.ScaleMode.ConstantPixelSize )
			return true;

		return false;
	}

	bool GetPositioningScreenSpaceCanvasScale ()
	{
		if( parentCanvas == null )
			return false;

		if( !GetPositioningScreenSpaceError() && parentCanvas.GetComponent<RectTransform>().localScale == Vector3.one )
			return true;

		return false;
	}
	#endregion

	Canvas GetParentCanvas ()
	{
		if( Selection.activeGameObject == null )
			return null;

		// Store the current parent.
		Transform parent = Selection.activeGameObject.transform.parent;

		// Loop through parents as long as there is one.
		while( parent != null )
		{ 
			// If there is a Canvas component, return that gameObject.
			if( parent.transform.GetComponent<Canvas>() && parent.transform.GetComponent<Canvas>().enabled == true )
				return parent.transform.GetComponent<Canvas>();
			
			// Else, shift to the next parent.
			parent = parent.transform.parent;
		}
		if( parent == null && PrefabUtility.GetPrefabType( Selection.activeGameObject ) != PrefabType.Prefab )
			UltimateStatusBarCreator.RequestCanvas( Selection.activeGameObject );
		return null;
	}

	string GenerateExampleCode ( string basicCode, string name, string name2 = "" )
	{
		if( name2 != string.Empty )
			name += "\", \"" + name2;

		return string.Format( basicCode, name );
	}

	void UpdateStatusColor ( int index )
	{
		// If the status image component is null, then return.
		if( targ.UltimateStatusList[ index ].statusImage == null )
			return;

		// Switch statement for the color mode option. Each case handles the color according to the option.
		switch( targ.UltimateStatusList[ index ].colorMode )
		{
			case UltimateStatusBar.UltimateStatus.ColorMode.Single:
			{
				targ.UltimateStatusList[ index ].statusImage.color = targ.UltimateStatusList[ index ].statusColor;
			} break;
			case UltimateStatusBar.UltimateStatus.ColorMode.Gradient:
			{
				targ.UltimateStatusList[ index ].statusImage.color = targ.UltimateStatusList[ index ].statusGradient.Evaluate( targ.UltimateStatusList[ index ].GetCurrentCalculatedFraction );
			} break;
		}
		EditorUtility.SetDirty( targ.UltimateStatusList[ index ].statusImage );
	}
}

/* Written by Kaz Crowe */
/* UltimateStatusBarCreator.cs */
public class UltimateStatusBarCreator
	{
		[MenuItem( "GameObject/Ultimate UI/Ultimate Status Bar", false, 20 )]
		private static void CreateUltimateStatusBar ()
		{
			GameObject prefab = EditorGUIUtility.Load( "Ultimate Status Bar/UltimateStatusBar.prefab" ) as GameObject;

			if( prefab == null )
			{
				Debug.LogError( "Could not find 'UltimateStatusBar.prefab' in any Editor Default Resources folders." );
				return;
			}
			CreateNewUI( prefab );
		}

		private static void CreateNewUI ( Object objectPrefab )
		{
			GameObject prefab = ( GameObject )Object.Instantiate( objectPrefab, Vector3.zero, Quaternion.identity );
			prefab.name = objectPrefab.name;
			Selection.activeGameObject = prefab;
			RequestCanvas( prefab );
		}

		private static void CreateNewCanvas ( GameObject child )
		{
			GameObject root = new GameObject( "Ultimate UI Canvas" );
			root.layer = LayerMask.NameToLayer( "UI" );
			Canvas canvas = root.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			root.AddComponent<GraphicRaycaster>();
			Undo.RegisterCreatedObjectUndo( root, "Create " + root.name );

			child.transform.SetParent( root.transform, false );
		
			CreateEventSystem();
		}

		private static void CreateEventSystem ()
		{
			Object esys = Object.FindObjectOfType<EventSystem>();
			if( esys == null )
			{
				GameObject eventSystem = new GameObject( "EventSystem" );
				esys = eventSystem.AddComponent<EventSystem>();
				eventSystem.AddComponent<StandaloneInputModule>();

				Undo.RegisterCreatedObjectUndo( eventSystem, "Create " + eventSystem.name );
			}
		}

		/* PUBLIC STATIC FUNCTIONS */
		public static void RequestCanvas ( GameObject child )
		{
			Canvas[] allCanvas = Object.FindObjectsOfType( typeof( Canvas ) ) as Canvas[];

			for( int i = 0; i < allCanvas.Length; i++ )
			{
				if( allCanvas[ i ].renderMode == RenderMode.ScreenSpaceOverlay && allCanvas[ i ].enabled == true && !allCanvas[ i ].GetComponent<CanvasScaler>() )
				{
					child.transform.SetParent( allCanvas[ i ].transform, false );
					CreateEventSystem();
					return;
				}
			}
			CreateNewCanvas( child );
		}

		public static void RequestNewWorldSpaceCanvas ( GameObject child )
		{
			CreateNewCanvas( child );
		}
	}