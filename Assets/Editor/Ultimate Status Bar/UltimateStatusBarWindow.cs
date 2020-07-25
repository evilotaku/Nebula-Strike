/* Written by Kaz Crowe */
/* UltimateStatusBarWindow.cs */
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Collections.Generic;

public class UltimateStatusBarWindow : EditorWindow
{
	static string version = "2.1.1";// ALWAYS UPDATE
	static int importantChanges = 2;// UPDATE ON IMPORTANT CHANGES
	static string menuTitle = "Main Menu";

	// Layout Styles //
	int sectionSpace = 20;
	int itemHeaderSpace = 10;
	int paragraphSpace = 5;
	GUIStyle sectionHeaderStyle = new GUIStyle();
	GUIStyle itemHeaderStyle = new GUIStyle();
	GUIStyle paragraphStyle = new GUIStyle();

	GUILayoutOption[] buttonSize = new GUILayoutOption[] { GUILayout.Width( 200 ), GUILayout.Height( 35 ) };
	GUILayoutOption[] docSize = new GUILayoutOption[] { GUILayout.Width( 300 ), GUILayout.Height( 330 ) };
	GUISkin style;
	Texture2D scriptRef_StatusInformation, scriptRef_ScriptReference;
	Texture2D ubPromo, ujPromo;
	
	class PageInformation
	{
		public string pageName = "";
		public Vector2 scrollPosition = Vector2.zero;
		public delegate void TargetMethod();
		public TargetMethod targetMethod;
	}
	static PageInformation mainMenu = new PageInformation() { pageName = "Main Menu" };
	static PageInformation howTo = new PageInformation() { pageName = "How To" };
	static PageInformation overview = new PageInformation() { pageName = "Overview" };
	static PageInformation overview_USB = new PageInformation() { pageName = "Ultimate Status Bar" };
	static PageInformation overview_DSF = new PageInformation() { pageName = "Dramatic Status Fill" };
	static PageInformation overview_ASH = new PageInformation() { pageName = "Alt. State Handler" };
	static PageInformation overview_SFF = new PageInformation() { pageName = "Status Fill Follower" };
	static PageInformation documentation = new PageInformation() { pageName = "Documentation" };
	static PageInformation documentation_US = new PageInformation() { pageName = "Ultimate Status" };
	static PageInformation documentation_USB = new PageInformation() { pageName = "Ultimate Status Bar" };
	static PageInformation documentation_ASH = new PageInformation() { pageName = "Alt. State Handler" };
	static PageInformation extras = new PageInformation() { pageName = "Extras" };
	static PageInformation otherProducts = new PageInformation() { pageName = "Other Products" };
	static PageInformation feedback = new PageInformation() { pageName = "Feedback" };
	static PageInformation changeLog = new PageInformation() { pageName = "Change Log" };
	static PageInformation plannedFeatures = new PageInformation() { pageName = "Planned Features" };
	static PageInformation versionChanges = new PageInformation() { pageName = "Version Changes" };
	static PageInformation thankYou = new PageInformation() { pageName = "Thank You" };
	static List<PageInformation> pageHistory = new List<PageInformation>();
	static PageInformation currentPage = new PageInformation();
	
	enum PositioningOption
	{
		Disabled,
		ScreenSpace,
		WorldSpace
	}
	PositioningOption positioningOption = PositioningOption.ScreenSpace;
	AnimBool overview_PositioningScreen = new AnimBool( true );
	AnimBool overview_PositioningWorld = new AnimBool( false );
	AnimBool overview_PositioningDisabled = new AnimBool( false );
	
	enum UpdateVisibility
	{
		Never,
		Manually,
		OnStatusUpdated
	}
	UpdateVisibility updateVisibility = UpdateVisibility.Never;
	AnimBool overview_VisibilityNever = new AnimBool( true );
	AnimBool overview_VisibilityManually = new AnimBool( false );
	AnimBool overview_VisibilityOnStatusUpdated = new AnimBool( false );
	AnimBool overview_UpdateVisibilityOptions = new AnimBool( false );
	
	enum UpdateUsing
	{
		Fade,
		Animation
	}
	UpdateUsing updateUsing = UpdateUsing.Fade;
	AnimBool overview_VisibilityFade = new AnimBool( true );
	AnimBool overview_VisibilityAnimation = new AnimBool( false );
	
	enum DramaticStyle
	{
		Increase,
		Decrease
	}
	DramaticStyle followStatus = DramaticStyle.Increase;
	AnimBool overview_DSF_StyleIncrease = new AnimBool( true );
	AnimBool overview_DSF_StyleDecrease = new AnimBool( false );

	enum TriggerOption
	{
		Manual,
		Percentage
	}
	TriggerOption triggerOption = TriggerOption.Manual;
	AnimBool overview_ASH_TriggerManual = new AnimBool( true );
	AnimBool overview_ASH_TriggerPercentage = new AnimBool( false );
	

	[MenuItem( "Window/Tank and Healer Studio/Ultimate Status Bar", false, 10 )]
	static void Init ()
	{
		InitializeWindow();
	}

	static void InitializeWindow ()
	{
		EditorWindow window = GetWindow<UltimateStatusBarWindow>( true, "Tank and Healer Studio Asset Window", true );
		window.maxSize = new Vector2( 500, 500 );
		window.minSize = new Vector2( 500, 500 );
		window.Show();
	}

	void OnEnable ()
	{
		style = ( GUISkin )EditorGUIUtility.Load( "Ultimate Status Bar/UltimateStatusBarEditorSkin.guiskin" );

		ubPromo = ( Texture2D )EditorGUIUtility.Load( "Ultimate UI/UB_Promo.png" );
		ujPromo = ( Texture2D )EditorGUIUtility.Load( "Ultimate UI/UJ_Promo.png" );
		scriptRef_StatusInformation = ( Texture2D )EditorGUIUtility.Load( "Ultimate Status Bar/USB_StatusInformation.jpg" );
		scriptRef_ScriptReference = ( Texture2D )EditorGUIUtility.Load( "Ultimate Status Bar/USB_ScriptReference.jpg" );
		
		if( !pageHistory.Contains( mainMenu ) )
			pageHistory.Insert( 0, mainMenu );

		mainMenu.targetMethod = MainMenu;
		howTo.targetMethod = HowTo;
		overview.targetMethod = OverviewPage;
		overview_USB.targetMethod = Overview_UltimateStatusBar;
		overview_DSF.targetMethod = Overview_DramaticStatusFill;
		overview_ASH.targetMethod = Overview_AlternateStateHandler;
		overview_SFF.targetMethod = Overview_StatusFillFollower;
		documentation.targetMethod = DocumentationPage;
		documentation_US.targetMethod = Documentation_UltimateStatus;
		documentation_USB.targetMethod = Documentation_UltimateStatusBar;
		documentation_ASH.targetMethod = Documentation_AlternateStateHandler;
		extras.targetMethod = Extras;
		otherProducts.targetMethod = OtherProducts;
		feedback.targetMethod = Feedback;
		changeLog.targetMethod = ChangeLog;
		plannedFeatures.targetMethod = PlannedFeatures;
		versionChanges.targetMethod = VersionChanges;
		thankYou.targetMethod = ThankYou;

		if( pageHistory.Count == 1 )
			currentPage = mainMenu;
	}
	
	void OnGUI ()
	{
		if( style == null )
		{
			GUILayout.BeginVertical( "Box" );
			GUILayout.FlexibleSpace();
			ErrorScreen();
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndVertical();
			return;
		}

		GUI.skin = style;

		paragraphStyle = GUI.skin.GetStyle( "ParagraphStyle" );
		itemHeaderStyle = GUI.skin.GetStyle( "ItemHeader" );
		sectionHeaderStyle = GUI.skin.GetStyle( "SectionHeader" );
		
		EditorGUILayout.Space();

		GUILayout.BeginVertical( "Box" );
		
		EditorGUILayout.LabelField( "Ultimate Status Bar", GUI.skin.GetStyle( "WindowTitle" ) );

		GUILayout.Space( 3 );
		
		if( GUILayout.Button( "Version " + version, GUI.skin.GetStyle( "VersionNumber" ) ) && currentPage != changeLog )
			NavigateForward( changeLog );

		GUILayout.Space( 12 );

		EditorGUILayout.BeginHorizontal();
		GUILayout.Space( 5 );
		if( pageHistory.Count > 1 )
		{
			if( GUILayout.Button( "", GUI.skin.GetStyle( "BackButton" ), GUILayout.Width( 80 ), GUILayout.Height( 40 ) ) )
				NavigateBack();
		}
		else
			GUILayout.Space( 80 );

		GUILayout.Space( 15 );
		EditorGUILayout.LabelField( menuTitle, GUI.skin.GetStyle( "MenuTitle" ) );
		GUILayout.FlexibleSpace();
		GUILayout.Space( 80 );
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( 10 );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if( currentPage.targetMethod != null )
			currentPage.targetMethod();

		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.Space( 25 );

		EditorGUILayout.EndVertical();

		Repaint();
	}

	void ErrorScreen ()
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space( 50 );
		EditorGUILayout.LabelField( "ERROR", EditorStyles.boldLabel );
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		GUILayout.Space( 50 );
		EditorGUILayout.LabelField( "Could not find the needed GUISkin located in the Editor Default Resources folder. Please ensure that the correct GUISkin, UltimateStatusBarEditorSkin, is in the right folder( Editor Default Resources/Ultimate Status Bar ) before trying to access the Ultimate Status Bar Window.", EditorStyles.wordWrappedLabel );
		GUILayout.Space( 50 );
		EditorGUILayout.EndHorizontal();
	}

	static void NavigateBack ()
	{
		pageHistory.RemoveAt( pageHistory.Count - 1 );
		menuTitle = pageHistory[ pageHistory.Count - 1 ].pageName;
		currentPage = pageHistory[ pageHistory.Count - 1 ];
	}

	static void NavigateForward ( PageInformation menu )
	{
		pageHistory.Add( menu );
		menuTitle = menu.pageName;
		currentPage = menu;
	}
	
	void MainMenu ()
	{
		mainMenu.scrollPosition = EditorGUILayout.BeginScrollView( mainMenu.scrollPosition, false, false, docSize );

		GUILayout.Space( 25 );
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "How To", buttonSize ) )
			NavigateForward( howTo );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Overview", buttonSize ) )
			NavigateForward( overview );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Documentation", buttonSize ) )
			NavigateForward( documentation );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Extras", buttonSize ) )
			NavigateForward( extras );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Other Products", buttonSize ) )
			NavigateForward( otherProducts );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Feedback", buttonSize ) )
			NavigateForward( feedback );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		EditorGUILayout.EndScrollView();
	}
	
	void HowTo ()
	{
		howTo.scrollPosition = EditorGUILayout.BeginScrollView( howTo.scrollPosition, false, false, docSize );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "How To Create", sectionHeaderStyle );

		EditorGUILayout.LabelField( "   There are several ways to create an Ultimate Status Bar within your scene. To <b>(1) create one from scratch</b> using your own images, go to GameObject / Ultimate UI / Ultimate Status Bar. This will create a base Ultimate Status Bar GameObject with only the basic objects to get your Ultimate Status Bar started. You could also <b>(2) use a Prefab</b> from the Ultimate Status Bar Prefabs folder. These prefabs are complete Ultimate Status Bar GameObjects with all the needed components. Simply drag and drop these prefabs into the Hierarchy window to get them in your scene. You could also <b>(3) attach the Ultimate Status Bar script</b> to your own status bar and customize it from there.", paragraphStyle );

		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "How To Customize", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   There are many ways to use the Ultimate Status Bar within your projects. The main Ultimate Status Bar component is used to display each status within your scene, while the Dramatic Status Fill, Alternate State Handler and Status Fill Follower are all used to enhance the visual display of the Ultimate Status Bar. For more information about each of the scripts, please see the Overview and Documentation sections of this help window. The Overview section describes each option in the inspector and what it does for each component, while the Documentation section explains each function available in each class. To start with each component, simply add the script to a UI GameObject in your scene.", paragraphStyle );
		
		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "How To Reference", sectionHeaderStyle );

		EditorGUILayout.LabelField( "   The Ultimate Status Bar is incredibly easy to get implemented into your custom scripts. There are a few ways that you can reference the Ultimate Status Bar through code, and it all depends on how many different status sections you have created on that particular Ultimate Status Bar. For more information on how to reference the Ultimate Status Bar, please see the Documentation section of this window, or the Script Reference section of the Ultimate Status Bar inspector.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "For this example, we will create an Ultimate Status Bar for the Player of a simple game. Let's assume that the Player has several different status values that must be displayed. For this example, the Player will have a <i>Health</i> value, and a <i>Energy</i> value. These will need to be created inside the <b>Status Information</b> section in order to be referenced through code.", paragraphStyle );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( scriptRef_StatusInformation != null )
			GUILayout.Label( scriptRef_StatusInformation );
		else
		{
			EditorGUILayout.BeginVertical();
			GUILayout.Space( sectionSpace );
			EditorGUILayout.LabelField( "<color=red>Image Missing</color>", paragraphStyle );
			GUILayout.Space( sectionSpace );
			EditorGUILayout.EndVertical();
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
			
		EditorGUILayout.LabelField( "After these have been created, we need to give the Ultimate Status Bar a unique name to be referenced through code. This is done in the <b>Script Reference</b> section located within the Inspector window. For this example, we are creating this status bar for the <i>Player</i>, so that's what we will name it.", paragraphStyle );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( scriptRef_ScriptReference != null )
			GUILayout.Label( scriptRef_ScriptReference );
		else
		{
			EditorGUILayout.BeginVertical();
			GUILayout.Space( sectionSpace );
			EditorGUILayout.LabelField( "<color=red>Image Missing</color>", paragraphStyle );
			GUILayout.Space( sectionSpace );
			EditorGUILayout.EndVertical();
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField( "Now that each status has been named, and the Ultimate Status Bar has a unique name that can be referenced, simply copy the code provided inside the <b>Script Reference</b> section for the desired status. Make sure that the Function option is set to Update Status.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "After copying the code that is provided, find the function in <i>your player's health script</i> where your player is receiving damage from and paste the example code into the function. Be sure to put it after the damage or healing has modified the health value. Of course, be sure to replace the currentValue and maxValue of the example code with your character's current and maximum health values. Whenever the character's health is updated, either by damage or healing done to the character, you will want to send the new information of the health's value.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "This process can be used for any status that you need to be displayed to the user. For more information about the individual functions available for the Ultimate Status Bar and other components, please refer to the Documentation section of this window.", paragraphStyle );
		
		EditorGUILayout.EndScrollView();
	}
	
	void OverviewPage ()
	{
		overview.scrollPosition = EditorGUILayout.BeginScrollView( overview.scrollPosition, false, false, docSize );

		EditorGUILayout.LabelField( "Introduction", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   Welcome to the Ultimate Status Bar Overview section. This section will go over the different options that can be found on the inspector window for each script of the Ultimate Status Bar Asset Package. Please see each section below to learn more about each inspector individually.", paragraphStyle );

		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "Ultimate Status Bar", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   The <i>Ultimate Status Bar</i> can be used to display the status of anything from your Player and Enemies in your scene, to the Loading Bar when starting your game. Click <b>More Info</b> to find out more about the options that you have available to you in the Ultimate Status Bar inspector.", paragraphStyle );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "More Info", buttonSize ) )
			NavigateForward( overview_USB );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "Dramatic Status Fill", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   The <i>Dramatic Status Fill</i> component uses a second image to display a dramatic effect to the user. This component helps to draw attention to the change in status amount. Click <b>More Info</b> below to find out more about the options that you have available to you in the Dramatic Status Fill inspector.", paragraphStyle );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "More Info", buttonSize ) )
			NavigateForward( overview_DSF );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "Alternate State Handler", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   The <i>Alternate State Handler</i> component allows you to display different states for each status in your scene. This can be done using images or text. Click <b>More Info</b> below to find out more about the options that you have available to you in the Alternate State Handler inspector.", paragraphStyle );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "More Info", buttonSize ) )
			NavigateForward( overview_ASH );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "Status Fill Follower", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   The <i>Status Fill Follower</i> component can be used to follow the current fill of an image to draw attention to the amount that it is at. Click <b>More Info</b> below to find out more about the options that you have available to you in the Status Fill Follower inspector.", paragraphStyle );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "More Info", buttonSize ) )
			NavigateForward( overview_SFF );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.EndScrollView();
	}

	void Overview_UltimateStatusBar ()
	{
		overview_USB.scrollPosition = EditorGUILayout.BeginScrollView( overview_USB.scrollPosition, false, false, docSize );
		
		/* //// --------------------------- < STATUS BAR POSITIONING > --------------------------- \\\\ */
		EditorGUILayout.LabelField( "Status Bar Positioning", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   The Status Bar Positioning section presents you with two different ways to handle the Ultimate Status Bar's positioning. One will display the Ultimate Status Bar on the screen, while the other allows for following camera rotation within the scene.", paragraphStyle );
		
		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Positioning", itemHeaderStyle );
		EditorGUILayout.LabelField( "The <i>Positioning</i> variable determines whether the Ultimate Status Bar should position itself on the screen or inside the scene facing the camera. Please use the selection below to view more about each option. Of course, the positioning option can also be disabled as well.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUI.BeginChangeCheck();
		positioningOption = ( PositioningOption )EditorGUILayout.EnumPopup( positioningOption );
		if( EditorGUI.EndChangeCheck() )
		{
			overview_PositioningDisabled.target = positioningOption == PositioningOption.Disabled;
			overview_PositioningScreen.target = positioningOption == PositioningOption.ScreenSpace;
			overview_PositioningWorld.target = positioningOption == PositioningOption.WorldSpace;
		}

		GUILayout.Space( paragraphSpace );
		
		if( EditorGUILayout.BeginFadeGroup( overview_PositioningScreen.faded ) )
		{
			EditorGUILayout.LabelField( "The <i>Screen Space</i> option will allow you to set where the status should be displayed on the screen.", paragraphStyle );

			GUILayout.Space( paragraphSpace );

			EditorGUILayout.LabelField( "Scaling Axis", itemHeaderStyle );
			EditorGUILayout.LabelField( "Determines which axis the Rect Transform will be scaled from. If Height is chosen, then the Ultimate Status Bar will scale itself proportionately to the Height of the screen.", paragraphStyle );

			GUILayout.Space( paragraphSpace );

			EditorGUILayout.LabelField( "Status Bar Size", itemHeaderStyle );
			EditorGUILayout.LabelField( "This option changes the overall size of the Ultimate Status Bar on the screen.", paragraphStyle );
				
			GUILayout.Space( paragraphSpace );

			EditorGUILayout.LabelField( "Image Aspect Ratio", itemHeaderStyle );
			EditorGUILayout.LabelField( "This option will allow you to preserve the aspect ratio of the targeted images, so that you will not have to calculate out the dimensions that it must be to look right.", paragraphStyle );
				
			GUILayout.Space( paragraphSpace );

			EditorGUILayout.LabelField( "Status Bar Position", itemHeaderStyle );
			EditorGUILayout.LabelField( "This section of options will help to position the Ultimate Status Bar on the screen exactly where you want it.", paragraphStyle );
		}
		EditorGUILayout.EndFadeGroup();

		if( EditorGUILayout.BeginFadeGroup( overview_PositioningWorld.faded ) )
		{
			EditorGUILayout.LabelField( "The <i>World Space</i> option will allow you to set options to follow the camera in the scene.", paragraphStyle );

			GUILayout.Space( paragraphSpace );

			EditorGUILayout.LabelField( "Find Camera By", itemHeaderStyle );
			EditorGUILayout.LabelField( "This variable will determine how the Ultimate Status Bar finds and follows the Camera that the Ultimate Status Bar should be facing. Each option will present a different variable for how the Camera can be referenced.", paragraphStyle );
		}
		EditorGUILayout.EndFadeGroup();

		if( EditorGUILayout.BeginFadeGroup( overview_PositioningDisabled.faded ) )
		{
			EditorGUILayout.LabelField( "The <i>Disabled</i> option will disable all the positioning functionality of the Ultimate Status Bar. This will allow you to position and scale the Ultimate Status Bar by your own means.", paragraphStyle );
		}
		EditorGUILayout.EndFadeGroup();
		/* \\\\ -------------------------- < END STATUS BAR POSITIONING > --------------------------- //// */

		GUILayout.Space( sectionSpace );

		/* //// ----------------------------- < STATUS BAR OPTIONS > ----------------------------- \\\\ */
		EditorGUILayout.LabelField( "Status Bar Options", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   The Status Bar Options section allows you to set the Ultimate Status Bar's visual states for a smooth look that will help draw attention to your UI when needed.", paragraphStyle );
		
		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Status Bar Icon", itemHeaderStyle );
		EditorGUILayout.LabelField( "If the Status Bar Icon variable is assigned you will have access to not only a color property to change the color, but also the ability to quickly change the icon graphic at runtime using the UpdateStatusBarIcon() function. See the documentation section for more details.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Status Bar Text", itemHeaderStyle );
		EditorGUILayout.LabelField( "If the Status Bar Text variable is assigned, you will be presented with a color property to choose the color of the text and also a string to change the value of the text. You also can use the UpdateStatusBarText() function to update the text value at runtime. Please see the documentation section to learn more.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Update Visibility", itemHeaderStyle );
		EditorGUILayout.LabelField( "The Update Visibility variable determines whether or not the visibility of the Ultimate Status Bar should ever be updated. The Manually option will allow you to manually show and hide the Ultimate Status Bar using the ShowStatusBar() and HideStatusBar() functions For more information about these functions, please see the documentation section.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUI.BeginChangeCheck();
		updateVisibility = ( UpdateVisibility )EditorGUILayout.EnumPopup( updateVisibility );
		if( EditorGUI.EndChangeCheck() )
		{
			overview_VisibilityNever.target = updateVisibility == UpdateVisibility.Never;
			overview_VisibilityManually.target = updateVisibility == UpdateVisibility.Manually;
			overview_VisibilityOnStatusUpdated.target = updateVisibility == UpdateVisibility.OnStatusUpdated;
			overview_UpdateVisibilityOptions.target = updateVisibility != UpdateVisibility.Never;
		}

		GUILayout.Space( paragraphSpace );

		if( EditorGUILayout.BeginFadeGroup( overview_VisibilityNever.faded ) )
		{
			EditorGUILayout.LabelField( "The option <i>Never</i> will never update the visibility of the Ultimate Status Bar. It's worth noting that the ShowStatusBar() and HideStatusBar() functions will no longer work.", paragraphStyle );
		}
		EditorGUILayout.EndFadeGroup();

		if( EditorGUILayout.BeginFadeGroup( overview_VisibilityOnStatusUpdated.faded ) )
		{
			EditorGUILayout.LabelField( "The option <i>OnStatusUpdated</i> will allow the Ultimate Status Bar to communicate internally to each Ultimate Status that is inside of it to see if they have been updated, or if they are at a crucial value that will need to force the Ultimate Status Bar to stay visible.", paragraphStyle );

			GUILayout.Space( paragraphSpace );

			EditorGUILayout.LabelField( "Idle Seconds", itemHeaderStyle );
			EditorGUILayout.LabelField( "Determines the time is seconds before the Ultimate Status Bar will hide itself. This time resets each time a status is updated.", paragraphStyle );
		}
		EditorGUILayout.EndFadeGroup();

		if( EditorGUILayout.BeginFadeGroup( overview_VisibilityManually.faded ) )
		{
			EditorGUILayout.LabelField( "The option <i>Manually</i> will allow you to update the visibility of the Ultimate Status Bar manually using the ShowStatusBar() and HideStatusBar() functions. Please see the documentation section for more information about these functions.", paragraphStyle );
		}
		EditorGUILayout.EndFadeGroup();

		if( EditorGUILayout.BeginFadeGroup( overview_UpdateVisibilityOptions.faded ) )
		{
			GUILayout.Space( paragraphSpace );

			EditorGUILayout.LabelField( "Initial State", itemHeaderStyle );
			EditorGUILayout.LabelField( "The initial visibility of the Ultimate Status Bar. <b>NOTE:</b> If you are using the Animation option, you will need to change the options within the animator to hide the Ultimate Status Bar by default.", paragraphStyle );

			GUILayout.Space( paragraphSpace );

			EditorGUILayout.LabelField( "Update Using", itemHeaderStyle );
			EditorGUILayout.LabelField( "Determines which type of component the Ultimate Status Bar will use to update it's visibility.", paragraphStyle );

			GUILayout.Space( paragraphSpace );

			EditorGUI.BeginChangeCheck();
			updateUsing = ( UpdateUsing )EditorGUILayout.EnumPopup( updateUsing );
			if( EditorGUI.EndChangeCheck() )
			{
				overview_VisibilityFade.target = updateUsing == UpdateUsing.Fade;
				overview_VisibilityAnimation.target = updateUsing == UpdateUsing.Animation;
			}

			GUILayout.Space( paragraphSpace );

			if( EditorGUILayout.BeginFadeGroup( overview_VisibilityFade.faded ) )
			{
				EditorGUILayout.LabelField( "The option <i>Fade</i> will use a CanvasGroup component to update the visibility of the Ultimate Status Bar.", paragraphStyle );

				GUILayout.Space( paragraphSpace );

				EditorGUILayout.LabelField( "Fade Duration", itemHeaderStyle );
				EditorGUILayout.LabelField( "The <i>Fade In Duration</i> and <i>Fade In Duration</i> options allow you to customize how long in seconds it will take for the Ultimate Status Bar to fade from it's current alpha to the target amount.", paragraphStyle );

				GUILayout.Space( paragraphSpace );

				EditorGUILayout.LabelField( "Alpha", itemHeaderStyle );
				EditorGUILayout.LabelField( "The <i>Enabled Alpha</i> and <i>Disabled Alpha</i> options allow for a custom value for the alpha of both the enabled and disabled states of the Ultimate Status Bar.", paragraphStyle );
			}
			if( overview_UpdateVisibilityOptions.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();

			if( EditorGUILayout.BeginFadeGroup( overview_VisibilityAnimation.faded ) )
			{
				EditorGUILayout.LabelField( "The option <i>Animation</i> will use an Animator component to update the visibility of the Ultimate Status Bar.", paragraphStyle );

				GUILayout.Space( paragraphSpace );

				EditorGUILayout.LabelField( "Animator", itemHeaderStyle );
				EditorGUILayout.LabelField( "The Animator component to be used for updating the visibility.", paragraphStyle );
			}
			if( overview_UpdateVisibilityOptions.faded == 1.0f )
				EditorGUILayout.EndFadeGroup();
		}
		EditorGUILayout.EndFadeGroup();
		/* \\\\ -------------------------- < END STATUS BAR OPTIONS > --------------------------- //// */

		GUILayout.Space( sectionSpace );

		/* //// ----------------------------- < STATUS INFORMATION > ----------------------------- \\\\ */
		EditorGUILayout.LabelField( "Status Information", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   The Status Information section is where you will create a customize each of your individual status to be used in your project.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Status Name", itemHeaderStyle );
		EditorGUILayout.LabelField( "The unique name to be used in reference to the particular status. This name must be unique to any other names used on that specific Ultimate Status Bar component.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Status Image", itemHeaderStyle );
		EditorGUILayout.LabelField( "The image component to be used for the particular status. Upon assigning the component the Ultimate Status Bar will set the Image to a filled setting if it is not already done.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Color Mode", itemHeaderStyle );
		EditorGUILayout.LabelField( "The mode in which to display the color of the status to the image component. The <i>Single</i> option will apply a single color to the image, whereas the <i>Gradient</i> option will display the status according to the <i>Status Gradient</i> option.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Test Value", itemHeaderStyle );
		EditorGUILayout.LabelField( "A simple slider to roughly display how the status will behave in the scene.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Display Text", itemHeaderStyle );
		EditorGUILayout.LabelField( "Determines how the status will display text to the user, if at all. The different options will display the values in unique ways. The <i>Additional Text</i> option will allow you to put text before the status value. An example of this would be using \"HP: \" before the Health value of the player.", paragraphStyle );
		
		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Smooth Fill", itemHeaderStyle );
		EditorGUILayout.LabelField( "Smooth Fill can be used to give a nice smooth feel to your status by gradually transitioning from the current to the target value. If this option is selected, you will be presented with another option that you can set for the duration that it will take to get to the target value.", paragraphStyle );
		
		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Fill Constraint", itemHeaderStyle );
		EditorGUILayout.LabelField( "If you have an image that isn't correctly sliced by Unity, or if you have an status image that is circular but not a complete circle, then the <i>Fill Constraint</i> option can help to display the status correctly to the user.", paragraphStyle );
		
		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Keep Visible", itemHeaderStyle );
		EditorGUILayout.LabelField( "If the Ultimate Status Bar is using the <i>On Status Updated</i> option for <i>Update Visibility</i>, then each status will have this variable. If enabled, the status can trigger the Ultimate Status Bar to stay visible if the value of the status is lower than the trigger amount.", paragraphStyle );
		/* //// ----------------------------- < END STATUS INFORMATION > ----------------------------- \\\\ */

		GUILayout.Space( sectionSpace );

		/* //// ----------------------------- < SCRIPT REFERENCE > ----------------------------- \\\\ */
		EditorGUILayout.LabelField( "Script Reference", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   The Script Reference section has useful code snippets that can be used to quickly implement the Ultimate Status Bar into your scene. It is worth noting that this section does not contain each and every function of the Ultimate Status Bar. For information on each function, please refer to the documentation section of this window.", paragraphStyle );
		/* //// ----------------------------- < END SCRIPT REFERENCE > ----------------------------- \\\\ */
		
		EditorGUILayout.EndScrollView();
	}

	void Overview_DramaticStatusFill ()
	{
		overview_DSF.scrollPosition = EditorGUILayout.BeginScrollView( overview_DSF.scrollPosition, false, false, docSize );

		/* //// --------------------------- < STATUS BAR POSITIONING > --------------------------- \\\\ */
		EditorGUILayout.LabelField( "Ultimate Status Bar", itemHeaderStyle );
		EditorGUILayout.LabelField( "The <i>Ultimate Status Bar</i> variable must be assigned in order to customize the Dramatic Status Fill component. This variable supplies the needed information for the Dramatic Status Fill to be customized and function correctly.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Status Name", itemHeaderStyle );
		EditorGUILayout.LabelField( "The <i>Status Name</i> variable allows you to select which status on the Ultimate Status Bar to reference. It's worth noting that this option will only be available when there are more than one Ultimate Status created on the targeted Ultimate Status Bar.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Status Image", itemHeaderStyle );
		EditorGUILayout.LabelField( "The <i>Status Image</i> is the Image component that should be used for the Dramatic Status. This variable must be assigned in order for the script to function correctly.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Status Color", itemHeaderStyle );
		EditorGUILayout.LabelField( "The <i>Status Color</i> allows you to change the color of the Status Image. All changes to the color of the image should be done from this property.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Dramatic Style", itemHeaderStyle );
		EditorGUILayout.LabelField( "The <i>Dramatic Style</i> variable determines what will appear to the user as \"dramatic\".", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUI.BeginChangeCheck();
		followStatus = ( DramaticStyle )EditorGUILayout.EnumPopup( followStatus );
		if( EditorGUI.EndChangeCheck() )
		{
			overview_DSF_StyleIncrease.target = followStatus == DramaticStyle.Increase;
			overview_DSF_StyleDecrease.target = followStatus == DramaticStyle.Decrease;
		}

		GUILayout.Space( paragraphSpace );
		
		if( EditorGUILayout.BeginFadeGroup( overview_DSF_StyleIncrease.faded ) )
		{
			EditorGUILayout.LabelField( "The <i>Increase</i> option will show the target amount of fill from the Ultimate Status. It's worth noting that in order to see this style working, you must be using the <b>Smooth Fill</b> option on the Ultimate Status.", paragraphStyle );
		}
		EditorGUILayout.EndFadeGroup();

		if( EditorGUILayout.BeginFadeGroup( overview_DSF_StyleDecrease.faded ) )
		{
			EditorGUILayout.LabelField( "The <i>Decrease</i> option will display a fill over time from the current amount to the target amount, determined by the selected Ultimate Status.", paragraphStyle );

			GUILayout.Space( paragraphSpace );

			EditorGUILayout.LabelField( "Seconds Delay", itemHeaderStyle );
			EditorGUILayout.LabelField( "Determines how much time is seconds the Dramatic Status should wait before proceeding to the target amount.", paragraphStyle );

			GUILayout.Space( paragraphSpace );

			EditorGUILayout.LabelField( "Reset Sensitivity", itemHeaderStyle );
			EditorGUILayout.LabelField( "This option allows you to set a sensitivity for reseting the Seconds Delay onto the Dramatic Status. This option is useful for times when the Ultimate Status is updated while the Dramatic Status is very close to the previous target fill. This sensitivity will force the Dramatic Status to delay again to draw attention to the new status amount. It's worth noting that this option is only available if the Seconds Delay option is set to greater than zero.", paragraphStyle );
				
			GUILayout.Space( paragraphSpace );

			EditorGUILayout.LabelField( "Fill Speed", itemHeaderStyle );
			EditorGUILayout.LabelField( "This option determines how fast the Dramatic Status image will move towards the target amount of fill. The value is literally amount of fill per second to be applied to the image.", paragraphStyle );
		}
		EditorGUILayout.EndFadeGroup();

		EditorGUILayout.EndScrollView();
	}
	
	void Overview_AlternateStateHandler ()
	{
		overview_ASH.scrollPosition = EditorGUILayout.BeginScrollView( overview_ASH.scrollPosition, false, false, docSize );

		EditorGUILayout.LabelField( "Ultimate Status Bar", itemHeaderStyle );
		EditorGUILayout.LabelField( "The <i>Ultimate Status Bar</i> variable must be assigned in order to customize the Alternate State Handler component. This variable supplies the needed information for the Alternate State Handler to be customized and function properly.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Alternate States", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   The Alternate States section contains information and options about each individual state that has been created.", paragraphStyle );
		
		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "State Name", itemHeaderStyle );
		EditorGUILayout.LabelField( "This variable represents that name that this state will be registered as. The State Name will be the parameter passed through functions in order to reference this particular Alternate State.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Default Color", itemHeaderStyle );
		EditorGUILayout.LabelField( "The <i>Default Color</i> variable is the color that the state will return to once the state is no longer enabled.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Trigger Option", itemHeaderStyle );
		EditorGUILayout.LabelField( "This option determines how the state will be triggered, whether manually or by the percentage of the status.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUI.BeginChangeCheck();
		triggerOption = ( TriggerOption )EditorGUILayout.EnumPopup( triggerOption );
		if( EditorGUI.EndChangeCheck() )
		{
			overview_ASH_TriggerManual.target = triggerOption == TriggerOption.Manual;
			overview_ASH_TriggerPercentage.target = triggerOption == TriggerOption.Percentage;
		}

		GUILayout.Space( paragraphSpace );

		if( EditorGUILayout.BeginFadeGroup( overview_ASH_TriggerManual.faded ) )
		{
			EditorGUILayout.LabelField( "The <i>Manual</i> trigger method requires the user to switch the state manually.", paragraphStyle );
		}
		EditorGUILayout.EndFadeGroup();

		if( EditorGUILayout.BeginFadeGroup( overview_ASH_TriggerPercentage.faded ) )
		{
			EditorGUILayout.LabelField( "The <i>Percentage</i> trigger method will automatically switch states according to the current Ultimate Status percentage. You will also have some options to customize when the state will change.", paragraphStyle );

			GUILayout.Space( paragraphSpace );

			EditorGUILayout.LabelField( "Status Name", itemHeaderStyle );
			EditorGUILayout.LabelField( "The <i>Status Name</i> allows you to set which Ultimate Status this state will makes it's calculations with.", paragraphStyle );

			GUILayout.Space( paragraphSpace );

			EditorGUILayout.LabelField( "Trigger By", itemHeaderStyle );
			EditorGUILayout.LabelField( "The <i>Trigger By</i> option allows you to set how the state will trigger, and the <i>Value</i> option allows you to set at which percentage the state will trigger.", paragraphStyle );
		}
		EditorGUILayout.EndFadeGroup();

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "State Color", itemHeaderStyle );
		EditorGUILayout.LabelField( "This option determines the color of this Alternate State.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "State Type", itemHeaderStyle );
		EditorGUILayout.LabelField( "The <i>State Type</i> option allows you to choose between either a Image or Text component to be used with this Alternate State.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Flashing", itemHeaderStyle );
		EditorGUILayout.LabelField( "The Flashing option will make the state flashing between two colors. The options available allow you to customize the <i>Flashing Color</i> for the extra color to be used, <i>Flashing Speed</i> to control the speed of the flash, and a <i>Flashing Duration</i> if you want the state flash to only last a certain amount of time.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Script Reference", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   The Script Reference section has useful code snippets that can be used to quickly implement the Alternate State Handler into your scene. For information on each function, please refer to the documentation section of this window.", paragraphStyle );

		EditorGUILayout.EndScrollView();
	}

	void Overview_StatusFillFollower ()
	{
		overview_SFF.scrollPosition = EditorGUILayout.BeginScrollView( overview_SFF.scrollPosition, false, false, docSize );

		/* //// --------------------------- < STATUS BAR POSITIONING > --------------------------- \\\\ */
		EditorGUILayout.LabelField( "Ultimate Status Bar", itemHeaderStyle );
		EditorGUILayout.LabelField( "The <i>Ultimate Status Bar</i> variable must be assigned in order to customize the Status Fill Follower component. This variable supplies the needed information for the Status Fill Follower to be customized and function properly.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "State Name", itemHeaderStyle );
		EditorGUILayout.LabelField( "The <i>Status Name</i> variable allows you to select which status on the Ultimate Status Bar to follow. It's worth noting that this option will only be available when there are more than one Ultimate Status created on the targeted Ultimate Status Bar.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Scale Direction", itemHeaderStyle );
		EditorGUILayout.LabelField( "This option determines which axis should be referenced for the scale of the image component.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Image Size", itemHeaderStyle );
		EditorGUILayout.LabelField( "The size of the image that will follow the status.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Image Aspect Ratio", itemHeaderStyle );
		EditorGUILayout.LabelField( "This option determines whether the ratio of the image should be customized or calculated.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Position Anchors", itemHeaderStyle );
		EditorGUILayout.LabelField( "This section allows you to set the <i>Minimum</i> and <i>Maximum</i> positions to be used for following the status. The <b>Set</b> buttons will set the corresponding position at the point where the image component is. The <b>Edit In Scene</b> check box will present you with an Axis Gizmo to edit the position in the scene. Please note that while the Set buttons will warn you about overwriting previous saved positions, the Edit In Scene option will automatically overwrite the previous saved positions when moving the Gizmo.", paragraphStyle );

		EditorGUILayout.EndScrollView();
	}

	void DocumentationPage ()
	{
		documentation.scrollPosition = EditorGUILayout.BeginScrollView( documentation.scrollPosition, false, false, docSize );

		EditorGUILayout.LabelField( "Ultimate Status", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   The Ultimate Status class is located inside of the Ultimate Status Bar script. Each status that is displayed on the Ultimate Status Bar inspector is a implementation of the Ultimate Status class. Click <b>More Info</b> below for information on the public functions that available for use through the Ultimate Status class.", paragraphStyle );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "More Info", buttonSize ) )
			NavigateForward( documentation_US );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "Ultimate Status Bar", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   The Ultimate Status Bar is the key component to display any sort of status in your scene. It can display anything from your Player, to the NPC's in your scene, to the Loading Bar when starting your game. Click <b>More Info</b> below for information on the public functions that available for use through the Ultimate Status Bar class.", paragraphStyle );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "More Info", buttonSize ) )
			NavigateForward( documentation_USB );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "Alternate State Handler", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   The Alternate State Handler component allows you to display different states for each status in your scene. This can be done using images or text. Click <b>More Info</b> below to find out more about the public functions that you have available to you through the Alternate State Handler class.", paragraphStyle );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "More Info", buttonSize ) )
			NavigateForward( documentation_ASH );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.EndScrollView();
	}

	void Documentation_UltimateStatus ()
	{
		documentation_US.scrollPosition = EditorGUILayout.BeginScrollView( documentation_US.scrollPosition, false, false, docSize );

		/* //// --------------------------- < PUBLIC FUNCTIONS > --------------------------- \\\\ */
		EditorGUILayout.LabelField( "Public Functions", sectionHeaderStyle );

		GUILayout.Space( paragraphSpace );
		// UpdateStatus
		EditorGUILayout.LabelField( "UpdateStatus( float currentValue, float maxValue )", itemHeaderStyle );
		EditorGUILayout.LabelField( "This function will update the values of the status in order to display them to the user. This function has two parameters that need to be passed into it. The <i>currentValue</i> should be the current amount of the targeted status, whereas the <i>maxValue</i> should be the maximum amount that the status can be. These values must be passed into the function in order to correctly display them to the user. Using these values, the Ultimate Status will calculate out the percentage values and then display this information to the user according to the options set in the inspector.", paragraphStyle );

		GUILayout.Space( paragraphSpace );
		// GetCurrentFraction
		EditorGUILayout.LabelField( "GetCurrentFraction", itemHeaderStyle );
		EditorGUILayout.LabelField( "The GetCurrentFraction property will return the percentage value that was calculated when the status was updated. This number will not be current with the Smooth Fill option.", paragraphStyle );

		GUILayout.Space( paragraphSpace );
		// GetMaxValue
		EditorGUILayout.LabelField( "GetMaxValue", itemHeaderStyle );
		EditorGUILayout.LabelField( "The GetMaxValue property will return the last known maximum value passed through the UpdateStatus function.", paragraphStyle );

		GUILayout.Space( paragraphSpace );
		// GetTargetFill
		EditorGUILayout.LabelField( "GetTargetFill", itemHeaderStyle );
		EditorGUILayout.LabelField( "The GetTargetFill property will return the target amount of fill for the image. This is used by other classes, such as the Dramatic Status Fill.", paragraphStyle );
		
		GUILayout.Space( paragraphSpace );
		// GetCurrentCalculatedFraction
		EditorGUILayout.LabelField( "GetCurrentCalculatedFraction", itemHeaderStyle );
		EditorGUILayout.LabelField( "The GetCurrentCalculatedFraction property will return the current percentage of the status. This value is current with the Smooth Fill or Fill Constraint options.", paragraphStyle );

		GUILayout.Space( paragraphSpace );
		// UpdateStatusColor
		EditorGUILayout.LabelField( "UpdateStatusColor( Color targetColor )", itemHeaderStyle );
		EditorGUILayout.LabelField( "This function will update the status color variable and apply the color immediately to the status image.", paragraphStyle );

		GUILayout.Space( paragraphSpace );
		// UpdateStatusTextColor
		EditorGUILayout.LabelField( "UpdateStatusTextColor( Color targetColor )", itemHeaderStyle );
		EditorGUILayout.LabelField( "This function will update the associated Text component's color value with the Color parameter.", paragraphStyle );

		EditorGUILayout.EndScrollView();
	}

	void Documentation_UltimateStatusBar ()
	{
		documentation_USB.scrollPosition = EditorGUILayout.BeginScrollView( documentation_USB.scrollPosition, false, false, docSize );

		/* //// --------------------------- < PUBLIC FUNCTIONS > --------------------------- \\\\ */
		EditorGUILayout.LabelField( "Public Functions", sectionHeaderStyle );

		GUILayout.Space( itemHeaderSpace );
		
		EditorGUILayout.LabelField( "UpdateStatus( float currentValue, float maxValue )", itemHeaderStyle );
		GUILayout.Space( paragraphSpace );
		EditorGUILayout.LabelField( "This function will call the default status on the targeted Ultimate Status Bar. It updates the values of the status in order to display them to the user. This function has two parameters that need to be passed into it. The <i>currentValue</i> should be the current amount of the targeted status, whereas the <i>maxValue</i> should be the maximum amount that the status can be. These values must be passed into the function in order to correctly display them to the user.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );
		
		EditorGUILayout.LabelField( "UpdateStatus( string statusName, float currentValue, float maxValue )", itemHeaderStyle );
		GUILayout.Space( paragraphSpace );
		EditorGUILayout.LabelField( "This function will call the targeted Ultimate Status that has been registered with the <i>statusName</i> parameter. It updates the values of the status in order to display them to the user. The <i>currentValue</i> should be the current amount of the targeted status, whereas the <i>maxValue</i> should be the maximum amount that the status can be. These values must be passed into the function in order to correctly display them to the user.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "GetStatusBarState", itemHeaderStyle );
		GUILayout.Space( paragraphSpace );
		EditorGUILayout.LabelField( "Returns the current state of the Ultimate Status Bar visibility.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );
		
		EditorGUILayout.LabelField( "UpdatePositioning()", itemHeaderStyle );
		GUILayout.Space( paragraphSpace );
		EditorGUILayout.LabelField( "This function updates the size and positioning of the Ultimate Status Bar on the screen. It's worth noting that this function will only work in the Screen Space option is selected for the Positioning setting.", paragraphStyle );
		
		GUILayout.Space( itemHeaderSpace );
		
		EditorGUILayout.LabelField( "UpdateStatusBarIcon( Sprite newIcon )", itemHeaderStyle );
		GUILayout.Space( paragraphSpace );
		EditorGUILayout.LabelField( "This function updates the icon image associated with the Ultimate Status Bar with the <i>newIcon</i> sprite parameter.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );
		
		EditorGUILayout.LabelField( "UpdateStatusBarText( string newText )", itemHeaderStyle );
		GUILayout.Space( paragraphSpace );
		EditorGUILayout.LabelField( "This function will update the text component to display the <i>newText</i> string parameter.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );
		
		EditorGUILayout.LabelField( "GetUltimateStatus( string statusName )", itemHeaderStyle );
		GUILayout.Space( paragraphSpace );
		EditorGUILayout.LabelField( "This function returns the Ultimate Status class that has been registered with the <i>statusName</i> parameter.", paragraphStyle );

		GUILayout.Space( sectionSpace );
		
		/* //// --------------------------- < STATIC FUNCTIONS > --------------------------- \\\\ */
		EditorGUILayout.LabelField( "Static Functions", sectionHeaderStyle );

		GUILayout.Space( paragraphSpace );
		EditorGUILayout.LabelField( "   All static functions require a string to be passed through the function first. The <i>statusBarName</i> parameter is used to locate the targeted Ultimate Status Bar from a static list of Ultimate Status Bars that has been stored.", paragraphStyle );

		GUILayout.Space( paragraphSpace );
		
		EditorGUILayout.LabelField( "UpdateStatus( string statusBarName, string statusName, float currentValue, float maxValue )", itemHeaderStyle );
		EditorGUILayout.LabelField( "This function will update the targeted Ultimate Status that has been registered on the Ultimate Status Bar component. See the UpdateStatus() function inside the Ultimate Status documentation for more details.", paragraphStyle );

		GUILayout.Space( paragraphSpace );
		
		EditorGUILayout.LabelField( "UpdateStatusBarIcon( string statusBarName, Sprite newIcon )", itemHeaderStyle );
		EditorGUILayout.LabelField( "Calls the UpdateStatusBarIcon() function of the targeted Ultimate Status Bar. See the public function, UpdateStatusBarIcon(), for more details.", paragraphStyle );
		
		GUILayout.Space( paragraphSpace );
		
		EditorGUILayout.LabelField( "UpdateStatusBarText( string statusBarName, string newText )", itemHeaderStyle );
		EditorGUILayout.LabelField( "Calls the UpdateStatusBarText() function of the targeted Ultimate Status Bar. See the public function, UpdateStatusBarText(), for more details.", paragraphStyle );
		
		GUILayout.Space( paragraphSpace );
		
		EditorGUILayout.LabelField( "GetUltimateStatusBar( string statusBarName )", itemHeaderStyle );
		EditorGUILayout.LabelField( "This function will return the Ultimate Status Bar component that has been registered with the <i>statusBarName</i> parameter.", paragraphStyle );

		EditorGUILayout.EndScrollView();
	}
	
	void Documentation_AlternateStateHandler ()
	{
		documentation_USB.scrollPosition = EditorGUILayout.BeginScrollView( documentation_USB.scrollPosition, false, false, docSize );

		/* //// --------------------------- < PUBLIC FUNCTIONS > --------------------------- \\\\ */
		EditorGUILayout.LabelField( "Public Functions", sectionHeaderStyle );

		GUILayout.Space( itemHeaderSpace );
		
		EditorGUILayout.LabelField( "SwitchState( string stateName, bool state )", itemHeaderStyle );
		GUILayout.Space( paragraphSpace );
		EditorGUILayout.LabelField( "This function switches the targeted Alternate State to the desired state. The <i>stateName</i> parameter will allow the script to find that specific state in order to switch it.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );
		
		EditorGUILayout.LabelField( "GetAlternateState( string stateName )", itemHeaderStyle );
		GUILayout.Space( paragraphSpace );
		EditorGUILayout.LabelField( "This function will return the AlternateState class that has been registered with the <i>stateName</i> parameter.", paragraphStyle );

		GUILayout.Space( sectionSpace );
		
		/* //// --------------------------- < STATIC FUNCTIONS > --------------------------- \\\\ */
		EditorGUILayout.LabelField( "Static Functions", sectionHeaderStyle );

		GUILayout.Space( paragraphSpace );
		EditorGUILayout.LabelField( "   All static functions require a string to be passed through the function first. Each Alternate State Handler is registered with the targeted Ultimate Status Bar's name. The <i>statusBarName</i> parameter is used to locate the targeted Alternate State Handler from a static list of Alternate State Handler classes that have been stored.", paragraphStyle );

		GUILayout.Space( paragraphSpace );
		
		EditorGUILayout.LabelField( "SwitchState( string statusBarName, string stateName, bool state )", itemHeaderStyle );
		EditorGUILayout.LabelField( "This function switches the targeted Alternate State to the desired state. The <i>statusBarName</i> parameter will allow the script to find the specific Alternate State Handler that has been registered with the name of the Ultimate Status Bar that it is associated with. The <i>stateName</i> parameter will allow the script to find that specific state in order to switch it.", paragraphStyle );

		GUILayout.Space( paragraphSpace );
		
		EditorGUILayout.LabelField( "GetAlternateStateHandler( string statusBarName )", itemHeaderStyle );
		EditorGUILayout.LabelField( "This function returns the AlternateStateHandler class that has been registered with the <i>statusBarName</i> parameter. It's worth noting that the Alternate State Handler will be registered with the name of the Ultimate Status Bar that it is associated with.", paragraphStyle );

		EditorGUILayout.EndScrollView();
	}
	
	void Extras ()
	{
		extras.scrollPosition = EditorGUILayout.BeginScrollView( extras.scrollPosition, false, false, docSize );

		EditorGUILayout.LabelField( "Videos", sectionHeaderStyle );
		EditorGUILayout.LabelField( "   The links below are to the collection of videos that we have made in connection with the Ultimate Status Bar. The Tutorial Videos are designed to get the Ultimate Status Bar implemented into your project as fast as possible, and give you a good understanding of what you can achieve using it in your projects, whereas the demonstrations are videos showing how we, and others in the Unity community, have used assets created by Tank & Healer Studio in our projects.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Tutorials", buttonSize ) )
			Application.OpenURL( "https://www.youtube.com/playlist?list=PL7crd9xMJ9Tl0VRLpo3VoU2U-SbLgwB3-" );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Demonstrations", buttonSize ) )
			Application.OpenURL( "https://www.youtube.com/playlist?playnext=1&list=PL7crd9xMJ9TlkjepDAY_GnpA1CX-rFltz" );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndScrollView();
	}
	
	void OtherProducts ()
	{
		otherProducts.scrollPosition = EditorGUILayout.BeginScrollView( otherProducts.scrollPosition, false, false, docSize );

		/* ------------ < ULTIMATE JOYSTICK > ------------ */
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Space( 15 );
		GUILayout.Label( ujPromo, GUILayout.Width( 250 ), GUILayout.Height( 125 ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Ultimate Joystick", sectionHeaderStyle );

		EditorGUILayout.LabelField( "   The Ultimate Joystick is a simple, yet powerful tool for the development of your mobile games. The Ultimate Joystick was created with the goal of giving Unity Developers an incredibly versatile joystick solution, while being extremely easy to implement into existing, or new scripts. You don't need to be a programmer to work with the Ultimate Joystick, and it is very easy to implement into any type of character controller that you need. Additionally, Ultimate Joystick's source code is extremely well commented, easy to modify, and has complete documentation, making it ideal for game-specific adjustments. All in all, with Ultimate Joystick you can't go wrong!", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "More Info", buttonSize ) )
			Application.OpenURL( "http://www.tankandhealerstudio.com/ultimate-joystick.html" );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		/* -------------- < END ULTIMATE JOYSTICK > --------------- */

		GUILayout.Space( 25 );

		/* -------------- < ULTIMATE BUTTON > -------------- */
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Space( 15 );
		GUILayout.Label( ubPromo, GUILayout.Width( 250 ), GUILayout.Height( 125 ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Ultimate Button", sectionHeaderStyle );

		EditorGUILayout.LabelField( "   Buttons are a core element of UI, and as such they should be easy to customize and implement. The Ultimate Button is the embodiment of that very idea. This code package takes the best of Unity's Input and UnityEvent methods and pairs it with exceptional customization to give you the most versatile button for your mobile project. Are you in need of a button for attacking, jumping, shooting, or all of the above? With Ultimate Button's easy size and placement options, style options, and touch actions, you'll have everything you need to create your custom buttons, whether they are simple or complex.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "More Info", buttonSize ) )
			Application.OpenURL( "http://www.tankandhealerstudio.com/ultimate-button.html" );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		/* ------------ < END ULTIMATE BUTTON > ------------ */

		EditorGUILayout.EndScrollView();
	}
	
	void Feedback ()
	{
		feedback.scrollPosition = EditorGUILayout.BeginScrollView( feedback.scrollPosition, false, false, docSize );

		EditorGUILayout.LabelField( "Having Problems?", sectionHeaderStyle );

		EditorGUILayout.LabelField( "   If you experience any issues with the Ultimate Status Bar, please send us an email right away! We will lend any assistance that we can to resolve any issues that you have.\n\n<b>Support Email:</b>", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.SelectableLabel( "tankandhealerstudio@outlook.com", itemHeaderStyle, GUILayout.Height( 15 ) );

		GUILayout.Space( 25 );

		EditorGUILayout.LabelField( "Good Experiences?", sectionHeaderStyle );

		EditorGUILayout.LabelField( "   If you have appreciated how easy the Ultimate Status Bar is to get into your project, leave us a comment and rating on the Unity Asset Store. We are very grateful for all positive feedback that we get.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Rate Us", buttonSize ) )
			Application.OpenURL( "https://www.assetstore.unity3d.com/en/#!/content/48320" );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( 25 );

		EditorGUILayout.LabelField( "Show Us What You've Done!", sectionHeaderStyle );

		EditorGUILayout.LabelField( "   If you have used any of the assets created by Tank & Healer Studio in your project, we would love to see what you have done. Contact us with any information on your game and we will be happy to support you in any way that we can!\n\n<b>Contact Us:</b>", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.SelectableLabel( "tankandhealerstudio@outlook.com" , itemHeaderStyle, GUILayout.Height( 15 ) );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField( "Happy Game Making,\n	-Tank & Healer Studio", paragraphStyle, GUILayout.Height( 30 ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( 25 );

		EditorGUILayout.EndScrollView();
	}
	
	void ChangeLog()
	{
		changeLog.scrollPosition = EditorGUILayout.BeginScrollView( changeLog.scrollPosition, false, false, docSize );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Future Updates", buttonSize ) )
			NavigateForward( plannedFeatures );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "Version 2.1.1", itemHeaderStyle );
		EditorGUILayout.LabelField( "  • Minor editor fixes.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "Version 2.1", itemHeaderStyle );
		EditorGUILayout.LabelField( "  • Completely removed the Ultimate Status Bar Controller script.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Complete revamp of the Ultimate Status Bar class.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Added three new classes to enhance the visual aspects of the Ultimate Status Bar.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Added three new simple example scenes to show the three new classes in action.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Removed all example files from the Plugins folder.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Created a new folder named <i>Ultimate Status Bar Examples</i> which will contain all the example files for the project.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Added new example scene: Asteroids.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Removed previous example scene from the download.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Drastically improved the functionality of the Ultimate Status Bar Documentation Window.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Renamed the <i>Textures</i> folder to <i>Sprites</i>.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Removed all previous textures from the download.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Added 25 new complete Ultimate Status Bar textures.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Created a new folder name <i>Prefabs</i> that contains prefabs of all the included Ultimate Status Bar's.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Improved overall performance of the Ultimate Status Bar script.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Greatly enhanced editor functionality for all scripts.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "Version 2.0.3", itemHeaderStyle );
		EditorGUILayout.LabelField( "  • Improved timeout functionality.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Minor changes to the editor scripts.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "Version 2.0.2", itemHeaderStyle );
		EditorGUILayout.LabelField( "  • Minor fixes to the editor scripts.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "Version 2.0.1", itemHeaderStyle );
		EditorGUILayout.LabelField( "  • Minor editor window fix.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "Version 2.0", itemHeaderStyle );
		EditorGUILayout.LabelField( "  • Added a new in-engine documentation window.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Removed Javascript scripts to improve script reference functionality.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Reorganized folder structure.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Added animation timeout options.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Improved editor functionality.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Added more static reference functionality to the UltimateStatusBar.cs script.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Added static reference functionality to the UltimateStatusBarController.cs script.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Improved overall performance.", paragraphStyle );

		EditorGUILayout.EndScrollView();
	}

	void PlannedFeatures ()
	{
		changeLog.scrollPosition = EditorGUILayout.BeginScrollView( changeLog.scrollPosition, false, false, docSize );

		EditorGUILayout.LabelField( "  • Add new textures using the 'Minimalist' style.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Include more Fill Follower images to be used.", paragraphStyle );
		
		EditorGUILayout.EndScrollView();
	}

	void ThankYou ()
	{
		thankYou.scrollPosition = EditorGUILayout.BeginScrollView( thankYou.scrollPosition, false, false, docSize );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "We here at Tank & Healer Studio would like to thank you for purchasing the Ultimate Status Bar asset package from the Unity Asset Store. If you have any questions about this product please don't hesitate to contact us at: ", paragraphStyle );

		GUILayout.Space( paragraphSpace );
		EditorGUILayout.SelectableLabel( "tankandhealerstudio@outlook.com" , itemHeaderStyle, GUILayout.Height( 15 ) );
		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "We hope that the Ultimate Status Bar will be a great help to you in the development of your game. After pressing the continue button below, you will be presented with helpful information on this asset to assist you in implementing it into your project.", paragraphStyle );

		GUILayout.Space( sectionSpace );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField( "Happy Game Making,\n	-Tank & Healer Studio", paragraphStyle, GUILayout.Height( 30 ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( 15 );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Continue", buttonSize ) )
			NavigateBack();
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndScrollView();
	}
	
	void VersionChanges ()
	{
		versionChanges.scrollPosition = EditorGUILayout.BeginScrollView( versionChanges.scrollPosition, false, false, docSize );

		GUILayout.Space( itemHeaderSpace );
		
		EditorGUILayout.LabelField( "  Thank you for downloading the most recent version of the Ultimate Status Bar. This most recent update was huge, and there were many things that were updated, and some things were completely removed! Please check out the sections below to see all the important changes that have been made. As always, if you run into any issues with the Ultimate Status Bar, please contact us at:", paragraphStyle );

		GUILayout.Space( paragraphSpace );
		EditorGUILayout.SelectableLabel( "tankandhealerstudio@outlook.com", itemHeaderStyle, GUILayout.Height( 15 ) );
		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "MAJOR CHANGES", sectionHeaderStyle );
		EditorGUILayout.LabelField( "  • Completely removed the Ultimate Status Bar Controller class. The functionality of this class has been put inside the Ultimate Status Bar class.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Completely revamped the Ultimate Status Bar class to contain the main bulk of the functionality for the package.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Added three new classes to enhance the visual aspects of the Ultimate Status Bar. These classes are explained in a little more detail below.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "GENERAL CHANGES", sectionHeaderStyle );
		EditorGUILayout.LabelField( "  • Removed example files from the Plugins folder. All example files will now be in the folder named: <i>Ultimate Status Bar Examples</i>. Please note that this folder should always be deleted after the example files have helped you to understand the product. These files are not meant to be used in your projects as they are simply examples.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Added new example scene: Asteroids.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Removed the previous example scene.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Drastic improvements to the Ultimate Status Bar Documentation Window.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "TEXTURE CHANGES", sectionHeaderStyle );
		EditorGUILayout.LabelField( "  • Removed all previous textures from the download.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Renamed the <i>Textures</i> folder to <i>Sprites</i>.", paragraphStyle );
		EditorGUILayout.LabelField( "  • Added 25 new textures that are available for use in your projects.", paragraphStyle );
		
		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "NEW CLASSES", sectionHeaderStyle );
		EditorGUILayout.LabelField( "  Some new classes were added to help enhance the functionality of the Ultimate Status Bar. For information on what you have available to you, please refer to the Overview section of this help window.", paragraphStyle );
		EditorGUILayout.LabelField( "  • <b>Ultimate Status</b> - This class is located inside of the Ultimate Status Bar script. It stores all the information for each individual status that is created.", paragraphStyle );
		EditorGUILayout.LabelField( "  • <b>Dramatic Status Fill</b> - This class uses a second image to display a dramatic effect to the user. This component helps to draw attention to the change in status amount.", paragraphStyle );
		EditorGUILayout.LabelField( "  • <b>Alternate State Handler</b> - This class allows you to display different states for each status in your scene. This can be done using images or text.", paragraphStyle );
		EditorGUILayout.LabelField( "  • <b>Status Fill Follower</b> - This class can be used to follow the current fill of an image to draw attention to the amount that it is at.", paragraphStyle );
		
		GUILayout.Space( sectionSpace );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField( "Happy Game Making,\n	-Tank & Healer Studio", paragraphStyle, GUILayout.Height( 30 ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( 15 );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Continue", buttonSize ) )
			NavigateBack();
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndScrollView();
	}
	
	[InitializeOnLoad]
	class UltimateStatusBarInitialLoad
	{
		static UltimateStatusBarInitialLoad ()
		{
			// If the user has a older version of USB that used the bool for startup...
			if( EditorPrefs.HasKey( "UltimateStatusBarStartup" ) && !EditorPrefs.HasKey( "UltimateStatusBarVersion" ) )
			{
				// Set the new pref to 0 so that the pref will exist and the version changes will be shown.
				EditorPrefs.SetInt( "UltimateStatusBarVersion", 0 );
			}

			// If this is the first time that the user has downloaded the Ultimate Status Bar...
			if( !EditorPrefs.HasKey( "UltimateStatusBarVersion" ) )
			{
				// Set the current menu to the thank you page.
				NavigateForward( thankYou );

				// Set the version to current so they won't see these version changes.
				EditorPrefs.SetInt( "UltimateStatusBarVersion", importantChanges );

				EditorApplication.update += WaitForCompile;
			}
			else if( EditorPrefs.GetInt( "UltimateStatusBarVersion" ) < importantChanges )
			{
				// Set the current menu to the version changes page.
				NavigateForward( versionChanges );

				// Set the version to current so they won't see this page again.
				EditorPrefs.SetInt( "UltimateStatusBarVersion", importantChanges );

				EditorApplication.update += WaitForCompile;
			}
		}

		static void WaitForCompile ()
		{
			if( EditorApplication.isCompiling )
				return;

			EditorApplication.update -= WaitForCompile;
			
			InitializeWindow();
		}
	}
}